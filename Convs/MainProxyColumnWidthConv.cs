using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainProxyColumnWidthConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isAdmin = (bool)value;

        return new GridLength(1, isAdmin ? GridUnitType.Star : GridUnitType.Auto);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}