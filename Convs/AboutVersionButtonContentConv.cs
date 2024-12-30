using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class AboutVersionButtonContentConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        string versionButtonLabelContent = (string)values[0];
        string versionButtonVersionContent = (string)values[1];
        bool isSheasCealerUtd = (bool)values[2];

        return $"{versionButtonLabelContent} {versionButtonVersionContent}" + (isSheasCealerUtd ? string.Empty : " *");
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}