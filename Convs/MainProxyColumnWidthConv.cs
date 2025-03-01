using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class MainProxyColumnWidthConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isAdmin = (bool)value;

        return new GridLength(1, isAdmin ? GridUnitType.Star : GridUnitType.Auto);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}