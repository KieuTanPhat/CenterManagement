using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CenterManagement.Models.DTOs;
namespace CenterManagement.API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config)
        {
            _config = config;
        }
        public string GenerateAccessToken(int id, string fullname, string username, int roleId)
        {
            var jwtKey = _config["AppSettings:Jwt:Key"];

            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("Jwt:Key is missing in appsettings");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim("fullName", fullname),
                new Claim(ClaimTypes.Role, roleId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var Expire = DateTime.UtcNow.AddMinutes(
            Convert.ToDouble(_config["AppSettings:Jwt:ExpireMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _config["AppSettings:Jwt:Issuer"],
                audience: _config["AppSettings:Jwt:Audience"],
                claims: claims,
                expires: Expire,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.UTF8.GetBytes(_config["AppSettings:Jwt:Key"]!);

            var validation = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["AppSettings:Jwt:Issuer"],
                ValidAudience = _config["AppSettings:Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, validation, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) // hs256 Hs256 hS256
            {
                throw new SecurityTokenException("Token không hợp lệ");
            }

            return principal;
        }
    }
}
