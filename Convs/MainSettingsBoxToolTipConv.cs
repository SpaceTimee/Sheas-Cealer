using Sheas_Cealer.Consts;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainSettingsBoxToolTipConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        MainConst.SettingsMode settingsMode = (MainConst.SettingsMode)value;

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