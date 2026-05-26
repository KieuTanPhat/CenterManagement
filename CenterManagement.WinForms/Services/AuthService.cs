using System.Net.Http.Json;
using System.Text.Json;
using CenterManagement.Models.DTOs;
using CenterManagement.WinForms.Core;

namespace CenterManagement.WinForms.Services;

public class AuthService : IAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<TokenResponseDto?> LoginAsync(string username, string password)
    {
        var response = await ApiClient.Instance.PostRawAsync("api/auth/login", new LoginDto
        {
            Username = username,
            Password = password
        });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(error.Trim('"'));
        }

        return await response.Content.ReadFromJsonAsync<TokenResponseDto>(JsonOptions);
    }

    public async Task LogoutAsync()
    {
        await ApiClient.Instance.PostRawAsync("api/auth/Logout", new { });
        ApiClient.Instance.ClearAuthToken();
        AppSession.Current.Clear();
    }
}
