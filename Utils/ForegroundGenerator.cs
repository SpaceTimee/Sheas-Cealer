using Avalonia.Media;
using Sheas_Cealer_Nix.Consts;
using System;

namespace Sheas_Cealer_Nix.Utils;

internal static class ForegroundGenerator
{
    internal static (Color?, Color) GetForeground(int red, int green, int blue)
    {
        double redComponent = red / 255.0, greenComponent = green / 255.0, blueComponent = blue / 255.0;

        double luminance = 0.2126 * GammaCorrect(redComponent) + 0.7152 * GammaCorrect(greenComponent) + 0.0722 * GammaCorrect(blueComponent);

        double blackContrast = (luminance + 0.05) / 0.05;
        double whiteContrast = 1.05 / (luminance + 0.05);

        double hue = redComponent > greenComponent && redComponent > blueComponent ? 60 * ((greenComponent - blueComponent) / (redComponent - Math.Min(greenComponent, blueComponent)) + (greenComponent < blueComponent ? 6 : 0)) :
            greenComponent > blueComponent && greenComponent > redComponent ? 60 * ((blueComponent - redComponent) / (greenComponent - Math.Min(blueComponent, redComponent)) + 2) :
            blueComponent > redComponent && blueComponent > greenComponent ? 60 * ((redComponent - greenComponent) / (blueComponent - Math.Min(redComponent, greenComponent)) + 4) : 0;

        double blueContrast = Math.Min(Math.Abs(hue - 206.57), 360 - Math.Abs(hue - 206.57));
        double redContrast = Math.Min(Math.Abs(hue - 4.11), 360 - Math.Abs(hue - 4.11));

        return (blackContrast >= 5.5 && whiteContrast >= 2.5 ? null :
            blackContrast >= whiteContrast ? Colors.Black : Colors.White,
            blueContrast >= redContrast ? AboutConst.AccentBlueColor : AboutConst.AccentRedColor);
    }

    private static double GammaCorrect(double component) => component <= 0.03928 ? component / 12.92 : Math.Pow((component + 0.055) / 1.055, 2.4);
}