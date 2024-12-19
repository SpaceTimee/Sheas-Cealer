using Sheas_Cealer.Consts;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class SettingsLangsButtonContentConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        bool? isEnglishLang = value as bool?;

        return isEnglishLang.HasValue ?
            isEnglishLang.GetValueOrDefault() ? SettingsConst.LangsButtonEnglishLangContent : SettingsConst.LangsButtonChineseLangContent :
            SettingsConst.LangsButtonInheritLangContent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}