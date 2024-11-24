using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace Sheas_Cealer.Preses;

internal partial class AppPres : ObservableObject
{
    [ObservableProperty]
    private bool? isLightTheme = null;
    partial void OnIsLightThemeChanged(bool? value)
    {
        PaletteHelper paletteHelper = new();
        Theme newTheme = paletteHelper.GetTheme();

        newTheme.SetBaseTheme(value.HasValue ? value.GetValueOrDefault() ? BaseTheme.Light : BaseTheme.Dark : BaseTheme.Inherit);
        paletteHelper.SetTheme(newTheme);
    }
}