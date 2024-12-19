using Sheas_Cealer.Consts;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainMihomoButtonToolTipConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isMihomoRunning = (bool)value;

        return isMihomoRunning ? MainConst.MihomoButtonIsRunningToolTip : MainConst.MihomoButtonIsStoppedToolTip;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}