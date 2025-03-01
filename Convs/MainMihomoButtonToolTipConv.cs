using Avalonia.Data.Converters;
using Sheas_Cealer_Nix.Consts;
using System;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class MainMihomoButtonToolTipConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isMihomoRunning = (bool)value;

        return isMihomoRunning ? MainConst.MihomoButtonIsRunningToolTip : MainConst.MihomoButtonIsStoppedToolTip;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}