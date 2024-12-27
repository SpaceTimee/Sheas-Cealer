using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainMihomoButtonIsEnabledConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool isCoMihomoExist = (bool)values[0];
        bool isMihomoExist = (bool)values[1];
        bool isCoproxyIniting = (bool)values[2];
        bool isComihomoIniting = (bool)values[3];
        bool isMihomoIniting = (bool)values[4];

        return !isCoproxyIniting && !isComihomoIniting && !isMihomoIniting && (isCoMihomoExist || isMihomoExist);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}