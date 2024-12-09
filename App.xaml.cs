using System;
using System.Windows;
using System.Windows.Controls;
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

        #region Primary Color
        PaletteHelper paletteHelper = new();
        Theme newTheme = paletteHelper.GetTheme();
        System.Drawing.Color newColor = Settings.Default.PrimaryColor;

        newTheme.SetPrimaryColor(Color.FromRgb(newColor.R, newColor.G, newColor.B));
        paletteHelper.SetTheme(newTheme);
        #endregion Primary Color

        #region Background Color
        if (Environment.OSVersion.Version.Build < 22000)
        {
            Style newWindowStyle = new(typeof(Window), Current.Resources["CommonWindow"] as Style);
            newWindowStyle.Setters.Add(new Setter(Window.BackgroundProperty, new DynamicResourceExtension("MaterialDesignBackground")));
            Current.Resources["CommonWindow"] = newWindowStyle;
        }
        #endregion Background Color

        #region Foreground Color
        Color? foregroundColor = ForegroundGenerator.GetForeground(newColor.R, newColor.G, newColor.B);

        Style newButtonStyle = new(typeof(Button), Current.Resources[typeof(Button)] as Style);
        newButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, foregroundColor.HasValue ? new SolidColorBrush(foregroundColor.Value) : new DynamicResourceExtension("MaterialDesignBackground")));
        Current.Resources[typeof(Button)] = newButtonStyle;
        #endregion Foreground Color

        new MainWin(e.Args).Show();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Error: {e.Exception.Message}");
        e.Handled = true;
    }
}