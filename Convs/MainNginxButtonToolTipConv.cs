using Avalonia.Data.Converters;
using Sheas_Cealer_Nix.Consts;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class MainNginxButtonToolTipConv : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isConginxRunning = (bool)values[0];
        bool isNginxRunning = (bool)values[1];

        return isConginxRunning || isNginxRunning ? MainConst.NginxButtonIsRunningToolTip : MainConst.NginxButtonIsStoppedToolTip;
    }
}