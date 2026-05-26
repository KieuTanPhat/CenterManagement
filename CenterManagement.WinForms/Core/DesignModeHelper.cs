using System.ComponentModel;

namespace CenterManagement.WinForms.Core;

internal static class DesignModeHelper
{
    public static bool IsInDesignMode =>
        LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
        string.Equals(System.Diagnostics.Process.GetCurrentProcess().ProcessName, "devenv", StringComparison.OrdinalIgnoreCase);
}
