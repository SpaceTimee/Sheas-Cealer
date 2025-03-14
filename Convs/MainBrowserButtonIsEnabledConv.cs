using Avalonia.Data.Converters;
using Sheas_Cealer_Nix.Consts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Sheas_Cealer_Nix.Convs;

internal class MainBrowserButtonIsEnabledConv : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        string browserPath = (string)values[0];
        string extraArgs = (string)values[1];

        return File.Exists(browserPath) && MainConst.ExtraArgsRegex().IsMatch(extraArgs);
    }
}