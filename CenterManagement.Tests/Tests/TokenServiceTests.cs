using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CenterManagement.API.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CenterManagement.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;
        private readonly IConfiguration _configuration;

        public TokenServiceTests()
        {
            var mockConfig = new Mock<IConfiguration>();

            mockConfig.Setup(x => x["AppSettings:Jwt:Key"])
                .Returns("your-super-secret-key-that-must-be-at-least-32-characters-long!");
            mockConfig.Setup(x => x["AppSettings:Jwt:Issuer"])
                .Returns("CenterManagementAPI");
            mockConfig.Setup(x => x["AppSettings:Jwt:Audience"])
                .Returns("CenterManagementUsers");
            mockConfig.Setup(x => x["AppSettings:Jwt:ExpireMinutes"])
                .Returns("15");

            _configuration = mockConfig.Object;
            _tokenService = new TokenService(_configuration);
        }

        // Test 1: Generate valid access token
        [Fact]
        public void GenerateAccessToken_WithValidInput_ShouldReturnValidToken()
        {
            // Arrange
            int userId = 1;
            string fullname = "Nguyen Van A";
            string username = "nguyenvana";
            int roleId = 1;

            // Act
            var token = _tokenService.GenerateAccessToken(userId, fullname, username, roleId);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);

            // Token phải có 3 phần (header.payload.signature)
            var parts = token.Split('.');
            Assert.Equal(3, parts.Length);
        }

        // Test 2: Verify token structure
        [Fact]
        public void GenerateAccessToken_ShouldContainCorrectClaims()
        {
            // Arrange
            int userId = 42;
            string fullname = "John Doe";
            string username = "johndoe";
            int roleId = 2;

            // Act
            var token = _tokenService.GenerateAccessToken(userId, fullname, username, roleId);
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            Assert.Equal(userId.ToString(), jwtToken.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            Assert.Equal(username, jwtToken.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);

            Assert.Equal(fullname, jwtToken.Claims
                .FirstOrDefault(c => c.Type == "fullName")?.Value);

            Assert.Equal(roleId.ToString(), jwtToken.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
        }

        // Test 3: Token has expiration
        [Fact]
        public void GenerateAccessToken_ShouldHaveCorrectExpiration()
        {
            // Arrange
            var beforeGeneration = DateTime.UtcNow;

            // Act
            var token = _tokenService.GenerateAccessToken(1, "Test", "test", 1);
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var afterGeneration = DateTime.UtcNow;

            // Assert - Token phải hết hạn sau 15 phút (±5 giây buffer)
            Assert.True(jwtToken.ValidTo > afterGeneration);
            Assert.True(jwtToken.ValidTo <= beforeGeneration.AddMinutes(15).AddSeconds(5));
        }

        // Test 4: Token has JTI (JWT ID)
        [Fact]
        public void GenerateAccessToken_ShouldHaveJti()
        {
            // Act
            var token = _tokenService.GenerateAccessToken(1, "Test", "test", 1);
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            Assert.NotNull(jtiClaim);
            Assert.NotEmpty(jtiClaim.Value);
            Assert.True(Guid.TryParse(jtiClaim.Value, out _));
        }

        // Test 5: Generate refresh token
        [Fact]
        public void GenerateRefreshToken_ShouldReturnBase64String()
        {
            // Act
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Assert
            Assert.NotNull(refreshToken);
            Assert.NotEmpty(refreshToken);

            // Nếu không phải base64 hợp lệ thì sẽ throw, test sẽ tự fail
            var bytes = Convert.FromBase64String(refreshToken);
            Assert.Equal(32, bytes.Length); // GenerateRefreshToken dùng 32 bytes
        }

        // Test 6: Each refresh token is unique
        [Fact]
        public void GenerateRefreshToken_ShouldGenerateUniqueTokens()
        {
            // Act
            var token1 = _tokenService.GenerateRefreshToken();
            var token2 = _tokenService.GenerateRefreshToken();
            var token3 = _tokenService.GenerateRefreshToken();

            // Assert
            Assert.NotEqual(token1, token2);
            Assert.NotEqual(token2, token3);
            Assert.NotEqual(token1, token3);
        }

        // Test 7: Get principal from valid expired token
        [Fact]
        public void GetPrincipalFromExpiredToken_WithValidToken_ShouldReturnPrincipal()
        {
            // Arrange
            var token = _tokenService.GenerateAccessToken(1, "Test User", "testuser", 1);

            // Act
            var principal = _tokenService.GetPrincipalFromExpiredToken(token);

            // Assert
            Assert.NotNull(principal);
            Assert.Equal("1", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            Assert.Equal("testuser", principal.FindFirst(ClaimTypes.Name)?.Value);
        }

        // Test 8: Get principal from invalid token
        [Fact]
        public void GetPrincipalFromExpiredToken_WithInvalidToken_ShouldThrowException()
        {
            
            string invalidToken = "invalid.token.here";

            
            Assert.ThrowsAny<Exception>(() =>
                _tokenService.GetPrincipalFromExpiredToken(invalidToken));
        }

        // Test 9: Get principal from tampered token
        [Fact]
        public void GetPrincipalFromExpiredToken_WithTamperedToken_ShouldThrowException()
        {
            var token = _tokenService.GenerateAccessToken(1, "Test", "test", 1);
            var tampered = token.Substring(0, token.Length - 5) + "xxxxx";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() =>
                _tokenService.GetPrincipalFromExpiredToken(tampered));
        }

        // Test 10: Token từ instance khác nhưng cùng config vẫn validate được
        [Fact]
        public void GetPrincipalFromExpiredToken_TokenFromAnotherInstance_ShouldReturnPrincipal()
        {
            // Arrange - Tạo service instance mới với cùng config
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["AppSettings:Jwt:Key"])
                .Returns("your-super-secret-key-that-must-be-at-least-32-characters-long!");
            mockConfig.Setup(x => x["AppSettings:Jwt:Issuer"])
                .Returns("CenterManagementAPI");
            mockConfig.Setup(x => x["AppSettings:Jwt:Audience"])
                .Returns("CenterManagementUsers");
            mockConfig.Setup(x => x["AppSettings:Jwt:ExpireMinutes"])
                .Returns("15");

            var otherService = new TokenService(mockConfig.Object);
            var token = otherService.GenerateAccessToken(1, "Test", "test", 1);

            // Act
            var principal = _tokenService.GetPrincipalFromExpiredToken(token);

            // Assert
            Assert.NotNull(principal);
        }

        // Test 11: Missing JWT key should throw exception
        [Fact]
        public void GenerateAccessToken_WithMissingJwtKey_ShouldThrowException()
        {
            // Arrange
            var mockConfigNoKey = new Mock<IConfiguration>();
            mockConfigNoKey.Setup(x => x["AppSettings:Jwt:Key"])
                .Returns((string)null);
            var serviceNoKey = new TokenService(mockConfigNoKey.Object);

            // Act & Assert
            Assert.Throws<Exception>(() =>
                serviceNoKey.GenerateAccessToken(1, "Test", "test", 1));
        }

        // Test 12: Token with different role IDs
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(99)]
        public void GenerateAccessToken_WithDifferentRoles_ShouldIncludeCorrectRole(int roleId)
        {
            // Act
            var token = _tokenService.GenerateAccessToken(1, "Test", "test", roleId);
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            Assert.Equal(roleId.ToString(), roleClaim?.Value);
        }
    }
}
