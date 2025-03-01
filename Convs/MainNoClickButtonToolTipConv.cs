﻿using Avalonia.Data.Converters;
using Sheas_Cealer_Nix.Consts;
using System;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class MainNoClickButtonToolTipConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isFlashing = (bool)value;

        return isFlashing ? MainConst.NoClickButtonIsFlashingToolTip : MainConst.NoClickButtonIsStoppedToolTip;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}