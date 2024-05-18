using System;
using System.Diagnostics;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainContentBoxTextConv : IMultiValueConverter
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
            MainConst.SettingsMode.BrowserPathMode => !isFocused.GetValueOrDefault() && string.IsNullOrEmpty(browserPath) ? MainConst.BrowserPathPlaceHolder :
                isFocused.GetValueOrDefault() && browserPath == MainConst.BrowserPathPlaceHolder ? string.Empty : browserPath!,
            MainConst.SettingsMode.UpstreamUrlMode => !isFocused.GetValueOrDefault() && string.IsNullOrEmpty(upstreamUrl) ? MainConst.UpstreamUrlPlaceHolder :
                isFocused.GetValueOrDefault() && upstreamUrl == MainConst.UpstreamUrlPlaceHolder ? string.Empty : upstreamUrl!,
            MainConst.SettingsMode.ExtraArgsMode => !isFocused.GetValueOrDefault() && string.IsNullOrEmpty(extraArgs) ? MainConst.ExtraArgsPlaceHolder :
                isFocused.GetValueOrDefault() && extraArgs == MainConst.ExtraArgsPlaceHolder ? string.Empty : extraArgs!,
            _ => throw new UnreachableException(),
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}