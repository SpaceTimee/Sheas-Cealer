using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Sheas_Cealer.Convs;

internal class AboutAccentButtonForegroundConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Color accentForegroundColor = (Color)value;

        return new SolidColorBrush(accentForegroundColor);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}