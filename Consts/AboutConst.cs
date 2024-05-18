using System.Reflection;

namespace Sheas_Cealer.Consts;

internal class AboutConst : AboutMultilangConst
{
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version!.ToString()[0..^2];
    public static string DeveloperButtonUrl => "https://www.spacetimee.xyz";
    public static string VersionButtonUrl => "https://spacetime.lanzouu.com/b017hp0lc";
    public static string EmailButtonUrl => "Zeus6_6@163.com";
    public static string InstructionButtonUrl => "https://github.com/SpaceTimee/Sheas-Cealer/wiki/Sheas-Cealer-Instruction";
    public static string OpenSourceButtonUrl => "https://github.com/SpaceTimee/Sheas-Cealer";
    public static string PrivacyButtonUrl => "https://thoughts.teambition.com/share/6264eda98adeb10041b92fda#title=Sheas_Cealer_隐私政策";
    public static string AgreementButtonUrl => "https://thoughts.teambition.com/share/6264edd78adeb10041b92fdb#title=Sheas_Cealer_使用协议";
}