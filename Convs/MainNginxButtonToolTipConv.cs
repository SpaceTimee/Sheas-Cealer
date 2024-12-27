using Sheas_Cealer.Consts;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainNginxButtonToolTipConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool isConginxRunning = (bool)values[0];
        bool isNginxRunning = (bool)values[1];

        return isConginxRunning || isNginxRunning ? MainConst.NginxButtonIsRunningToolTip : MainConst.NginxButtonIsStoppedToolTip;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}