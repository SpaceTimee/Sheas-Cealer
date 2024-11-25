using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;
using Sheas_Cealer.Utils;

namespace Sheas_Cealer.Preses;

internal partial class GlobalPres : ObservableObject
{
    [ObservableProperty]
    private static bool? isLightTheme = null;
    partial void OnIsLightThemeChanged(bool? value)
    {
        PaletteHelper paletteHelper = new();
        Theme newTheme = paletteHelper.GetTheme();

        newTheme.SetBaseTheme(value.HasValue ? value.GetValueOrDefault() ? BaseTheme.Light : BaseTheme.Dark : BaseTheme.Inherit);
        paletteHelper.SetTheme(newTheme);

        BorderThemeSetter.SetBorderTheme(Application.Current.MainWindow, value);
    }
}