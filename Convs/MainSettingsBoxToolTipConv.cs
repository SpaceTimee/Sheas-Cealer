using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainSettingsBoxToolTipConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        MainConst.SettingsMode? settingsMode = value as MainConst.SettingsMode?;

        return settingsMode switch
        {
            MainConst.SettingsMode.BrowserPathMode => MainConst.SettingsBoxBrowserPathToolTip,
            MainConst.SettingsMode.UpstreamUrlMode => MainConst.SettingsBoxUpstreamUrlToolTip,
            MainConst.SettingsMode.ExtraArgsMode => MainConst.SettingsBoxExtraArgsToolTip,
            _ => throw new UnreachableException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}