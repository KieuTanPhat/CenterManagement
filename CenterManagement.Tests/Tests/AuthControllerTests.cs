using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using CenterManagement.API.Controllers;
using CenterManagement.API.Services;
using CenterManagement.DataAccess.Data;
using CenterManagement.Models.DTOs;
using CenterManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CenterManagement.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly CenterManagementDBContext _context;
        private readonly TokenService _tokenService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<CenterManagementDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CenterManagementDBContext(options);

            // Setup mock IConfiguration
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["AppSettings:Jwt:Key"])
                .Returns("your-super-secret-key-that-must-be-at-least-32-characters-long!");
            mockConfig.Setup(x => x["AppSettings:Jwt:Issuer"])
                .Returns("CenterManagementAPI");
            mockConfig.Setup(x => x["AppSettings:Jwt:Audience"])
                .Returns("CenterManagementUsers");
            mockConfig.Setup(x => x["AppSettings:Jwt:ExpireMinutes"])
                .Returns("15");

            _tokenService = new TokenService(mockConfig.Object);
            _controller = new AuthController(_tokenService, _context);
        }

        // Test 1: Register with valid data
        [Fact]
        public async Task Register_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                Password = "Password123!"
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "testuser");
            Assert.NotNull(user);
            Assert.Equal("test@example.com", user.Email);
        }

        // Test 2: Register with duplicate username
        [Fact]
        public async Task Register_WithDuplicateUsername_ShouldReturnBadRequest()
        {
            // Arrange
            _context.Users.Add(new User
            {
                UserName = "duplicate",
                Email = "user1@example.com",
                FullName = "User One",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                RoleId = 2,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var registerDto = new RegisterDto
            {
                Username = "duplicate",
                Email = "different@example.com",
                FullName = "User Two",
                Password = "password"
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Test 3: Register with duplicate email
        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
        {
            // Arrange
            _context.Users.Add(new User
            {
                UserName = "user1",
                Email = "duplicate@example.com",
                FullName = "User One",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                RoleId = 2,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var registerDto = new RegisterDto
            {
                Username = "different",
                Email = "duplicate@example.com",
                FullName = "User Two",
                Password = "password"
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Test 4: Login with valid credentials
        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnTokens()
        {
            // Arrange
            var password = "Password123!";
            _context.Users.Add(new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = 2,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto { Username = "testuser", Password = password };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TokenResponseDto>(okResult.Value);

            Assert.NotEmpty(response.AccessToken);
            Assert.NotEmpty(response.RefreshToken);
        }

        // Test 5: Login with invalid username
        [Fact]
        public async Task Login_WithInvalidUsername_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "nonexistent", Password = "password" };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // Test 6: Login with wrong password
        [Fact]
        public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            _context.Users.Add(new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!"),
                RoleId = 2,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto { Username = "testuser", Password = "WrongPassword123!" };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // Test 7: Login should create refresh token in DB
        [Fact]
        public async Task Login_ShouldSaveRefreshTokenInDatabase()
        {
            // Arrange
            var password = "Password123!";
            var user = new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = 2,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto { Username = "testuser", Password = password };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TokenResponseDto>(okResult.Value);

            var refreshTokenInDb = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == response.RefreshToken);

            Assert.NotNull(refreshTokenInDb);
            Assert.Equal(user.Id, refreshTokenInDb.UserId);
            Assert.False(refreshTokenInDb.IsUsed);
            Assert.False(refreshTokenInDb.IsRevoked);
        }

        // Test 8: Refresh token with valid token
        [Fact]
        public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
        {
            // Arrange - Login first
            var password = "Password123!";
            _context.Users.Add(new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = 2,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var loginResult = Assert.IsType<OkObjectResult>(await _controller.Login(
                new LoginDto { Username = "testuser", Password = password }));
            var loginResponse = Assert.IsType<TokenResponseDto>(loginResult.Value);

            var refreshDto = new RefreshTokenRequestDto
            {
                AccessToken = loginResponse.AccessToken,
                RefreshToken = loginResponse.RefreshToken
            };

            // Act
            var result = await _controller.RefreshToken(refreshDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TokenResponseDto>(okResult.Value);

            Assert.NotEmpty(response.AccessToken);
            Assert.NotEmpty(response.RefreshToken);
            Assert.NotEqual(loginResponse.AccessToken, response.AccessToken);
            Assert.NotEqual(loginResponse.RefreshToken, response.RefreshToken);
        }

        // Test 9: Refresh token with invalid refresh token
        [Fact]
        public async Task RefreshToken_WithInvalidRefreshToken_ShouldReturnUnauthorized()
        {
            // Arrange - Login first to get access token
            var password = "Password123!";
            _context.Users.Add(new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = 2,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var loginResult = Assert.IsType<OkObjectResult>(await _controller.Login(
                new LoginDto { Username = "testuser", Password = password }));
            var loginResponse = Assert.IsType<TokenResponseDto>(loginResult.Value);

            var refreshDto = new RefreshTokenRequestDto
            {
                AccessToken = loginResponse.AccessToken,
                RefreshToken = "invalid.refresh.token"
            };

            // Act
            var result = await _controller.RefreshToken(refreshDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // Test 10: Old refresh token should be marked as used after refresh
        [Fact]
        public async Task RefreshToken_ShouldMarkOldTokenAsUsed()
        {
            // Arrange
            var password = "Password123!";
            _context.Users.Add(new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = 2,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var loginResult = Assert.IsType<OkObjectResult>(await _controller.Login(
                new LoginDto { Username = "testuser", Password = password }));
            var loginResponse = Assert.IsType<TokenResponseDto>(loginResult.Value);
            var oldRefreshToken = loginResponse.RefreshToken;

            var refreshDto = new RefreshTokenRequestDto
            {
                AccessToken = loginResponse.AccessToken,
                RefreshToken = oldRefreshToken
            };

            // Act
            await _controller.RefreshToken(refreshDto);

            // Assert
            var oldTokenInDb = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == oldRefreshToken);

            Assert.NotNull(oldTokenInDb);
            Assert.True(oldTokenInDb.IsUsed);
            Assert.True(oldTokenInDb.IsRevoked);
        }
    }
}
