using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Convs
{
    internal class MainFunctionButtonContentConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MainConst.Mode? mode = value as MainConst.Mode?;

            if (mode == MainConst.Mode.browserPathMode)
                return MainConst.FunctionButtonBrowserPathContent;
            else if (mode == MainConst.Mode.upstreamUrlMode)
                return MainConst.FunctionButtonUpstreamUrlContent;
            else if (mode == MainConst.Mode.extraArgsMode)
                return MainConst.FunctionButtonExtraArgsContent;

            throw new UnreachableException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}