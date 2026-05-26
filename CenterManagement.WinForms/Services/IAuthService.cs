using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface IAuthService
{
    Task<TokenResponseDto?> LoginAsync(string username, string password);
    Task LogoutAsync();
}
