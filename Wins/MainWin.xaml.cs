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
    private static readonly FileSystemWatcher CealHostWatcher = new(Path.GetDirectoryName(MainConst.CealHostPath)!, Path.GetFileName(MainConst.CealHostPath)) { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
    private static readonly FileSystemWatcher NginxConfWatcher = new(Path.GetDirectoryName(MainConst.NginxConfPath)!, Path.GetFileName(MainConst.NginxConfPath)) { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
    private static readonly FileSystemWatcher MihomoConfWatcher = new(Path.GetDirectoryName(MainConst.MihomoConfPath)!, Path.GetFileName(MainConst.MihomoConfPath)) { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };

    private static readonly SortedDictionary<string, List<(List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp)>> CealHostRulesDict = [];
    private static string CealArgs = string.Empty;
    private static NginxConfig? NginxConfs;
    private static string? ExtraNginxConfs;
    private static string? MihomoConfs;
    private static string? ExtraMihomoConfs;

    private static int GameClickTime = 0;
    private static int GameFlashInterval = 1000;

    internal MainWin(string[] args)
    {
        InitializeComponent();

        DataContext = MainPres = new(args);
    }
    protected override void OnSourceInitialized(EventArgs e) => IconRemover.RemoveIcon(this);
    private async void MainWin_Loaded(object sender, RoutedEventArgs e)
    {
        SettingsBox.Focus();

        await Task.Run(() =>
        {
            ProxyTimer.Tick += ProxyTimer_Tick;
            CealHostWatcher.Changed += CealHostWatcher_Changed;
            NginxConfWatcher.Changed += NginxConfWatcher_Changed;
            MihomoConfWatcher.Changed += MihomoConfWatcher_Changed;

            ProxyTimer.Start();

            foreach (string cealHostPath in Directory.GetFiles(CealHostWatcher.Path, CealHostWatcher.Filter))
                CealHostWatcher_Changed(null!, new(new(), Path.GetDirectoryName(cealHostPath)!, Path.GetFileName(cealHostPath)));

            MihomoConfWatcher_Changed(null!, null!);
        });
    }
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
    private async void StartButtonHoldTimer_Tick(object? sender, EventArgs e)
    {
        HoldButtonTimer?.Stop();

        if (string.IsNullOrWhiteSpace(CealArgs))
            throw new Exception(MainConst._HostErrorMsg);
        if (MessageBox.Show(MainConst._KillBrowserProcessPrompt, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        foreach (Process browserProcess in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainPres!.BrowserPath)))
        {
            browserProcess.Kill();
            await browserProcess.WaitForExitAsync();
        }

        await Task.Run(() =>
        {
            new BrowserProc(MainPres.BrowserPath, sender == null).ShellRun(Path.GetDirectoryName(MainPres.BrowserPath), ($"{CealArgs} {MainPres.ExtraArgs}").Trim());
        });
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

        if (!MainPres!.IsNginxRunning)
        {
            if (string.IsNullOrWhiteSpace(CealArgs))
                throw new Exception(MainConst._HostErrorMsg);
            if (MessageBox.Show(MainConst._LaunchProxyPrompt, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            if (MainPres.IsFlashing && MessageBox.Show(MainConst._LaunchNginxFlashingPrompt, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            if (!File.Exists(MainConst.NginxConfPath))
                File.Create(MainConst.NginxConfPath).Dispose();
            if (!Directory.Exists(MainConst.NginxLogsPath))
                Directory.CreateDirectory(MainConst.NginxLogsPath);
            if (!Directory.Exists(MainConst.NginxTempPath))
                Directory.CreateDirectory(MainConst.NginxTempPath);

            RSA certKey = RSA.Create(2048);

            CertificateRequest rootCertRequest = new(MainConst.NginxRootCertSubjectName, certKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            rootCertRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, false));
            using X509Certificate2 rootCert = rootCertRequest.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(100));
            using X509Store certStore = new(StoreName.Root, StoreLocation.CurrentUser, OpenFlags.ReadWrite);

            certStore.Add(rootCert);
            certStore.Close();

            CertificateRequest childCertRequest = new(MainConst.NginxChildCertSubjectName, certKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            SubjectAlternativeNameBuilder childCertSanBuilder = new();
            string hostsConfAppendContent = MainConst.HostsConfStartMarker;

            foreach (List<(List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp)> cealHostRules in CealHostRulesDict.Values)
                foreach ((List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, _, _) in cealHostRules)
                    foreach ((string cealHostIncludeDomain, _) in cealHostDomainPairs)
                    {
                        string cealHostIncludeDomainWithoutWildcard = cealHostIncludeDomain.TrimStart('$').TrimStart('*').TrimStart('.');

                        if (cealHostIncludeDomain.StartsWith('#') || cealHostIncludeDomainWithoutWildcard.Contains('*') || string.IsNullOrWhiteSpace(cealHostIncludeDomainWithoutWildcard))
                            continue;

                        if (cealHostIncludeDomain.TrimStart('$').StartsWith('*'))
                        {
                            childCertSanBuilder.AddDnsName($"*.{cealHostIncludeDomainWithoutWildcard}");
                            hostsConfAppendContent += $"127.0.0.1 www.{cealHostIncludeDomainWithoutWildcard}\n";

                            if (cealHostIncludeDomain.TrimStart('$').StartsWith("*."))
                                continue;
                        }

                        childCertSanBuilder.AddDnsName(cealHostIncludeDomainWithoutWildcard);
                        hostsConfAppendContent += $"127.0.0.1 {cealHostIncludeDomainWithoutWildcard}\n";
                    }

            childCertRequest.CertificateExtensions.Add(childCertSanBuilder.Build());
            using X509Certificate2 childCert = childCertRequest.Create(rootCert, rootCert.NotBefore, rootCert.NotAfter, Guid.NewGuid().ToByteArray());

            File.WriteAllText(MainConst.NginxCertPath, childCert.ExportCertificatePem());
            File.WriteAllText(MainConst.NginxKeyPath, certKey.ExportPkcs8PrivateKeyPem());

            hostsConfAppendContent += MainConst.HostsConfEndMarker;

            File.AppendAllText(MainConst.HostsConfPath, hostsConfAppendContent);

            MainPres.IsNginxIniting = true;
            NginxConfWatcher.EnableRaisingEvents = false;
            NginxConfs!.Save(MainConst.NginxConfPath);

            await Task.Run(() =>
            {
                new NginxProc().ShellRun(Path.GetDirectoryName(MainConst.NginxPath), @$"-c ""{MainConst.NginxConfPath}""");
            });

            while (true)
            {
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

                if (!MainPres.IsNginxRunning)
                    break;
            }

            File.WriteAllText(MainConst.NginxConfPath, ExtraNginxConfs);
            NginxConfWatcher.EnableRaisingEvents = true;
            MainPres.IsNginxIniting = false;

            if (sender == null)
                Application.Current.Dispatcher.InvokeShutdown();
        }
        else
            foreach (Process nginxProcess in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainConst.NginxPath)))
            {
                nginxProcess.Kill();
                await nginxProcess.WaitForExitAsync();
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

        if (!MainPres!.IsMihomoRunning)
        {
            if (string.IsNullOrWhiteSpace(MihomoConfs))
                throw new Exception(MainConst._ConfErrorMsg);
            if (MessageBox.Show(MainConst._LaunchProxyPrompt, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            if (!File.Exists(MainConst.MihomoConfPath))
                File.Create(MainConst.MihomoConfPath).Dispose();

            MainPres.IsMihomoIniting = true;
            MihomoConfWatcher.EnableRaisingEvents = false;
            File.WriteAllText(MainConst.MihomoConfPath, MihomoConfs);

            await Task.Run(() =>
            {
                new MihomoProc().ShellRun(Path.GetDirectoryName(MainConst.MihomoPath), @$"-d ""{Path.GetDirectoryName(MainConst.MihomoConfPath)}""");
            });

            while (true)
            {
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

                if (!MainPres.IsMihomoRunning)
                    break;
            }

            File.WriteAllText(MainConst.MihomoConfPath, ExtraMihomoConfs);
            MihomoConfWatcher.EnableRaisingEvents = true;
            MainPres.IsMihomoIniting = false;

            if (sender == null)
                Application.Current.Dispatcher.InvokeShutdown();
        }
        else
            foreach (Process mihomoProcess in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainConst.MihomoPath)))
            {
                mihomoProcess.Kill();
                await mihomoProcess.WaitForExitAsync();
            }
    }

    private void EditHostButton_Click(object sender, RoutedEventArgs e)
    {
        Button? senderButton = sender as Button;

        string cealHostPath = senderButton == EditLocalHostButton ? MainConst.LocalHostPath : MainConst.UpstreamHostPath;

        if (!File.Exists(cealHostPath))
            File.Create(cealHostPath).Dispose();

        ProcessStartInfo processStartInfo = new(cealHostPath) { UseShellExecute = true };
        Process.Start(processStartInfo);
    }
    private void EditConfButton_Click(object sender, RoutedEventArgs e)
    {
        Button? senderButton = sender as Button;

        string confPath = senderButton == EditHostsConfButton ? MainConst.HostsConfPath :
            senderButton == EditNginxConfButton ? MainConst.NginxConfPath : MainConst.MihomoConfPath;

        if (!File.Exists(confPath))
            File.Create(confPath).Dispose();

        ProcessStartInfo processStartInfo = new(confPath) { UseShellExecute = true };
        Process.Start(processStartInfo);
    }
    private async void UpdateUpstreamHostButton_Click(object sender, RoutedEventArgs e)
    {
        string upstreamUpstreamHostUrl = (MainPres!.UpstreamUrl.StartsWith("http://") || MainPres!.UpstreamUrl.StartsWith("https://") ? string.Empty : "https://") + MainPres!.UpstreamUrl;
        string upstreamUpstreamHostString = await Http.GetAsync<string>(upstreamUpstreamHostUrl, MainClient);
        string localUpstreamHostString;

        if (!File.Exists(MainConst.UpstreamHostPath))
            File.Create(MainConst.UpstreamHostPath).Dispose();

        localUpstreamHostString = File.ReadAllText(MainConst.UpstreamHostPath);

        if (localUpstreamHostString.Replace("\r", string.Empty) == upstreamUpstreamHostString)
            MessageBox.Show(MainConst._UpstreamHostUtdMsg);
        else
        {
            MessageBoxResult overrideOptionResult = MessageBox.Show(MainConst._OverrideUpstreamHostPrompt, string.Empty, MessageBoxButton.YesNoCancel);
            if (overrideOptionResult == MessageBoxResult.Yes)
            {
                File.WriteAllText(MainConst.UpstreamHostPath, upstreamUpstreamHostString);
                MessageBox.Show(MainConst._UpdateUpstreamHostSuccessMsg);
            }
            else if (overrideOptionResult == MessageBoxResult.No)
                Process.Start(new ProcessStartInfo(upstreamUpstreamHostUrl) { UseShellExecute = true });
        }
    }

    private void ThemesButton_Click(object sender, RoutedEventArgs e) => MainPres!.IsLightTheme = MainPres.IsLightTheme.HasValue ? MainPres.IsLightTheme.Value ? null : true : false;
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
            NginxConfWatcher_Changed(null!, null!);

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
            NginxConfWatcher_Changed(null!, null!);
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
    private void AboutButton_Click(object sender, RoutedEventArgs e) => new AboutWin().ShowDialog();

    private void ProxyTimer_Tick(object? sender, EventArgs e)
    {
        MainPres!.IsNginxExist = File.Exists(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, Path.GetFileName(MainConst.NginxPath)));
        MainPres.IsNginxRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainConst.NginxPath)).Length != 0;
        MainPres.IsMihomoExist = File.Exists(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, Path.GetFileName(MainConst.MihomoPath)));
        MainPres.IsMihomoRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainConst.MihomoPath)).Length != 0;
    }
    private void CealHostWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        string cealHostName = e.Name!.TrimStart("Cealing-Host-".ToCharArray()).TrimEnd(".json".ToCharArray());

        try
        {
            CealHostRulesDict[cealHostName] = [];
            string cealHostRulesFragments = string.Empty;
            string cealHostResolverRulesFragments = string.Empty;

            using FileStream cealHostStream = new(e.FullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            JsonDocumentOptions cealHostOptions = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
            JsonElement cealHostArray = JsonDocument.Parse(cealHostStream, cealHostOptions).RootElement;

            foreach (JsonElement cealHostRule in cealHostArray.EnumerateArray())
            {
                List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs = [];
                string? cealHostSni = cealHostRule[1].ValueKind == JsonValueKind.Null ? null :
                    string.IsNullOrWhiteSpace(cealHostRule[1].ToString()) ? $"{cealHostName}{CealHostRulesDict[cealHostName].Count}" : cealHostRule[1].ToString().Trim();
                string cealHostIp = string.IsNullOrWhiteSpace(cealHostRule[2].ToString()) ? "127.0.0.1" : cealHostRule[2].ToString().Trim();

                foreach (JsonElement cealHostDomain in cealHostRule[0].EnumerateArray())
                {
                    if (cealHostDomain.ToString().StartsWith('^') || string.IsNullOrWhiteSpace(cealHostDomain.ToString().TrimStart('#')) || string.IsNullOrWhiteSpace(cealHostDomain.ToString().TrimStart('$')))
                        continue;

                    string[] cealHostDomainPair = cealHostDomain.ToString().Split('^', 2);

                    cealHostDomainPairs.Add((cealHostDomainPair[0].Trim(), cealHostDomainPair.Length == 2 ? cealHostDomainPair[1].Trim() : string.Empty));
                }

                CealHostRulesDict[cealHostName].Add((cealHostDomainPairs, cealHostSni, cealHostIp));
            }
        }
        catch { CealHostRulesDict.Remove(cealHostName); }
        finally
        {
            string hostRules = string.Empty;
            string hostResolverRules = string.Empty;
            int nullSniNum = 0;

            foreach (List<(List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp)> cealHostRules in CealHostRulesDict.Values)
                foreach ((List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp) in cealHostRules)
                {
                    string cealHostSniWithoutNull = cealHostSni ?? $"{cealHostName}{CealHostRulesDict[cealHostName].Count + ++nullSniNum}";
                    bool isValidCealHostDomainExist = false;

                    foreach ((string cealHostIncludeDomain, string cealHostExcludeDomain) in cealHostDomainPairs)
                    {
                        if (cealHostIncludeDomain.StartsWith('$'))
                            continue;

                        hostRules += $"MAP {cealHostIncludeDomain.TrimStart('#')} {cealHostSniWithoutNull}," + (!string.IsNullOrWhiteSpace(cealHostExcludeDomain) ? $"EXCLUDE {cealHostExcludeDomain}," : string.Empty);
                        isValidCealHostDomainExist = true;
                    }

                    if (isValidCealHostDomainExist)
                        hostResolverRules += $"MAP {cealHostSniWithoutNull} {cealHostIp},";
                }

            CealArgs = @$"--host-rules=""{hostRules.TrimEnd(',')}"" --host-resolver-rules=""{hostResolverRules.TrimEnd(',')}"" --test-type --ignore-certificate-errors";

            NginxConfWatcher_Changed(null!, null!);
        }
    }
    private void NginxConfWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        if (MainConst.IsAdmin && MainPres!.IsNginxExist)
        {
            if (!File.Exists(MainConst.NginxConfPath))
                File.Create(MainConst.NginxConfPath).Dispose();
            if (!Directory.Exists(MainConst.NginxLogsPath))
                Directory.CreateDirectory(MainConst.NginxLogsPath);
            if (!Directory.Exists(MainConst.NginxTempPath))
                Directory.CreateDirectory(MainConst.NginxTempPath);

            using FileStream nginxConfStream = new(MainConst.NginxConfPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            ExtraNginxConfs = new StreamReader(nginxConfStream).ReadToEnd();

            NginxConfig extraNginxConfig = NginxConfig.Load(ExtraNginxConfs);
            int serverIndex = 0;

            foreach (IToken extraNginxConfigToken in extraNginxConfig.GetTokens())
                if (extraNginxConfigToken is GroupToken extraNginxConfigGroupToken && extraNginxConfigGroupToken.Key.Equals("http", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (IToken serverToken in extraNginxConfigGroupToken.Tokens)
                        if (serverToken is GroupToken serverGroupServer && extraNginxConfigGroupToken.Key.Equals("server", StringComparison.InvariantCultureIgnoreCase))
                            ++serverIndex;

                    break;
                }

            NginxConfs = extraNginxConfig
                .AddOrUpdate("worker_processes", "auto")
                .AddOrUpdate("events:worker_connections", "65536")
                .AddOrUpdate("http:proxy_set_header", "Host $http_host")
                .AddOrUpdate("http:proxy_ssl_server_name", !MainPres.IsFlashing ? "on" : "off")
                .AddOrUpdate($"http:server[{serverIndex}]:return", "https://$host$request_uri");

            foreach (List<(List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp)> cealHostRules in CealHostRulesDict.Values)
                foreach ((List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp) in cealHostRules)
                {
                    ++serverIndex;

                    string serverName = "~";

                    foreach ((string cealHostIncludeDomain, string cealHostExcludeDomain) in cealHostDomainPairs)
                    {
                        if (cealHostIncludeDomain.StartsWith('#'))
                            continue;

                        serverName += "^" + (!string.IsNullOrWhiteSpace(cealHostExcludeDomain) ? $"(?!{cealHostExcludeDomain.Replace(".", "\\.").Replace("*", ".*")})" : string.Empty) +
                            cealHostIncludeDomain.TrimStart('$').Replace(".", "\\.").Replace("*", ".*") + "$|";
                    }

                    NginxConfs = NginxConfs
                        .AddOrUpdate($"http:server[{serverIndex}]:server_name", serverName.TrimEnd('|'))
                        .AddOrUpdate($"http:server[{serverIndex}]:listen", "443 ssl")
                        .AddOrUpdate($"http:server[{serverIndex}]:ssl_certificate", Path.GetFileName(MainConst.NginxCertPath))
                        .AddOrUpdate($"http:server[{serverIndex}]:ssl_certificate_key", Path.GetFileName(MainConst.NginxKeyPath))
                        .AddOrUpdate($"http:server[{serverIndex}]:location", "/", true)
                        .AddOrUpdate($"http:server[{serverIndex}]:location:proxy_pass", $"https://{cealHostIp}");

                    NginxConfs = cealHostSni == null ?
                        NginxConfs.AddOrUpdate($"http:server[{serverIndex}]:proxy_ssl_server_name", "off") :
                        NginxConfs.AddOrUpdate($"http:server[{serverIndex}]:proxy_ssl_name", cealHostSni);
                }
        }
    }
    private void MihomoConfWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        if (MainConst.IsAdmin && MainPres!.IsMihomoExist)
        {
            try
            {
                if (!File.Exists(MainConst.MihomoConfPath))
                    File.Create(MainConst.MihomoConfPath).Dispose();

                using FileStream mihomoConfStream = new(MainConst.NginxConfPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                ExtraMihomoConfs = new StreamReader(mihomoConfStream).ReadToEnd();

                Dictionary<string, object> mihomoConfDict = new DeserializerBuilder()
                    .WithNamingConvention(HyphenatedNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build()
                    .Deserialize<Dictionary<string, object>>(ExtraMihomoConfs) ?? [];

                mihomoConfDict["mixed-port"] = 7880;
                mihomoConfDict["dns"] = new
                {
                    enable = true,
                    listen = ":53",
                    enhancedMode = "redir-host",
                    nameserver = new[] { "https://doh.apad.pro/dns-query", "https://ns.net.kg/dns-query" }
                };
                mihomoConfDict["tun"] = new
                {
                    enable = true,
                    stack = "system",
                    autoRoute = true,
                    autoDetectInterface = true,
                    dnsHijack = new[] { "any:53", "tcp://any:53" }
                };

                MihomoConfs = new SerializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build().Serialize(mihomoConfDict);
            }
            catch { MihomoConfs = string.Empty; }
        }
    }
    private void MainWin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.W)
            Application.Current.Shutdown();
    }
}