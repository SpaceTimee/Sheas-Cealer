using System;
using System.Globalization;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainMihomoButtonContentConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isMihomoRunning = (bool)value;

        return isMihomoRunning ? MainConst.MihomoButtonIsRunningContent : MainConst.MihomoButtonIsStoppedContent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}