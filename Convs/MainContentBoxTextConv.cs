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

        if (mode == MainConst.SettingsMode.BrowserPathMode)
        {
            if ((bool)!isFocused! && string.IsNullOrEmpty(browserPath))
                return MainConst.BrowserPathPlaceHolder;
            else if ((bool)isFocused! && browserPath == MainConst.BrowserPathPlaceHolder)
                return string.Empty;
            else
                return browserPath!;
        }
        else if (mode == MainConst.SettingsMode.UpstreamUrlMode)
        {
            if ((bool)!isFocused! && string.IsNullOrEmpty(upstreamUrl))
                return MainConst.UpstreamUrlPlaceHolder;
            else if ((bool)isFocused! && upstreamUrl == MainConst.UpstreamUrlPlaceHolder)
                return string.Empty;
            else
                return upstreamUrl!;
        }
        else if (mode == MainConst.SettingsMode.ExtraArgsMode)
        {
            if ((bool)!isFocused! && string.IsNullOrEmpty(extraArgs))
                return MainConst.ExtraArgsPlaceHolder;
            else if ((bool)isFocused! && extraArgs == MainConst.ExtraArgsPlaceHolder)
                return string.Empty;
            else
                return extraArgs!;
        }
        else
        {
            throw new UnreachableException();
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}