using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class MainAdminControlVisibilityConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isAdmin = (bool)value;

        return isAdmin;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}