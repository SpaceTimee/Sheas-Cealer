using System;
using System.Diagnostics;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainSettingsBoxTextConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        MainConst.SettingsMode? settingsMode = values[0] as MainConst.SettingsMode?;
        string? browserPath = values[1] as string;
        string? upstreamUrl = values[2] as string;
        string? extraArgs = values[3] as string;

        return settingsMode switch
        {
            MainConst.SettingsMode.BrowserPathMode => browserPath!,
            MainConst.SettingsMode.UpstreamUrlMode => upstreamUrl!,
            MainConst.SettingsMode.ExtraArgsMode => extraArgs!,
            _ => throw new UnreachableException(),
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}