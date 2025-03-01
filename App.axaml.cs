using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Sheas_Cealer_Nix.Preses;
using Sheas_Cealer_Nix.Utils;
using Sheas_Cealer_Nix.Wins;
using System;
using System.Linq;

namespace Sheas_Cealer_Nix;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            #region Upgrade Settings
            //if (Settings.Default.IsUpgradeRequired)
            //{
            //    Settings.Default.Upgrade();
            //    Settings.Default.IsUpgradeRequired = false;
            //    Settings.Default.Save();
            //}
            #endregion Upgrade Settings

            #region Primary Color
            //PaletteHelper paletteHelper = new();
            //Theme newTheme = paletteHelper.GetTheme();
            //Color newPrimaryColor = Color.FromRgb(Settings.Default.PrimaryColor.R, Settings.Default.PrimaryColor.G, Settings.Default.PrimaryColor.B);

            //newTheme.SetPrimaryColor(newPrimaryColor);
            //paletteHelper.SetTheme(newTheme);
            #endregion Primary Color

            #region Background Color
            //if (Environment.OSVersion.Version.Build < 22000)
            //{
            //    Style newWindowStyle = new(typeof(Window), Current.Resources["CommonWindow"] as Style);

            //    newWindowStyle.Setters.Add(new Setter(Window.BackgroundProperty, new DynamicResourceExtension("MaterialDesignBackground")));
            //    Current.Resources["CommonWindow"] = newWindowStyle;
            //}
            #endregion Background Color

            #region Foreground Color
            //Style newButtonStyle = new(typeof(Button), Current.Resources[typeof(Button)] as Style);
            //(Color? newForegroundColor, Color newAccentForegroundColor) = ForegroundGenerator.GetForeground(newPrimaryColor.R, newPrimaryColor.G, newPrimaryColor.B);

            //newButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, newForegroundColor.HasValue ? new SolidColorBrush(newForegroundColor.Value) : new DynamicResourceExtension("MaterialDesignBackground")));
            //Current.Resources[typeof(Button)] = newButtonStyle;

            //new SettingsPres().AccentForegroundColor = newAccentForegroundColor;
            #endregion Foreground Color

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWin();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }
}