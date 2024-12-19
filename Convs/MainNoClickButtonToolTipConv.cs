using Sheas_Cealer.Consts;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainNoClickButtonToolTipConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isFlashing = (bool)value;

        return isFlashing ? MainConst.NoClickButtonIsFlashingToolTip : MainConst.NoClickButtonIsStoppedToolTip;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}