namespace CenterManagement.WinForms.Core;

public interface IView
{
    void ShowMessage(string message);
    void ShowError(string error);
}
