using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainAdminControlVisibilityConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isAdmin = (bool)value;

        return isAdmin ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}