using Sheas_Cealer.Consts;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainSettingsModeButtonContentConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        MainConst.SettingsMode settingsMode = (MainConst.SettingsMode)value;

        return settingsMode switch
        {
            MainConst.SettingsMode.BrowserPathMode => MainConst.SettingsModeButtonUpstreamUrlContent,
            MainConst.SettingsMode.UpstreamUrlMode => MainConst.SettingsModeButtonExtraArgsContent,
            MainConst.SettingsMode.ExtraArgsMode => MainConst.SettingsModeButtonBrowserPathContent,
            _ => throw new NotImplementedException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}