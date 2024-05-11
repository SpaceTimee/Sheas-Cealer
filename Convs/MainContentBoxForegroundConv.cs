using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs
{
    internal class MainContentBoxForegroundConv : IMultiValueConverter
    {
        private static readonly MainConst MainConst = new();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            MainConst.Mode? mode = values[0] as MainConst.Mode?;
            bool? isFocused = values[1] as bool?;
            string? browserPath = values[2] as string;
            string? upstreamUrl = values[3] as string;
            string? extraArgs = values[4] as string;

            if (!(bool)isFocused!)
            {
                switch (mode)
                {
                    case MainConst.Mode.browserPathMode:
                        if (browserPath == MainConst.BrowserPathPlaceHolder)
                            return new SolidColorBrush(Color.FromRgb(191, 205, 219));
                        break;
                    case MainConst.Mode.upstreamUrlMode:
                        if (upstreamUrl == MainConst.UpstreamUrlPlaceHolder)
                            return new SolidColorBrush(Color.FromRgb(191, 205, 219));
                        break;
                    case MainConst.Mode.extraArgsMode:
                        if (extraArgs == MainConst.ExtraArgsPlaceHolder)
                            return new SolidColorBrush(Color.FromRgb(191, 205, 219));
                        break;
                }
            }

            return new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}