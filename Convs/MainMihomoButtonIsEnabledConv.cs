using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class MainMihomoButtonIsEnabledConv : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isCoMihomoExist = (bool)values[0];
        bool isMihomoExist = (bool)values[1];
        bool isCoproxyIniting = (bool)values[2];
        bool isComihomoIniting = (bool)values[3];
        bool isMihomoIniting = (bool)values[4];

        return !isCoproxyIniting && !isComihomoIniting && !isMihomoIniting && (isCoMihomoExist || isMihomoExist);
    }
}