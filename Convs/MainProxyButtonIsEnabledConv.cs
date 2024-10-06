using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainProxyButtonIsEnabledConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool isProxyExist = (bool)values[0];
        bool isProxyIniting = (bool)values[1];

        return !isProxyIniting && isProxyExist;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}