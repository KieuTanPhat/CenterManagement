using CenterManagement.API.Services;
using CenterManagement.DataAccess.Data;
using CenterManagement.Models.DTOs;
using CenterManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace CenterManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly CenterManagementDBContext _context;

        public AuthController(TokenService tokenService,
                              CenterManagementDBContext context)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Xác thực user từ DB
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Sai tài khoản hoặc mật khẩu");

            if (!user.IsActive)
                return Unauthorized("Tài khoản đã bị khóa");

            if (user.RoleId is < 1 or > 4)
                return Unauthorized("Học viên không được đăng nhập hệ thống quản trị");

            var refreshToken = _tokenService.GenerateRefreshToken();
            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.FullName, user.UserName, user.RoleId);
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                JwtId  = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7) // 7 ngày
            };
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15)
            });
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
            if (principal == null)
                return Unauthorized("Access token không hợp lệ");

            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == dto.RefreshToken
                                       && r.UserId == userId);
            var jwtId = principal
        .FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (storedToken == null
                || storedToken.IsUsed
                || storedToken.IsRevoked
                || storedToken.ExpiryDate < DateTime.UtcNow
                || storedToken.JwtId != jwtId)
            {
                return Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn");
            }

            storedToken.IsUsed = true;
            storedToken.IsRevoked = true;

            var user = await _context.Users.FindAsync(userId);
            var newAccessToken = _tokenService.GenerateAccessToken(
                user!.Id, user.FullName, user.UserName, user.RoleId);
            var newRefreshToken = _tokenService.GenerateRefreshToken();


            var handler = new JwtSecurityTokenHandler();

            var jwtToken = handler.ReadJwtToken(newAccessToken);

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = userId,
                JwtId = jwtToken.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            });
            await _context.SaveChangesAsync();

            return Ok(new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15)
            });
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Revoke()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Thu hồi tất cả refresh token của user (logout)
            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && r.IsRevoked == false)
                .ToListAsync();

            tokens.ForEach(t => t.IsRevoked = true);
            await _context.SaveChangesAsync();

            return Ok("Đã đăng xuất");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto.RoleId is < 1 or > 4)
                return BadRequest("Chỉ cho phép tạo tài khoản quản trị, quản lý, nhân viên hoặc giáo viên");

            var checkUserName = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.Username);

            var checkEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (checkUserName != null || checkEmail != null) { 
                return BadRequest("Tên đăng nhập hoặc email đã tồn tại");
            }
            _context.Users.Add(new Models.Entities.User
            {
                UserName = dto.Username,
                Email = dto.Email,
                FullName = dto.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = dto.RoleId,
                IsActive = true
            });
            var rolename = await _context.Roles
                .Where(r => r.Id == dto.RoleId).Select(r => r.RoleName)
                .FirstOrDefaultAsync();
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đăng ký thành công", role = rolename });
        }
    }
}
