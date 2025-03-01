using Avalonia.Data.Converters;
using Sheas_Cealer_Nix.Consts;
using System;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class SettingsThemesButtonContentConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool? isLightMode = value as bool?;

        return isLightMode.HasValue ?
            isLightMode.GetValueOrDefault() ? SettingsConst.ThemesButtonLightThemeContent : SettingsConst.ThemesButtonDarkThemeContent :
            SettingsConst.ThemesButtonInheritThemeContent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}