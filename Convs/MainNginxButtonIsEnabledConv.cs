using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainNginxButtonIsEnabledConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool isConginxExist = (bool)values[0];
        bool isNginxExist = (bool)values[1];
        bool isCoproxyIniting = (bool)values[2];
        bool isNginxIniting = (bool)values[3];
        bool isComihomoIniting = (bool)values[4];
        bool isMihomoIniting = (bool)values[5];

        return !isCoproxyIniting && !isNginxIniting && !isComihomoIniting && !isMihomoIniting && (isConginxExist || isNginxExist);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}