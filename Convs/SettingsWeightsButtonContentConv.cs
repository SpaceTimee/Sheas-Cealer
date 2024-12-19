using Sheas_Cealer.Consts;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class SettingsWeightsButtonContentConv : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        bool? isLightWeight = value as bool?;

        return isLightWeight.HasValue ?
            isLightWeight.GetValueOrDefault() ? SettingsConst.WeightsButtonLightWeightContent : SettingsConst.WeightsButtonBoldWeightContent :
            SettingsConst.WeightsButtonRegularWeightContent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}