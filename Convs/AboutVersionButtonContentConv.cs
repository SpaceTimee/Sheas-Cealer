using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class AboutVersionButtonContentConv : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        string versionButtonLabelContent = (string)values[0];
        string versionButtonVersionContent = (string)values[1];
        bool isSheasCealerUtd = (bool)values[2];

        return $"{versionButtonLabelContent} {versionButtonVersionContent}" + (isSheasCealerUtd ? string.Empty : " *");
    }
}