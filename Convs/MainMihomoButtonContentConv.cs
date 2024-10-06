using System;
using System.Globalization;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainMihomoButtonContentConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool isMihomoRunning = (bool)values[0];
        bool isMihomoIniting = (bool)values[1];

        return isMihomoIniting ? MainConst.MihomoButtonIsInitingContent :
            isMihomoRunning ? MainConst.MihomoButtonIsRunningContent : MainConst.MihomoButtonIsStoppedContent;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}