using Sheas_Cealer.Consts;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainSettingsBoxTextConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
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

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}