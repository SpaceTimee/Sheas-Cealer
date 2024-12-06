using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Props;
using Sheas_Cealer.Utils;
using Sheas_Cealer.Wins;

namespace Sheas_Cealer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        _ = new SettingsPres();

        PaletteHelper paletteHelper = new();
        Theme newTheme = paletteHelper.GetTheme();
        System.Drawing.Color newColor = Settings.Default.PrimaryColor;

        newTheme.SetPrimaryColor(Color.FromRgb(newColor.R, newColor.G, newColor.B));
        paletteHelper.SetTheme(newTheme);

        Color? foregroundColor = ForegroundGenerator.GetForeground(newColor.R, newColor.G, newColor.B);

        if (foregroundColor.HasValue)
            Current.Resources["MaterialDesignBackground"] = new SolidColorBrush(foregroundColor.Value);
        else
            Current.Resources.Remove("MaterialDesignBackground");

        new MainWin(e.Args).Show();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Error: {e.Exception.Message}");
        e.Handled = true;
    }
}