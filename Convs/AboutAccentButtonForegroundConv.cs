using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class AboutAccentButtonForegroundConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color accentForegroundColor = (Color)value;

        return new SolidColorBrush(accentForegroundColor);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}