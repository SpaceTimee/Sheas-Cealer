using System;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Sheas_Cealer.Consts;

internal partial class MainConst : MainMultilangConst
{
    internal enum SettingsMode
    { BrowserPathMode, UpstreamUrlMode, ExtraArgsMode };

    public static bool IsAdmin => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    internal static string CealingHostPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Host-*.json");
    internal static string HostsConfPath => Path.Combine(Registry.LocalMachine.OpenSubKey(@"\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\DataBasePath")?.GetValue("DataBasePath", null)?.ToString() ?? @"C:\Windows\System32\drivers\etc", "hosts");
    internal static string HostsConfStartMarker => "# Cealing Nginx Start\n";
    internal static string HostsConfEndMarker => "# Cealing Nginx End";
    internal static string NginxConfPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "nginx.conf");
    internal static string NginxLogsPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "logs");
    internal static string NginxTempPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Temp");
    internal static string NginxCertPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Cert.pem");
    internal static string NginxKeyPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Key.pem");
    internal static string MihomoConfPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "config.yaml");
    internal static string DefaultUpstreamUrl => "https://gitlab.com/SpaceTimee/Cealing-Host/raw/main/Cealing-Host.json";

    [GeneratedRegex(@"^(https?:\/\/)?[a-zA-Z0-9](-*[a-zA-Z0-9])*(\.[a-zA-Z0-9](-*[a-zA-Z0-9])*)*(:\d{1,5})?(\/[a-zA-Z0-9.\-_\~\!\$\&\'\(\)\*\+\,\;\=\:\@\%]*)*$")]
    internal static partial Regex UpstreamUrlRegex();

    [GeneratedRegex(@"^(--[a-z](-?[a-z])*(=("".*"")|.*)?( --[a-z](-?[a-z])*(="".*"")?)*)?$")]
    internal static partial Regex ExtraArgsRegex();
}