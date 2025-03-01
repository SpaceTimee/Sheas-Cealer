using Avalonia.Data.Converters;
using Sheas_Cealer_Nix.Consts;
using System;
using System.Globalization;

namespace Sheas_Cealer_Nix.Convs;

internal class SettingsWeightsButtonContentConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool? isLightWeight = value as bool?;

        return isLightWeight.HasValue ?
            isLightWeight.GetValueOrDefault() ? SettingsConst.WeightsButtonLightWeightContent : SettingsConst.WeightsButtonBoldWeightContent :
            SettingsConst.WeightsButtonRegularWeightContent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}