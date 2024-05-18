using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainSettingsBoxForegroundConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        MainConst.SettingsMode? mode = values[0] as MainConst.SettingsMode?;
        string? browserPath = values[1] as string;
        string? upstreamUrl = values[2] as string;
        string? extraArgs = values[3] as string;

        switch (mode)
        {
            case MainConst.SettingsMode.BrowserPathMode when browserPath == MainConst.SettingsBoxBrowserPathPlaceHolder:
            case MainConst.SettingsMode.UpstreamUrlMode when upstreamUrl == MainConst.SettingsBoxUpstreamUrlPlaceHolder:
            case MainConst.SettingsMode.ExtraArgsMode when extraArgs == MainConst.SettingsBoxExtraArgsPlaceHolder:
                return new SolidColorBrush(Color.FromRgb(191, 205, 219));
        }

        return new SolidColorBrush(Color.FromRgb(0, 0, 0));
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}