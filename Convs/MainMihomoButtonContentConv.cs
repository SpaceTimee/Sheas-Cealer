using Sheas_Cealer.Consts;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainMihomoButtonContentConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool isMihomoRunning = (bool)values[0];
        bool isComihomoIniting = (bool)values[1];
        bool isMihomoIniting = (bool)values[2];
        bool isCoproxyIniting = (bool)values[3];
        bool isCoproxyStopping = (bool)values[4];

        return isCoproxyIniting || isCoproxyStopping ? Binding.DoNothing :
            isComihomoIniting ? MainConst.MihomoButtonIsStoppingContent :
            isMihomoIniting ? MainConst.MihomoButtonIsInitingContent :
            isMihomoRunning ? MainConst.MihomoButtonIsRunningContent : MainConst.MihomoButtonIsStoppedContent;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}