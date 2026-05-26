using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;
using CenterManagement.WinForms.Views.Auth;

namespace CenterManagement.WinForms.Presenters;

public class LoginPresenter
{
    private readonly ILoginView _view;
    private readonly IAuthService _authService;

    public event EventHandler? LoginSuccess;

    public LoginPresenter(ILoginView view, IAuthService authService)
    {
        _view = view;
        _authService = authService;
        _view.LoginClicked += OnLoginClicked;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_view.Username))
        {
            _view.ErrorMessage = "Vui long nhap ten dang nhap.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_view.Password))
        {
            _view.ErrorMessage = "Vui long nhap mat khau.";
            return;
        }

        _view.IsLoading = true;
        _view.ErrorMessage = string.Empty;

        try
        {
            var result = await _authService.LoginAsync(_view.Username, _view.Password);
            if (result == null)
            {
                _view.ErrorMessage = "Dang nhap that bai, vui long thu lai.";
                return;
            }

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
            var roleId = int.Parse(jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
            if (roleId is < 1 or > 4)
            {
                _view.ErrorMessage = "Hoc vien khong duoc dang nhap ung dung quan tri.";
                return;
            }

            AppSession.Current.AccessToken = result.AccessToken;
            AppSession.Current.RefreshToken = result.RefreshToken;
            AppSession.Current.TokenExpiration = result.Expiration;
            AppSession.Current.UserId = int.Parse(jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            AppSession.Current.Username = jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value;
            AppSession.Current.FullName = jwt.Claims.First(c => c.Type == "fullName").Value;
            AppSession.Current.RoleId = roleId;

            ApiClient.Instance.SetAuthToken(result.AccessToken);
            LoginSuccess?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _view.ErrorMessage = ex.Message;
        }
        finally
        {
            _view.IsLoading = false;
        }
    }
}
