using Sheas_Cealer.Consts;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class SettingsThemesButtonContentConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        bool? isLightMode = value as bool?;

        return isLightMode.HasValue ?
            isLightMode.GetValueOrDefault() ? SettingsConst.ThemesButtonLightThemeContent : SettingsConst.ThemesButtonDarkThemeContent :
            SettingsConst.ThemesButtonInheritThemeContent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}