using CenterManagement.WinForms.Core;

namespace CenterManagement.WinForms.Views.Auth;

public interface ILoginView : IView
{
    string Username { get; }
    string Password { get; }
    bool IsLoading { set; }
    string ErrorMessage { set; }
    event EventHandler LoginClicked;
}
