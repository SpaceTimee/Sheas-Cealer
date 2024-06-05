using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class AboutVersionButtonContentConv : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        string? versionButtonLabelContent = values[0] as string;
        string? versionButtonVersionContent = values[1] as string;

        return $"{versionButtonLabelContent} {versionButtonVersionContent}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}