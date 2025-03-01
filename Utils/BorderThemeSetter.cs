using System.Runtime.InteropServices;
using System.Windows;

namespace Sheas_Cealer_Nix.Utils;

internal static partial class BorderThemeSetter
{
    //private const int DwmwaUseImmersiveDarkModeOld = 19;
    //private const int DwmwaUseImmersiveDarkMode = 20;

    //[LibraryImport("dwmapi.dll")]
    //private static partial void DwmGetWindowAttribute(nint hwnd, uint attr, out nint attrValue, uint attrSize);
    //[LibraryImport("dwmapi.dll")]
    //private static partial void DwmSetWindowAttribute(nint hwnd, uint attr, ref nint attrValue, uint attrSize);

    //internal static void SetBorderTheme(Window window, bool? isLightTheme)
    //{
    //    nint isDarkTheme;
    //    nint desktopHwnd = nint.Zero;
    //    nint windowHwnd = new WindowInteropHelper(window).EnsureHandle();

    //    if (isLightTheme.HasValue)
    //        isDarkTheme = !isLightTheme.Value ? 1 : 0;
    //    else
    //        DwmGetWindowAttribute(desktopHwnd, DwmwaUseImmersiveDarkMode, out isDarkTheme, (uint)Marshal.SizeOf(typeof(nint)));

    //    DwmSetWindowAttribute(windowHwnd, DwmwaUseImmersiveDarkModeOld, ref isDarkTheme, (uint)Marshal.SizeOf(typeof(nint)));
    //    DwmSetWindowAttribute(windowHwnd, DwmwaUseImmersiveDarkMode, ref isDarkTheme, (uint)Marshal.SizeOf(typeof(nint)));
    //}
}