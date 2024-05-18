using System;
using System.Diagnostics;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainSettingsBoxTextConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        MainConst.SettingsMode? mode = values[0] as MainConst.SettingsMode?;
        bool? isFocused = values[1] as bool?;
        string? browserPath = values[2] as string;
        string? upstreamUrl = values[3] as string;
        string? extraArgs = values[4] as string;

        return mode switch
        {
            MainConst.SettingsMode.BrowserPathMode => !isFocused.GetValueOrDefault() && string.IsNullOrEmpty(browserPath) ? MainConst.SettingsBoxBrowserPathPlaceHolder :
                isFocused.GetValueOrDefault() && browserPath == MainConst.SettingsBoxBrowserPathPlaceHolder ? string.Empty : browserPath!,
            MainConst.SettingsMode.UpstreamUrlMode => !isFocused.GetValueOrDefault() && string.IsNullOrEmpty(upstreamUrl) ? MainConst.SettingsBoxUpstreamUrlPlaceHolder :
                isFocused.GetValueOrDefault() && upstreamUrl == MainConst.SettingsBoxUpstreamUrlPlaceHolder ? string.Empty : upstreamUrl!,
            MainConst.SettingsMode.ExtraArgsMode => !isFocused.GetValueOrDefault() && string.IsNullOrEmpty(extraArgs) ? MainConst.SettingsBoxExtraArgsPlaceHolder :
                isFocused.GetValueOrDefault() && extraArgs == MainConst.SettingsBoxExtraArgsPlaceHolder ? string.Empty : extraArgs!,
            _ => throw new UnreachableException(),
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}