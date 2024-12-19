using Sheas_Cealer.Consts;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainNginxButtonContentConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNginxRunning = (bool)values[0];
        bool isNginxIniting = (bool)values[1];

        return isNginxIniting ? MainConst.NginxButtonIsInitingContent :
            isNginxRunning ? MainConst.NginxButtonIsRunningContent : MainConst.NginxButtonIsStoppedContent;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}