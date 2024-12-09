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

        return $"{versionButtonLabelContent} {versionButtonVersionContent}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}