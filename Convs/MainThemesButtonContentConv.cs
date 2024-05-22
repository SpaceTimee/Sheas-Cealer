using System;
using System.Globalization;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainThemesButtonContentConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool? isLightMode = value as bool?;

        return isLightMode.HasValue ? (isLightMode.GetValueOrDefault() ? MainConst.ThemesButtonLightThemeContent : MainConst.ThemesButtonDarkThemeContent) : MainConst.ThemesButtonInheritThemeContent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}