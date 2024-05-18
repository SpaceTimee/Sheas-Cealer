using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainSwitchModeButtonContentConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        MainConst.SettingsMode? mode = value as MainConst.SettingsMode?;

        if (mode == MainConst.SettingsMode.BrowserPathMode)
            return MainConst.SwitchModeButtonBrowserPathContent;
        else if (mode == MainConst.SettingsMode.UpstreamUrlMode)
            return MainConst.SwitchModeButtonUpstreamUrlContent;
        else if (mode == MainConst.SettingsMode.ExtraArgsMode)
            return MainConst.SwitchModeButtonExtraArgsContent;

        throw new UnreachableException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}