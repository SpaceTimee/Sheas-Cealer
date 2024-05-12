using System.Text.RegularExpressions;

namespace Sheas_Cealer.Consts
{
    internal partial class MainConst : MainMultilangConst
    {
        internal enum Mode
        { browserPathMode, upstreamUrlMode, extraArgsMode };

        internal static string DefaultUpstreamUrl => "https://gitlab.com/SpaceTimee/Cealing-Host/raw/main/Cealing-Host.json";

        [GeneratedRegex(@"^\r$")]
        internal static partial Regex HostRegex();

        [GeneratedRegex(@"^((((ht|f)tps?):\/\/)?[a-zA-Z0-9](-*[a-zA-Z0-9])*(\.[a-zA-Z0-9](-*[a-zA-Z0-9])*)*(:\d{1,5})?(\/[a-zA-Z0-9.\-_\~\!\$\&\'\(\)\*\+\,\;\=\:\@\%]*)*)?$")]
        internal static partial Regex UrlRegex();

        [GeneratedRegex(@"^(--[a-z](-?[a-z])*( --[a-z](-?[a-z])*)*)?$")]
        internal static partial Regex ArgsRegex();
    }
}