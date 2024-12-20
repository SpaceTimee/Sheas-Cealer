using MaterialDesignThemes.Wpf;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Props;
using Sheas_Cealer.Utils;
using Sheas_Cealer.Wins;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Sheas_Cealer;

public partial class App : Application
{
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        #region Upgrade Settings
        if (Settings.Default.IsUpgradeRequired)
        {
            Settings.Default.Upgrade();
            Settings.Default.IsUpgradeRequired = false;
            Settings.Default.Save();
        }
        #endregion Upgrade Settings

        #region Primary Color
        PaletteHelper paletteHelper = new();
        Theme newTheme = paletteHelper.GetTheme();
        Color newPrimaryColor = Color.FromRgb(Settings.Default.PrimaryColor.R, Settings.Default.PrimaryColor.G, Settings.Default.PrimaryColor.B);

        newTheme.SetPrimaryColor(newPrimaryColor);
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
        Style newButtonStyle = new(typeof(Button), Current.Resources[typeof(Button)] as Style);
        (Color? newForegroundColor, Color newAccentForegroundColor) = ForegroundGenerator.GetForeground(newPrimaryColor.R, newPrimaryColor.G, newPrimaryColor.B);

        newButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, newForegroundColor.HasValue ? new SolidColorBrush(newForegroundColor.Value) : new DynamicResourceExtension("MaterialDesignBackground")));
        Current.Resources[typeof(Button)] = newButtonStyle;

        new SettingsPres().AccentForegroundColor = newAccentForegroundColor;
        #endregion Foreground Color

        new MainWin().Show();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Error: {e.Exception.Message}");
        e.Handled = true;
    }
}