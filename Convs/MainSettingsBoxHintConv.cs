using Avalonia.Data.Converters;
using Sheas_Cealer_Nix.Consts;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class MainSettingsBoxHintConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        MainConst.SettingsMode settingsMode = (MainConst.SettingsMode)value;

        return settingsMode switch
        {
            MainConst.SettingsMode.BrowserPathMode => MainConst.SettingsModeButtonBrowserPathContent,
            MainConst.SettingsMode.UpstreamUrlMode => MainConst.SettingsModeButtonUpstreamUrlContent,
            MainConst.SettingsMode.ExtraArgsMode => MainConst.SettingsModeButtonExtraArgsContent,
            _ => throw new UnreachableException()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}