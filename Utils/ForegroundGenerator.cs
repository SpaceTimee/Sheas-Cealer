using System;
using System.Windows.Media;

namespace Sheas_Cealer.Utils;

internal static class ForegroundGenerator
{
    internal static (Color?, Color) GetForeground(int red, int green, int blue)
    {
        double luminance = 0.2126 * GammaCorrect(red / 255.0) + 0.7152 * GammaCorrect(green / 255.0) + 0.0722 * GammaCorrect(blue / 255.0);

        double blackContrast = (luminance + 0.05) / 0.05;
        double whiteContrast = 1.05 / (luminance + 0.05);

        double hue = GetHue(red / 255.0, green / 255.0, blue / 255.0);

        double blueContrast = Math.Min(Math.Abs(hue - 206.57), 360 - Math.Abs(hue - 206.57));
        double redContrast = Math.Min(Math.Abs(hue), 360 - Math.Abs(hue));

        return (blackContrast >= 4 && whiteContrast >= 3 ? null :
            blackContrast >= whiteContrast ? Color.FromRgb(0, 0, 0) : Color.FromRgb(255, 255, 255),
            blueContrast >= redContrast ? (Color)ColorConverter.ConvertFromString("#2196f3") : Color.FromRgb(255, 0, 0));
    }

    private static double GammaCorrect(double component) => component <= 0.03928 ? component / 12.92 : Math.Pow((component + 0.055) / 1.055, 2.4);

    private static double GetHue(double redComponent, double greenComponent, double blueComponent) =>
        redComponent > greenComponent && redComponent > blueComponent ? 60 * ((greenComponent - blueComponent) / redComponent - Math.Min(greenComponent, blueComponent) + (greenComponent < blueComponent ? 6 : 0)) :
        greenComponent > blueComponent && greenComponent > redComponent ? 60 * ((blueComponent - redComponent) / greenComponent - Math.Min(blueComponent, redComponent) + 2) :
        blueComponent > redComponent && blueComponent > greenComponent ? 60 * ((redComponent - greenComponent) / blueComponent - Math.Min(redComponent, greenComponent) + 4) : 0;
}