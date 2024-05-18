using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainFunctionButtonContentConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        MainConst.SettingsMode? mode = value as MainConst.SettingsMode?;

        if (mode == MainConst.SettingsMode.BrowserPathMode)
            return MainConst.FunctionButtonBrowserPathContent;
        else if (mode == MainConst.SettingsMode.UpstreamUrlMode)
            return MainConst.FunctionButtonUpstreamUrlContent;
        else if (mode == MainConst.SettingsMode.ExtraArgsMode)
            return MainConst.FunctionButtonExtraArgsContent;

        throw new UnreachableException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}