using System.Runtime.InteropServices;
using System.Windows;

namespace Sheas_Cealer_Nix.Utils;

internal static partial class IconRemover
{
    //private const int GwlExStyle = -20;
    //private const int WsExDlgModalFrame = 0x0001;
    //private const int SwpNoSize = 0x0001;
    //private const int SwpNoMove = 0x0002;
    //private const int SwpNoZOrder = 0x0004;
    //private const int SwpFrameChanged = 0x0020;
    //private const uint WmSetIcon = 0x0080;

    //[LibraryImport("user32.dll", EntryPoint = "GetWindowLongW")]
    //private static partial int GetWindowLong(nint hwnd, int index);
    //[LibraryImport("user32.dll", EntryPoint = "SetWindowLongW")]
    //private static partial void SetWindowLong(nint hwnd, int index, nint newStyle);
    //[LibraryImport("user32.dll")]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //private static partial void SetWindowPos(nint hwnd, nint hwndInsertAfter, int x, int y, int width, int height, uint flags);
    //[LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    //private static partial void SendMessage(nint hwnd, uint msg, nint wParam, nint lParam);

    //internal static void RemoveIcon(Window window)
    //{
    //    nint hwnd = new WindowInteropHelper(window).Handle;

    //    SetWindowLong(hwnd, GwlExStyle, GetWindowLong(hwnd, GwlExStyle) | WsExDlgModalFrame);

    //    SetWindowPos(hwnd, nint.Zero, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoZOrder | SwpFrameChanged);

    //    SendMessage(hwnd, WmSetIcon, new(1), nint.Zero);
    //    SendMessage(hwnd, WmSetIcon, nint.Zero, nint.Zero);
    //}
}