using Sheas_Cealer.Consts;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class MainNginxButtonContentConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool isConginxRunning = (bool)values[0];
        bool isNginxRunning = (bool)values[1];
        bool isCoproxyIniting = (bool)values[2];
        bool isNginxIniting = (bool)values[3];

        return isCoproxyIniting ? MainConst.ConginxButtonIsInitingContent :
            isNginxIniting ? MainConst.NginxButtonIsInitingContent :
            isConginxRunning || isNginxRunning ? MainConst.NginxButtonIsRunningContent : MainConst.NginxButtonIsStoppedContent;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}