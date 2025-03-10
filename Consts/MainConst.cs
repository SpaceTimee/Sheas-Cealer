using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Sheas_Cealer.Consts;

internal abstract partial class MainConst : MainMultilangConst
{
    internal enum SettingsMode
    { BrowserPathMode, UpstreamUrlMode, ExtraArgsMode }

    public static bool IsAdmin => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    internal static string EdgeBrowserRegistryPath => @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe";
    internal static string ChromeBrowserRegistryPath => @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe";
    internal static string BraveBrowserRegistryPath => @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\brave.exe";

    internal static string DefaultUpstreamUrl => "https://gitlab.com/SpaceTimee/Cealing-Host/raw/main/Cealing-Host.json";
    internal static string CealHostPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Host-*.json");
    internal static string LocalHostPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Host-L.json");
    internal static string UpstreamHostPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Host-U.json");

    internal static string HostsConfPath => Path.Combine(Registry.LocalMachine.OpenSubKey(@"\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\DataBasePath")?.GetValue("DataBasePath", null)?.ToString() ?? @"C:\Windows\System32\drivers\etc", "hosts");
    internal static string HostsConfStartMarker => $"# Cealing Nginx Start{Environment.NewLine}";
    internal static string HostsConfEndMarker => "# Cealing Nginx End";

    internal static string ConginxPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Conginx.exe");
    internal static string NginxPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Nginx.exe");
    internal static string NginxConfPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "nginx.conf");
    internal static string NginxLogsPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "logs");
    internal static string NginxErrorLogsPath => Path.Combine(NginxLogsPath, "error.log");
    internal static string NginxTempPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "temp");
    internal static string NginxCertPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Cert.pem");
    internal static string NginxKeyPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Key.pem");
    internal static string NginxRootCertSubjectName => "CN=Cealing Cert Root";
    internal static string NginxChildCertSubjectName => "CN=Cealing Cert Child";

    internal static string ComihomoPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Comihomo.exe");
    internal static string MihomoPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Mihomo.exe");
    internal static string MihomoConfPath => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "config.yaml");
    internal static string[] MihomoNameServers => ["https://ns.net.kg/dns-query", "https://dnschina1.soraharu.com/dns-query", "https://0ms.dev/dns-query"];

    internal static string NotifyIconText => "Sheas Cealer";

    [GeneratedRegex(@"^(https?:\/\/)?[a-zA-Z0-9](-*[a-zA-Z0-9])*(\.[a-zA-Z0-9](-*[a-zA-Z0-9])*)*(:\d{1,5})?(\/[a-zA-Z0-9.\-_\~\!\$\&\'\(\)\*\+\,\;\=\:\@\%]*)*$")]
    internal static partial Regex UpstreamUrlRegex();

    [GeneratedRegex(@"^(--[a-z](-?[a-z])*(=("".*"")|.*)?( --[a-z](-?[a-z])*(="".*"")?)*)?$")]
    internal static partial Regex ExtraArgsRegex();
}