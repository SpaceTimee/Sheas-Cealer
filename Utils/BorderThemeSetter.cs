using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Sheas_Cealer.Utils;

internal static partial class BorderThemeSetter
{
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmGetWindowAttribute(nint hwnd, uint attr, out nint attrValue, uint attrSize);
    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(nint hwnd, uint attr, ref nint attrValue, uint attrSize);

    internal static void SetBorderTheme(Window window, bool? isLightTheme)
    {
        if (!window.IsLoaded)
            return;

        nint isDarkTheme;
        nint desktopHwnd = nint.Zero;
        nint windowHwnd = new WindowInteropHelper(window).EnsureHandle();

        if (isLightTheme.HasValue)
            isDarkTheme = !isLightTheme.Value ? 1 : 0;
        else
            DwmGetWindowAttribute(desktopHwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, out isDarkTheme, (uint)Marshal.SizeOf(typeof(nint)));

        _ = DwmSetWindowAttribute(windowHwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref isDarkTheme, (uint)Marshal.SizeOf(typeof(nint)));
        _ = DwmSetWindowAttribute(windowHwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref isDarkTheme, (uint)Marshal.SizeOf(typeof(nint)));
    }
}