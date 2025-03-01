using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Sheas_Cealer_Nix.Consts;
using Sheas_Cealer_Nix.Props;
using System.Diagnostics;

namespace Sheas_Cealer_Nix.Preses;

internal partial class GlobalPres : ObservableObject
{
    internal GlobalPres()
    {
        //IsLightTheme = Settings.Default.IsLightTheme switch
        //{
        //    -1 => null,
        //    0 => false,
        //    1 => true,
        //    _ => throw new UnreachableException()
        //};
    }

    [ObservableProperty]
    private static bool? isLightTheme = null;
    partial void OnIsLightThemeChanged(bool? value)
    {
        //PaletteHelper paletteHelper = new();
        //Theme newTheme = paletteHelper.GetTheme();

        //newTheme.SetBaseTheme(value.HasValue ? value.Value ? BaseTheme.Light : BaseTheme.Dark : BaseTheme.Inherit);
        //paletteHelper.SetTheme(newTheme);

        //foreach (Window currentWindow in Application.Current.Windows)
        //    BorderThemeSetter.SetBorderTheme(currentWindow, value);

        //Settings.Default.IsLightTheme = (sbyte)(value.HasValue ? value.Value ? 1 : 0 : -1);
        //Settings.Default.Save();
    }

    [ObservableProperty]
    private static Color accentForegroundColor = AboutConst.AccentBlueColor;
}