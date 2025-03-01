using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class MainNginxButtonIsEnabledConv : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isConginxExist = (bool)values[0];
        bool isNginxExist = (bool)values[1];
        bool isCoproxyIniting = (bool)values[2];
        bool isNginxIniting = (bool)values[3];
        bool isComihomoIniting = (bool)values[4];
        bool isMihomoIniting = (bool)values[5];

        return !isCoproxyIniting && !isNginxIniting && !isComihomoIniting && !isMihomoIniting && (isConginxExist || isNginxExist);
    }
}