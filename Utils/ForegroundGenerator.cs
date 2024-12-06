using System;
using System.Windows.Media;

namespace Sheas_Cealer.Utils;

internal static class ForegroundGenerator
{
    internal static Color? GetForeground(int red, int green, int blue)
    {
        double luminance = 0.2126 * GammaCorrect(red / 255.0) + 0.7152 * GammaCorrect(green / 255.0) + 0.0722 * GammaCorrect(blue / 255.0);

        double blackContrast = (luminance + 0.05) / 0.05;
        double whiteContrast = 1.05 / (luminance + 0.05);

        return blackContrast >= 3.9 && whiteContrast >= 2.9 ? null :
            blackContrast >= whiteContrast ? Color.FromRgb(0, 0, 0) : Color.FromRgb(255, 255, 255);
    }

    private static double GammaCorrect(double c) => c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
}