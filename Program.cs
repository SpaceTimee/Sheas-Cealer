using Avalonia;
using MsBox.Avalonia;
using System;
using System.Threading.Tasks;

namespace Sheas_Cealer_Nix;

internal sealed class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        try { BuildAvaloniaApp().StartWithClassicDesktopLifetime(args); }
        catch (Exception ex) { await MessageBoxManager.GetMessageBoxStandard(string.Empty, $"Error: {ex.Message}").ShowAsync(); }
    }

    private static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont();
}

