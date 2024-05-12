using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs
{
    internal class MainStartCealButtonIsEnabledConv : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string? browserPath = values[0] as string;
            string? upstreamUrl = values[1] as string;
            string? extraArgs = values[2] as string;

            if (File.Exists(browserPath) && Path.GetFileName(browserPath).ToLower().EndsWith(".exe") && MainConst.UrlRegex().IsMatch(upstreamUrl!) && MainConst.ArgsRegex().IsMatch(extraArgs!))
                return true;

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}