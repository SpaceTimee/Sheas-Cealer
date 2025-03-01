using Avalonia.Data.Converters;
using Sheas_Cealer_Nix.Consts;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class MainMihomoButtonContentConv : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isMihomoRunning = (bool)values[0];
        bool isComihomoIniting = (bool)values[1];
        bool isMihomoIniting = (bool)values[2];
        bool isCoproxyIniting = (bool)values[3];
        bool isCoproxyStopping = (bool)values[4];

        return isCoproxyIniting || isCoproxyStopping ? MainConst.MihomoButtonIsInitingContent :
            isComihomoIniting ? MainConst.MihomoButtonIsStoppingContent :
            isMihomoIniting ? MainConst.MihomoButtonIsInitingContent :
            isMihomoRunning ? MainConst.MihomoButtonIsRunningContent : MainConst.MihomoButtonIsStoppedContent;
    }
}