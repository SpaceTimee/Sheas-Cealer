using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Sheas_Cealer.Utils;

internal static partial class IconRemover
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_DLGMODALFRAME = 0x0001;
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOZORDER = 0x0004;
    private const int SWP_FRAMECHANGED = 0x0020;
    private const uint WM_SETICON = 0x0080;

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static partial int GetWindowLong(nint hwnd, int index);
    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static partial int SetWindowLong(nint hwnd, int index, nint newStyle);
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(nint hwnd, nint hwndInsertAfter, int x, int y, int width, int height, uint flags);
    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial nint SendMessage(nint hwnd, uint msg, nint wParam, nint lParam);

    internal static void RemoveIcon(Window window)
    {
        nint hwnd = new WindowInteropHelper(window).Handle;

        _ = SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_DLGMODALFRAME);

        SetWindowPos(hwnd, nint.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

        SendMessage(hwnd, WM_SETICON, new nint(1), nint.Zero);
        SendMessage(hwnd, WM_SETICON, nint.Zero, nint.Zero);
    }
}