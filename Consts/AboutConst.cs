using System.Reflection;
using System.Windows.Media;

namespace Sheas_Cealer.Consts;

internal abstract class AboutConst : AboutMultilangConst
{
    public static Color AccentBlueColor => (Color)ColorConverter.ConvertFromString("#2196F3");
    public static Color AccentRedColor => (Color)ColorConverter.ConvertFromString("#F44336");

    public static string DeveloperButtonUrl => "https://www.spacetimee.xyz";
    public static string VersionButtonVersionContent => Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];
    public static string VersionButtonUrl => "https://spacetime.lanzouu.com/b017hp0lc";
    public static string EmailButtonUrl => "Zeus6_6@163.com";

    public static string DocumentationButtonUrl => "https://github.com/SpaceTimee/Sheas-Cealer/wiki/Sheas-Cealer-Documentation";
    public static string RepositoryButtonUrl => "https://github.com/SpaceTimee/Sheas-Cealer";

    public static string PolicyButtonUrl => "https://thoughts.teambition.com/share/6264eda98adeb10041b92fda#title=Sheas_Cealer_隐私政策";
    public static string AgreementButtonUrl => "https://thoughts.teambition.com/share/6264edd78adeb10041b92fdb#title=Sheas_Cealer_使用协议";

    internal static string ReleaseApiUrl => "https://api.github.com/repos/SpaceTimee/Sheas-Cealer/releases/latest";
    internal static string ReleaseApiUserAgent => "Sheas-Cealer";
}