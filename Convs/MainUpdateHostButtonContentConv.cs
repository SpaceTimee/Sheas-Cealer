using System;
using System.Globalization;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs;

internal class MainUpdateHostButtonContentConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isUpstreamHostUtd = (bool)value;

        return isUpstreamHostUtd ? MainConst.UpdateUpstreamHostButtonContent : $"{MainConst.UpdateUpstreamHostButtonContent} *";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}