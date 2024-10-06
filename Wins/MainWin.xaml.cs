using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using IWshRuntimeLibrary;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using NginxConfigParser;
using OnaCore;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using File = System.IO.File;

namespace Sheas_Cealer.Wins;

public partial class MainWin : Window
{
    private static MainPres? MainPres;
    private static readonly HttpClient MainClient = new(new HttpClientHandler() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });
    private static DispatcherTimer? HoldButtonTimer;
    private static readonly DispatcherTimer ProxyTimer = new() { Interval = TimeSpan.FromSeconds(0.1) };
    private static readonly FileSystemWatcher HostWatcher = new(Path.GetDirectoryName(MainConst.CealingHostPath)!, Path.GetFileName(MainConst.CealingHostPath)) { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
    private static readonly FileSystemWatcher ConfWatcher = new(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "nginx.conf") { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
    private static readonly Dictionary<string, List<(List<(string hostIncludeDomain, string hostExcludeDomain)> hostDomainPairs, string hostSni, string hostIp)>> HostRulesDict = [];
    private static string CealArgs = string.Empty;
    private static NginxConfig? NginxConfs;
    private static string? ExtraConfs;
    private static int GameClickTime = 0;
    private static int GameFlashInterval = 1000;

    internal MainWin(string[] args)
    {
        InitializeComponent();

        DataContext = MainPres = new(args);

        ProxyTimer.Tick += ProxyTimer_Tick;
        ProxyTimer.Start();

        HostWatcher.Changed += HostWatcher_Changed;
        ConfWatcher.Changed += ConfWatcher_Changed;
        foreach (string hostPath in Directory.GetFiles(HostWatcher.Path, HostWatcher.Filter))
            HostWatcher_Changed(null!, new(new(), Path.GetDirectoryName(hostPath)!, Path.GetFileName(hostPath)));
    }

    protected override void OnSourceInitialized(EventArgs e) => IconRemover.RemoveIcon(this);
    private void MainWin_Loaded(object sender, RoutedEventArgs e) => SettingsBox.Focus();
    private void MainWin_Closing(object sender, CancelEventArgs e) => Application.Current.Shutdown();

    private void MainWin_DragEnter(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Link : DragDropEffects.None;
        e.Handled = true;
    }
    private void MainWin_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            MainPres!.BrowserPath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
    }

    private void SettingsBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        switch (MainPres!.SettingsMode)
        {
            case MainConst.SettingsMode.BrowserPathMode:
                MainPres.BrowserPath = SettingsBox.Text;
                return;
            case MainConst.SettingsMode.UpstreamUrlMode:
                MainPres.UpstreamUrl = SettingsBox.Text;
                return;
            case MainConst.SettingsMode.ExtraArgsMode:
                MainPres.ExtraArgs = SettingsBox.Text;
                return;
        }
    }
    private void SettingsModeButton_Click(object sender, RoutedEventArgs e)
    {
        MainPres!.SettingsMode = MainPres.SettingsMode switch
        {
            MainConst.SettingsMode.BrowserPathMode => MainConst.SettingsMode.UpstreamUrlMode,
            MainConst.SettingsMode.UpstreamUrlMode => MainConst.SettingsMode.ExtraArgsMode,
            MainConst.SettingsMode.ExtraArgsMode => MainConst.SettingsMode.BrowserPathMode,
            _ => throw new UnreachableException()
        };
    }
    private void SettingsFunctionButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog browserPathDialog = new() { Filter = $"{MainConst._BrowserPathDialogFilterFileType} (*.exe)|*.exe" };

        switch (MainPres!.SettingsMode)
        {
            case MainConst.SettingsMode.BrowserPathMode when browserPathDialog.ShowDialog().GetValueOrDefault():
                SettingsBox.Focus();
                MainPres.BrowserPath = browserPathDialog.FileName;
                return;
            case MainConst.SettingsMode.UpstreamUrlMode:
                MainPres.UpstreamUrl = MainConst.DefaultUpstreamUrl;
                return;
            case MainConst.SettingsMode.ExtraArgsMode:
                MainPres.ExtraArgs = string.Empty;
                return;
        }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (HoldButtonTimer == null || HoldButtonTimer.IsEnabled)
            StartButtonHoldTimer_Tick(null, null!);
    }
    private void StartButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        HoldButtonTimer = new() { Interval = TimeSpan.FromSeconds(1) };
        HoldButtonTimer.Tick += StartButtonHoldTimer_Tick;
        HoldButtonTimer.Start();
    }
    private void StartButtonHoldTimer_Tick(object? sender, EventArgs e)
    {
        HoldButtonTimer?.Stop();

        if (string.IsNullOrWhiteSpace(CealArgs))
            throw new Exception(MainConst._HostErrorMsg);
        if (MessageBox.Show(MainConst._KillBrowserProcessPrompt, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        IWshShortcut uncealedBrowserShortcut = (IWshShortcut)new WshShell().CreateShortcut(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Uncealed-Browser.lnk"));
        uncealedBrowserShortcut.TargetPath = MainPres!.BrowserPath;
        uncealedBrowserShortcut.Description = "Created By Sheas Cealer";
        uncealedBrowserShortcut.Save();

        foreach (Process browserProcess in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainPres.BrowserPath)))
        {
            browserProcess.Kill();
            browserProcess.WaitForExit();
        }

        new CommandProc(sender == null).ShellRun(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, ($"{CealArgs} {MainPres!.ExtraArgs}").Trim());
    }
    private void NginxButton_Click(object sender, RoutedEventArgs e)
    {
        if (HoldButtonTimer == null || HoldButtonTimer.IsEnabled)
            NginxButtonHoldTimer_Tick(null, null!);
    }
    private void NginxButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        HoldButtonTimer = new() { Interval = TimeSpan.FromSeconds(1) };
        HoldButtonTimer.Tick += NginxButtonHoldTimer_Tick;
        HoldButtonTimer.Start();
    }
    private async void NginxButtonHoldTimer_Tick(object? sender, EventArgs e)
    {
        HoldButtonTimer?.Stop();

        string hostsPath = Path.Combine(Registry.LocalMachine.OpenSubKey(@"\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\DataBasePath")?.GetValue("DataBasePath", null)?.ToString() ?? @"C:\Windows\System32\drivers\etc", "hosts");

        if (!MainPres!.IsNginxRunning)
        {
            if (MessageBox.Show(MainConst._LaunchProxyPrompt, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            string configPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "nginx.conf");
            string logsPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "logs");
            string tempPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "temp");

            if (!File.Exists(configPath))
                File.Create(configPath).Dispose();
            if (!Directory.Exists(logsPath))
                Directory.CreateDirectory(logsPath);
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            RSA certKey = RSA.Create(2048);

            CertificateRequest rootCertRequest = new("CN=Cealing Cert Root", certKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            rootCertRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, false));

            X509Certificate2 rootCert = rootCertRequest.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(100));

            CertificateRequest childCertRequest = new("CN=Cealing Cert Child", certKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            SubjectAlternativeNameBuilder childCertSanBuilder = new();
            foreach (List<(List<(string hostIncludeDomain, string hostExcludeDomain)> hostDomainPairs, string hostSni, string hostIp)> hostRules in HostRulesDict.Values)
                foreach ((List<(string hostIncludeDomain, string hostExcludeDomain)> hostDomainPairs, _, _) in hostRules)
                    foreach ((string hostIncludeDomain, _) in hostDomainPairs)
                    {
                        if (hostIncludeDomain.StartsWith("*."))
                        {
                            childCertSanBuilder.AddDnsName("*" + hostIncludeDomain.Replace("*", string.Empty));
                            continue;
                        }
                        else if (hostIncludeDomain.StartsWith('*'))
                            childCertSanBuilder.AddDnsName("*." + hostIncludeDomain.Replace("*", string.Empty));

                        childCertSanBuilder.AddDnsName(hostIncludeDomain.Replace("*", string.Empty));
                    }

            childCertRequest.CertificateExtensions.Add(childCertSanBuilder.Build());

            X509Certificate2 childCert = childCertRequest.Create(rootCert, rootCert.NotBefore, rootCert.NotAfter, Guid.NewGuid().ToByteArray());

            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "cert.pem"), childCert.ExportCertificatePem());
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "key.pem"), certKey.ExportPkcs8PrivateKeyPem());

            using X509Store certStore = new(StoreName.Root, StoreLocation.CurrentUser, OpenFlags.ReadWrite);

            foreach (X509Certificate2 cert in certStore.Certificates)
                if (cert.Subject == "CN=Cealing Cert Root")
                    certStore.Remove(cert);

            certStore.Add(rootCert);
            certStore.Close();

            string hostsAppendContent = "# Cealing Nginx Start\n";

            foreach (List<(List<(string hostIncludeDomain, string hostExcludeDomain)> hostDomainPairs, string hostSni, string hostIp)> hostRules in HostRulesDict.Values)
                foreach ((List<(string hostIncludeDomain, string hostExcludeDomain)> hostDomainPairs, string hostSni, string hostIp) in hostRules)
                    foreach ((string hostIncludeDomain, string hostExcludeDomain) in hostDomainPairs)
                    {
                        string hostIncludeDomainWithoutWildcard = hostIncludeDomain.Replace("*", string.Empty);

                        if (hostIncludeDomainWithoutWildcard.StartsWith('^') || hostIncludeDomainWithoutWildcard.EndsWith('^') ||
                            hostIncludeDomainWithoutWildcard.StartsWith('.') || hostIncludeDomainWithoutWildcard.EndsWith('.'))
                            continue;

                        hostsAppendContent += $"127.0.0.1 {hostIncludeDomainWithoutWildcard.Split('^', 2)[0]}\n";

                        if (hostIncludeDomain.StartsWith('*'))
                            hostsAppendContent += $"127.0.0.1 www.{hostIncludeDomainWithoutWildcard.Split('^', 2)[0]}\n";
                    }

            hostsAppendContent += "# Cealing Nginx End";

            File.AppendAllText(hostsPath, hostsAppendContent);

            ConfWatcher.EnableRaisingEvents = false;
            NginxConfs!.Save("nginx.conf");

            new NginxProc().ShellRun(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, @"-c nginx.conf");

            while (true)
                try
                {
                    await Http.GetAsync<HttpResponseMessage>("https://localhost", MainClient);
                    break;
                }
                catch (HttpRequestException ex) when (ex.InnerException is SocketException innerEx)
                {
                    if (innerEx.SocketErrorCode != SocketError.ConnectionRefused)
                        break;
                }

            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "nginx.conf"), ExtraConfs);
            ConfWatcher.EnableRaisingEvents = true;

            if (sender == null)
                Application.Current.Dispatcher.InvokeShutdown();
        }
        else
        {
            string hostsContent = File.ReadAllText(hostsPath);
            int cealingNginxStartIndex = hostsContent.IndexOf("# Cealing Nginx Start\n");
            int cealingNginxEndIndex = hostsContent.LastIndexOf("# Cealing Nginx End");

            if (cealingNginxStartIndex != -1 && cealingNginxEndIndex != -1)
                File.WriteAllText(hostsPath, hostsContent.Remove(cealingNginxStartIndex, cealingNginxEndIndex - cealingNginxStartIndex + "# Cealing Nginx End".Length));

            foreach (Process nginxProcess in Process.GetProcessesByName("Cealing-Nginx"))
            {
                nginxProcess.Kill();
                nginxProcess.WaitForExit();
            }
        }
    }
    private void MihomoButton_Click(object sender, RoutedEventArgs e)
    {
        if (HoldButtonTimer == null || HoldButtonTimer.IsEnabled)
            MihomoButtonHoldTimer_Tick(null, null!);
    }
    private void MihomoButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        HoldButtonTimer = new() { Interval = TimeSpan.FromSeconds(1) };
        HoldButtonTimer.Tick += MihomoButtonHoldTimer_Tick;
        HoldButtonTimer.Start();
    }
    private async void MihomoButtonHoldTimer_Tick(object? sender, EventArgs e)
    {
        HoldButtonTimer?.Stop();

        RegistryKey proxyKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true)!;

        if (!MainPres!.IsMihomoRunning)
        {
            if (MessageBox.Show(MainConst._LaunchProxyPrompt, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            string confPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "config.yaml");

            if (!File.Exists(confPath))
                File.Create(confPath).Dispose();

            string extraConfs = File.ReadAllText(confPath);

            Dictionary<string, object> mihomoConfs = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build().Deserialize<Dictionary<string, object>>(extraConfs) ?? []; ;

            mihomoConfs["mixed-port"] = 7880;
            mihomoConfs["ipv6"] = true;
            mihomoConfs["log-level"] = "silent";
            mihomoConfs["dns"] = new
            {
                enable = true,
                listen = ":53",
                enhancedMode = "redir-host",
                nameserver = new[] { "https://doh.apad.pro/dns-query" }
            };
            mihomoConfs["tun"] = new
            {
                enable = true,
                stack = "system",
                autoRoute = true,
                autoDetectInterface = true,
                dnsHijack = new[] { "any:53", "tcp://any:53" }
            };
            mihomoConfs["rules"] = new[] { "MATCH,DIRECT" };

            File.WriteAllText(confPath, new SerializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build().Serialize(mihomoConfs));

            proxyKey.SetValue("ProxyEnable", 1);
            proxyKey.SetValue("ProxyServer", "127.0.0.1:7880");

            new MihomoProc().ShellRun(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "-d .");

            while (true)
                try
                {
                    await Http.GetAsync<HttpResponseMessage>("http://localhost:7880", MainClient);
                    break;
                }
                catch (HttpRequestException ex) when (ex.InnerException is SocketException innerEx)
                {
                    if (innerEx.SocketErrorCode != SocketError.ConnectionRefused)
                        break;
                }

            File.WriteAllText(confPath, extraConfs);

            if (sender == null)
                Application.Current.Dispatcher.InvokeShutdown();
        }
        else
        {
            proxyKey.SetValue("ProxyEnable", 0);

            foreach (Process mihomoProcess in Process.GetProcessesByName("Cealing-Mihomo"))
            {
                mihomoProcess.Kill();
                mihomoProcess.WaitForExit();
            }
        }
    }

    private void EditHostButton_Click(object sender, RoutedEventArgs e)
    {
        Button? senderButton = sender as Button;

        string hostPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, senderButton == EditLocalHostButton ? "Cealing-Host-Local.json" : "Cealing-Host-Upstream.json");

        if (!File.Exists(hostPath))
            File.Create(hostPath).Dispose();

        ProcessStartInfo processStartInfo = new(hostPath) { UseShellExecute = true };
        Process.Start(processStartInfo);
    }
    private async void UpdateUpstreamHostButton_Click(object sender, RoutedEventArgs e)
    {
        string newUpstreamHostUrl = (MainPres!.UpstreamUrl.StartsWith("http://") || MainPres!.UpstreamUrl.StartsWith("https://") ? string.Empty : "https://") + MainPres!.UpstreamUrl;
        string newUpstreamHostString = await Http.GetAsync<string>(newUpstreamHostUrl, MainClient);
        string oldUpstreamHostPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Host-Upstream.json");
        string oldUpstreamHostString;

        if (!File.Exists(oldUpstreamHostPath))
            File.Create(oldUpstreamHostPath).Dispose();

        oldUpstreamHostString = File.ReadAllText(oldUpstreamHostPath);

        if (oldUpstreamHostString.Replace("\r", string.Empty) == newUpstreamHostString)
            MessageBox.Show(MainConst._UpstreamHostUtdMsg);
        else
        {
            MessageBoxResult overrideOptionResult = MessageBox.Show(MainConst._OverrideUpstreamHostPrompt, string.Empty, MessageBoxButton.YesNoCancel);
            if (overrideOptionResult == MessageBoxResult.Yes)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Host-Upstream.json"), newUpstreamHostString);
                MessageBox.Show(MainConst._UpdateUpstreamHostSuccessMsg);
            }
            else if (overrideOptionResult == MessageBoxResult.No)
                Process.Start(new ProcessStartInfo(newUpstreamHostUrl) { UseShellExecute = true });
        }
    }
    private void ThemesButton_Click(object sender, RoutedEventArgs e) => MainPres!.IsLightTheme = MainPres.IsLightTheme.HasValue ? MainPres.IsLightTheme.Value ? null : true : false;
    private void AboutButton_Click(object sender, RoutedEventArgs e) => new AboutWin().ShowDialog();

    private void EditConfButton_Click(object sender, RoutedEventArgs e)
    {
        Button? senderButton = sender as Button;

        string confPath = senderButton == EditHostsConfButton ? Path.Combine(Registry.LocalMachine.OpenSubKey(@"\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\DataBasePath")?.GetValue("DataBasePath", null)?.ToString() ?? @"C:\Windows\System32\drivers\etc", "hosts") :
            Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, senderButton == EditNginxConfButton ? "nginx.conf" : "config.yaml");

        if (!File.Exists(confPath))
            File.Create(confPath).Dispose();

        ProcessStartInfo processStartInfo = new(confPath) { UseShellExecute = true };
        Process.Start(processStartInfo);
    }
    private async void NoClickButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameFlashInterval <= 10)
        {
            MessageBox.Show(MainConst._GameReviewEndingMsg);
            return;
        }

        ++GameClickTime;

        switch (GameClickTime)
        {
            case 1:
                MessageBox.Show(MainConst._GameClickOnceMsg);
                return;
            case 2:
                MessageBox.Show(MainConst._GameClickTwiceMsg);
                return;
            case 3:
                MessageBox.Show(MainConst._GameClickThreeMsg);
                return;
        }

        if (!MainPres!.IsFlashing)
        {
            MessageBox.Show(MainConst._GameStartMsg);
            MainPres.IsFlashing = true;

            Random random = new();

            while (GameFlashInterval > 10)
            {
                Left = random.Next(0, (int)(SystemParameters.PrimaryScreenWidth - ActualWidth));
                Top = random.Next(0, (int)(SystemParameters.PrimaryScreenHeight - ActualHeight));

                PaletteHelper paletteHelper = new();
                Theme newTheme = paletteHelper.GetTheme();

                newTheme.SetPrimaryColor(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
                newTheme.SetBaseTheme(random.Next(2) == 0 ? BaseTheme.Light : BaseTheme.Dark);
                paletteHelper.SetTheme(newTheme);

                if (GameFlashInterval > 100)
                    GameFlashInterval += random.Next(1, 4);

                await Task.Delay(GameFlashInterval);
            }

            MainPres.IsFlashing = false;
            MessageBox.Show(MainConst._GameEndingMsg);
        }
        else
        {
            switch (GameFlashInterval)
            {
                case > 250:
                    GameFlashInterval -= 150;
                    break;
                case > 100:
                    GameFlashInterval = 100;
                    break;
                case > 10:
                    GameFlashInterval -= 30;
                    break;
            }

            if (GameFlashInterval > 10)
                MessageBox.Show($"{MainConst._GameGradeMsg} {GameFlashInterval}");
        }
    }

    private void ProxyTimer_Tick(object? sender, EventArgs e)
    {
        MainPres!.IsNginxExist = File.Exists(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Nginx.exe"));
        MainPres.IsNginxRunning = Process.GetProcessesByName("Cealing-Nginx").Length != 0;
        MainPres.IsMihomoExist = File.Exists(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Mihomo.exe"));
        MainPres.IsMihomoRunning = Process.GetProcessesByName("Cealing-Mihomo").Length != 0;
    }
    private void HostWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        string hostName = e.Name!.TrimStart("Cealing-Host-".ToCharArray()).TrimEnd(".json".ToCharArray());

        try
        {
            HostRulesDict[hostName] = [];
            string hostRulesFragments = string.Empty;
            string hostResolverRulesFragments = string.Empty;

            using FileStream hostStream = new(e.FullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            JsonDocumentOptions hostOptions = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
            JsonElement hostArray = JsonDocument.Parse(hostStream, hostOptions).RootElement;

            foreach (JsonElement hostRule in hostArray.EnumerateArray())
            {
                List<(string hostIncludeDomain, string hostExcludeDomain)> hostDomainPairs = [];
                string hostSni = string.IsNullOrWhiteSpace(hostRule[1].ToString()) ? $"{hostName}{HostRulesDict[hostName].Count}" : hostRule[1].ToString();
                string hostIp = string.IsNullOrWhiteSpace(hostRule[2].ToString()) ? "127.0.0.1" : hostRule[2].ToString();

                foreach (JsonElement hostDomain in hostRule[0].EnumerateArray())
                {
                    if (hostDomain.ToString().StartsWith('^') || hostDomain.ToString().EndsWith('^'))
                        continue;

                    string[] hostDomainPair = hostDomain.ToString().Split('^', 2);

                    hostDomainPairs.Add((hostDomainPair[0], hostDomainPair.Length == 2 ? hostDomainPair[1] : string.Empty));
                }

                HostRulesDict[hostName].Add((hostDomainPairs, hostSni, hostIp));
            }
        }
        catch { HostRulesDict.Remove(hostName); }
        finally
        {
            string cealHostRules = string.Empty;
            string cealHostResolverRules = string.Empty;

            foreach (List<(List<(string hostIncludeDomain, string hostExcludeDomain)> hostDomainPairs, string hostSni, string hostIp)> hostRules in HostRulesDict.Values)
                foreach ((List<(string hostIncludeDomain, string hostExcludeDomain)> hostDomainPairs, string hostSni, string hostIp) in hostRules)
                {
                    foreach ((string hostIncludeDomain, string hostExcludeDomain) in hostDomainPairs)
                        cealHostRules += $"MAP {hostIncludeDomain} {hostSni}," + (!string.IsNullOrWhiteSpace(hostExcludeDomain) ? $"EXCLUDE {hostExcludeDomain}," : string.Empty);

                    cealHostResolverRules += $"MAP {hostSni} {hostIp},";
                }

            CealArgs = @$"/c @start .\""Uncealed-Browser.lnk"" --host-rules=""{cealHostRules.TrimEnd(',')}"" --host-resolver-rules=""{cealHostResolverRules.TrimEnd(',')}"" --test-type --ignore-certificate-errors";

            ConfWatcher_Changed(null!, new(new(), ConfWatcher.Path, ConfWatcher.Filter));
        }
    }
    private void ConfWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        if (MainConst.IsAdmin && MainPres!.IsNginxExist)
        {
            ExtraConfs = File.ReadAllText(e.FullPath);
            int ruleIndex = 1;

            NginxConfs = NginxConfig.Load(ExtraConfs)
                .AddOrUpdate("worker_processes", "auto")
                .AddOrUpdate("events:worker_connections", "65536")
                .AddOrUpdate("http:proxy_set_header", "Host $http_host")
                .AddOrUpdate("http:proxy_ssl_server_name", "on")
                .AddOrUpdate("http:server:return", "https://$host$request_uri");

            foreach (List<(List<(string hostIncludeDomain, string hostExcludeDomain)> hostDomainPairs, string hostSni, string hostIp)> hostRules in HostRulesDict.Values)
                foreach ((List<(string hostIncludeDomain, string hostExcludeDomain)> hostDomainPairs, string hostSni, string hostIp) in hostRules)
                {
                    string serverName = "~";

                    foreach ((string hostIncludeDomain, string hostExcludeDomain) in hostDomainPairs)
                        serverName += "^" + (!string.IsNullOrWhiteSpace(hostExcludeDomain) ? $"(?!{hostExcludeDomain.Replace(".", "\\.").Replace("*", ".*")})" : string.Empty) + hostIncludeDomain.Replace(".", "\\.").Replace("*", ".*") + "$|";

                    NginxConfs = NginxConfs
                        .AddOrUpdate($"http:server[{ruleIndex}]:server_name", serverName.TrimEnd('|'))
                        .AddOrUpdate($"http:server[{ruleIndex}]:listen", "443 ssl")
                        .AddOrUpdate($"http:server[{ruleIndex}]:ssl_certificate", "cert.pem")
                        .AddOrUpdate($"http:server[{ruleIndex}]:ssl_certificate_key", "key.pem")
                        .AddOrUpdate($"http:server[{ruleIndex}]:proxy_ssl_name", hostSni)
                        .AddOrUpdate($"http:server[{ruleIndex}]:location", "/", true)
                        .AddOrUpdate($"http:server[{ruleIndex}]:location:proxy_pass", $"https://{hostIp}");

                    ++ruleIndex;
                }
        }
    }
    private void MainWin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.W)
            Application.Current.Shutdown();
    }
}