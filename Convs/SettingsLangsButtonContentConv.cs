using Avalonia.Data.Converters;
using Sheas_Cealer_Nix.Consts;
using System;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class SettingsLangsButtonContentConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool? isEnglishLang = value as bool?;

        return isEnglishLang.HasValue ?
            isEnglishLang.GetValueOrDefault() ? SettingsConst.LangsButtonEnglishLangContent : SettingsConst.LangsButtonChineseLangContent :
            SettingsConst.LangsButtonInheritLangContent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}