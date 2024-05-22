using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainSettingsBoxHintConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        MainConst.SettingsMode? mode = value as MainConst.SettingsMode?;

        return mode switch
        {
            MainConst.SettingsMode.BrowserPathMode => MainConst.BrowserPathModeName,
            MainConst.SettingsMode.UpstreamUrlMode => MainConst.UpstreamUrlModeName,
            MainConst.SettingsMode.ExtraArgsMode => MainConst.ExtraArgsModeName,
            _ => throw new UnreachableException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}