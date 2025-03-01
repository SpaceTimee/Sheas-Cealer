using Avalonia.Data.Converters;
using Sheas_Cealer_Nix.Consts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class MainSettingsBoxTextConv : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        MainConst.SettingsMode settingsMode = (MainConst.SettingsMode)values[0];
        string browserPath = (string)values[1];
        string upstreamUrl = (string)values[2];
        string extraArgs = (string)values[3];

        return settingsMode switch
        {
            MainConst.SettingsMode.BrowserPathMode => browserPath,
            MainConst.SettingsMode.UpstreamUrlMode => upstreamUrl,
            MainConst.SettingsMode.ExtraArgsMode => extraArgs,
            _ => throw new UnreachableException()
        };
    }
}