using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NginxConfigParser;
using Ona_Core;
using Sheas_Cealer_Nix.Consts;
using Sheas_Cealer_Nix.Preses;
using Sheas_Cealer_Nix.Proces;
using Sheas_Cealer_Nix.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using File = System.IO.File;

namespace Sheas_Cealer_Nix.Wins;

public partial class MainWin : Window
{
    private readonly MainPres MainPres;
    private readonly HttpClient MainClient = new(new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });
    private DispatcherTimer? HoldButtonTimer;
    private readonly DispatcherTimer ProxyTimer = new() { Interval = TimeSpan.FromSeconds(0.1) };
    private readonly FileSystemWatcher CealHostWatcher = new(Path.GetDirectoryName(MainConst.CealHostPath)!, Path.GetFileName(MainConst.CealHostPath)) { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
    private readonly FileSystemWatcher NginxConfWatcher = new(Path.GetDirectoryName(MainConst.NginxConfPath)!, Path.GetFileName(MainConst.NginxConfPath)) { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
    private readonly FileSystemWatcher MihomoConfWatcher = new(Path.GetDirectoryName(MainConst.MihomoConfPath)!, Path.GetFileName(MainConst.MihomoConfPath)) { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
    private readonly SemaphoreSlim IsNginxLaunchingSemaphore = new(1);

    private readonly SortedDictionary<string, List<(List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp)>?> CealHostRulesDict = [];
    private string CealArgs = string.Empty;
    private NginxConfig? NginxConfs;
    private string? ExtraNginxConfs;
    private string? ComihomoConfs;
    private string? HostsComihomoConfs;
    private string? MihomoConfs;
    private string? ExtraMihomoConfs;

    private int NginxHttpPort = 80;
    private int NginxHttpsPort = 443;
    private int MihomoMixedPort = 7880;

    private int GameClickTime = 0;
    private int GameFlashInterval = 1000;

    internal MainWin()
    {
        DataContext = MainPres = new();

        InitializeComponent();
    }
    //private void MainWin_SourceInitialized(object sender, EventArgs e)
    //{
    //    IconRemover.RemoveIcon(this);
    //    BorderThemeSetter.SetBorderTheme(this, MainPres.IsLightTheme);
    //}
    private async void MainWin_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(async () =>
        {
            ProxyTimer.Tick += ProxyTimer_Tick;
            CealHostWatcher.Changed += CealHostWatcher_Changed;
            NginxConfWatcher.Changed += NginxConfWatcher_Changed;
            MihomoConfWatcher.Changed += MihomoConfWatcher_Changed;

            ProxyTimer.Start();

            foreach (string cealHostPath in Directory.GetFiles(CealHostWatcher.Path, CealHostWatcher.Filter))
                CealHostWatcher_Changed(null!, new(new(), Path.GetDirectoryName(cealHostPath)!, Path.GetFileName(cealHostPath)));

            if (MainConst.IsAdmin && !MainPres.IsConginxRunning && !MainPres.IsNginxRunning)
                await NginxCleaner.Clean();

            if (Array.Exists(Environment.GetCommandLineArgs(), arg => arg.Equals("-s", StringComparison.OrdinalIgnoreCase)))
                LaunchButton_Click(null, (RoutedEventArgs)RoutedEventArgs.Empty);

            UpdateUpstreamHostButton_Click(null, null!);
        });
    }
    private async void MainWin_Closing(object sender, WindowClosingEventArgs e)
    {
        if (MainPres.IsNginxIniting)
            await File.WriteAllTextAsync(MainConst.NginxConfPath, ExtraNginxConfs);
        if (MainPres.IsMihomoIniting)
            await File.WriteAllTextAsync(MainConst.MihomoConfPath, ExtraMihomoConfs);

        Environment.Exit(0);
    }

    private void MainWin_DragEnter(object sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.GetFiles() != null ? DragDropEffects.Link : DragDropEffects.None;
        e.Handled = true;
    }
    private void MainWin_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() != null)
            MainPres.BrowserPath = e.Data.GetFiles()!.FirstOrDefault()!.Path.LocalPath;
    }

    private void SettingsBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        switch (MainPres.SettingsMode)
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
        MainPres.SettingsMode = MainPres.SettingsMode switch
        {
            MainConst.SettingsMode.BrowserPathMode => MainConst.SettingsMode.UpstreamUrlMode,
            MainConst.SettingsMode.UpstreamUrlMode => MainConst.SettingsMode.ExtraArgsMode,
            MainConst.SettingsMode.ExtraArgsMode => MainConst.SettingsMode.BrowserPathMode,
            _ => throw new UnreachableException()
        };
    }
    private async void SettingsFunctionButton_Click(object sender, RoutedEventArgs e)
    {
        switch (MainPres.SettingsMode)
        {
            case MainConst.SettingsMode.BrowserPathMode:
                IReadOnlyList<IStorageFile> browserFiles = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(new() { Title = MainConst._BrowserPathDialogFilterFileType, AllowMultiple = false });  //Todo: Filter

                if (browserFiles.Count > 0)
                    MainPres.BrowserPath = browserFiles[0].Path.LocalPath;

                return;
            case MainConst.SettingsMode.UpstreamUrlMode:
                MainPres.UpstreamUrl = MainConst.DefaultUpstreamUrl;
                return;
            case MainConst.SettingsMode.ExtraArgsMode:
                MainPres.ExtraArgs = string.Empty;
                return;
        }
    }

    private void LaunchButton_Click(object? sender, RoutedEventArgs e)
    {
        if (HoldButtonTimer is { IsEnabled: false })
            return;

        Button? senderButton = sender as Button;

        if (senderButton == NginxButton)
            NginxButtonHoldTimer_Tick(null, EventArgs.Empty);
        else if (senderButton == MihomoButton)
            MihomoButtonHoldTimer_Tick(null, EventArgs.Empty);
        else
            BrowserButtonHoldTimer_Tick(sender == null, EventArgs.Empty);
    }
    private void LaunchButton_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        Button senderButton = (Button)sender;

        HoldButtonTimer = new() { Interval = TimeSpan.FromSeconds(1) };
        HoldButtonTimer.Tick += senderButton == NginxButton ? NginxButtonHoldTimer_Tick : senderButton == MihomoButton ? MihomoButtonHoldTimer_Tick : BrowserButtonHoldTimer_Tick;
        HoldButtonTimer.Start();
    }
    private async void BrowserButtonHoldTimer_Tick(object? sender, EventArgs e)
    {
        HoldButtonTimer?.Stop();

        if ((CealHostRulesDict.ContainsValue(null!) && await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._CealHostErrorPrompt, ButtonEnum.YesNo).ShowWindowDialogAsync(this) != ButtonResult.Yes) ||
            (sender is not true && await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._KillBrowserProcessPrompt, ButtonEnum.YesNo).ShowWindowDialogAsync(this) != ButtonResult.Yes))
            return;

        foreach (Process browserProcess in Process.GetProcessesByName(OperatingSystem.IsWindows() ? Path.GetFileNameWithoutExtension(MainPres.BrowserPath) : Path.GetFileName(MainPres.BrowserPath)))
        {
            browserProcess.Kill();
            await browserProcess.WaitForExitAsync();
        }

        await Task.Run(() =>
        {
            new BrowserProc(MainPres.BrowserPath, sender is bool).Run(Path.GetDirectoryName(MainPres.BrowserPath)!, $"{CealArgs} {MainPres.ExtraArgs.Trim()}");
        });
    }
    private async void NginxButtonHoldTimer_Tick(object? sender, EventArgs e)
    {
        HoldButtonTimer?.Stop();

        if (!MainPres.IsConginxRunning && !MainPres.IsNginxRunning)
        {
            if (NginxCleaner.IsNginxCleaningSemaphore.CurrentCount == 0 || !await IsNginxLaunchingSemaphore.WaitAsync(0))
                return;

            try
            {
                if ((!MainPres.IsConginxExist && !MainPres.IsNginxExist) ||
                    (CealHostRulesDict.ContainsValue(null!) && await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._CealHostErrorPrompt, ButtonEnum.YesNo).ShowWindowDialogAsync(this) != ButtonResult.Yes) ||
                    (NginxHttpsPort != 443 && await MessageBoxManager.GetMessageBoxStandard(string.Empty, string.Format(MainConst._NginxHttpsPortOccupiedPrompt, NginxHttpsPort), ButtonEnum.YesNo).ShowWindowDialogAsync(this) != ButtonResult.Yes) ||
                    (NginxHttpPort != 80 && await MessageBoxManager.GetMessageBoxStandard(string.Empty, string.Format(MainConst._NginxHttpPortOccupiedPrompt, NginxHttpPort), ButtonEnum.YesNo).ShowWindowDialogAsync(this) != ButtonResult.Yes) ||
                    (sender != null && await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._LaunchHostsNginxPrompt, ButtonEnum.YesNo).ShowWindowDialogAsync(this) != ButtonResult.Yes) ||
                    (await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._LaunchProxyPrompt, ButtonEnum.YesNo).ShowWindowDialogAsync(this) != ButtonResult.Yes) ||
                    (MainPres.IsFlashing && await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._LaunchNginxFlashingPrompt, ButtonEnum.YesNo).ShowWindowDialogAsync(this) != ButtonResult.Yes))
                    return;

                if (!File.Exists(MainConst.NginxConfPath))
                    await File.Create(MainConst.NginxConfPath).DisposeAsync();
                if (!Directory.Exists(MainConst.NginxLogsPath))
                    Directory.CreateDirectory(MainConst.NginxLogsPath);
                if (!Directory.Exists(MainConst.NginxTempPath))
                    Directory.CreateDirectory(MainConst.NginxTempPath);

                if (sender == null)
                    File.Move(MainConst.NginxPath, MainConst.ConginxPath);

                RSA certKey = RSA.Create(2048);

                #region Root Cert
                CertificateRequest rootCertRequest = new(MainConst.NginxRootCertSubjectName, certKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                rootCertRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, false));

                using X509Certificate2 rootCert = rootCertRequest.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(100));
                using X509Store certStore = new(StoreName.Root, StoreLocation.CurrentUser, OpenFlags.ReadWrite);

                certStore.Add(rootCert);
                certStore.Close();
                #endregion Root Cert

                #region Child Cert & Hosts
                CertificateRequest childCertRequest = new(MainConst.NginxChildCertSubjectName, certKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                SubjectAlternativeNameBuilder childCertSanBuilder = new();
                string hostsConfAppendContent = MainConst.HostsConfStartMarker;

                foreach (List<(List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp)>? cealHostRules in CealHostRulesDict.Values)
                    foreach ((List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, _, _) in cealHostRules ?? [])
                        foreach ((string cealHostIncludeDomain, _) in cealHostDomainPairs)
                        {
                            string cealHostIncludeDomainWithoutWildcard = cealHostIncludeDomain.TrimStart('$').TrimStart('*').TrimStart('.');

                            if (cealHostIncludeDomain.StartsWith('#') || cealHostIncludeDomainWithoutWildcard.Contains('*') || string.IsNullOrWhiteSpace(cealHostIncludeDomainWithoutWildcard))
                                continue;

                            if (cealHostIncludeDomain.TrimStart('$').StartsWith('*'))
                            {
                                childCertSanBuilder.AddDnsName($"*.{cealHostIncludeDomainWithoutWildcard}");

                                if (sender != null)
                                    hostsConfAppendContent += $"127.0.0.1 www.{cealHostIncludeDomainWithoutWildcard}{Environment.NewLine}";

                                if (cealHostIncludeDomain.TrimStart('$').StartsWith("*."))
                                    continue;
                            }

                            childCertSanBuilder.AddDnsName(cealHostIncludeDomainWithoutWildcard);

                            if (sender != null)
                                hostsConfAppendContent += $"127.0.0.1 {cealHostIncludeDomainWithoutWildcard}{Environment.NewLine}";
                        }

                childCertRequest.CertificateExtensions.Add(childCertSanBuilder.Build());

                using X509Certificate2 childCert = childCertRequest.Create(rootCert, rootCert.NotBefore, rootCert.NotAfter, Guid.NewGuid().ToByteArray());

                await File.WriteAllTextAsync(MainConst.NginxCertPath, childCert.ExportCertificatePem());
                await File.WriteAllTextAsync(MainConst.NginxKeyPath, certKey.ExportPkcs8PrivateKeyPem());

                if (sender != null)
                {
                    hostsConfAppendContent += MainConst.HostsConfEndMarker;

                    File.SetAttributes(MainConst.HostsConfPath, File.GetAttributes(MainConst.HostsConfPath) & ~FileAttributes.ReadOnly);
                    await File.AppendAllTextAsync(MainConst.HostsConfPath, hostsConfAppendContent);
                }
                #endregion Child Cert & Hosts

                try
                {
                    if (sender == null)
                        MainPres.IsCoproxyIniting = true;
                    else
                        MainPres.IsNginxIniting = true;

                    NginxConfWatcher.EnableRaisingEvents = false;
                    NginxConfs!.Save(MainConst.NginxConfPath);

                    await Task.Run(() =>
                    {
                        if (sender == null)
                            new ConginxProc().Run(Path.GetDirectoryName(MainConst.ConginxPath)!, @$"-c ""{Path.GetRelativePath(Path.GetDirectoryName(MainConst.ConginxPath)!, MainConst.NginxConfPath)}""");
                        else
                            new NginxProc().Run(Path.GetDirectoryName(MainConst.NginxPath)!, @$"-c ""{Path.GetRelativePath(Path.GetDirectoryName(MainConst.NginxPath)!, MainConst.NginxConfPath)}""");
                    });

                    while (true)
                    {
                        try
                        {
                            await Http.GetAsync<HttpResponseMessage>($"https://localhost:{NginxHttpsPort}", MainClient);

                            break;
                        }
                        catch (HttpRequestException ex)
                        {
                            if (ex.InnerException is SocketException innerEx && innerEx.SocketErrorCode != SocketError.ConnectionRefused)
                                break;
                        }

                        if (sender == null ? MainPres.IsConginxRunning : MainPres.IsNginxRunning)
                            continue;

                        if (await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._LaunchNginxErrorPrompt, ButtonEnum.YesNo).ShowWindowDialogAsync(this) == ButtonResult.Yes)
                            Process.Start(new ProcessStartInfo(MainConst.NginxErrorLogsPath) { UseShellExecute = true });

                        break;
                    }

                    if (sender == null)
                        MihomoButtonHoldTimer_Tick(null, EventArgs.Empty);

                    if (OperatingSystem.IsWindows())
                        DnsFlusher.FlushDns();
                }
                finally
                {
                    await File.WriteAllTextAsync(MainConst.NginxConfPath, ExtraNginxConfs);
                    NginxConfWatcher.EnableRaisingEvents = true;

                    if (sender != null)
                        MainPres.IsNginxIniting = false;
                }
            }
            finally { IsNginxLaunchingSemaphore.Release(); }
        }
        else
        {
            bool isConginxRunning = MainPres.IsConginxRunning;

            if (isConginxRunning)
                MainPres.IsCoproxyStopping = true;

            foreach (Process nginxProcess in Process.GetProcessesByName(OperatingSystem.IsWindows() ? Path.GetFileNameWithoutExtension(isConginxRunning ? MainConst.ConginxPath : MainConst.NginxPath) : Path.GetFileName(isConginxRunning ? MainConst.ConginxPath : MainConst.NginxPath)))
            {
                nginxProcess.Exited += async (_, _) =>
                {
                    await NginxCleaner.Clean();

                    NginxConfWatcher_Changed(null!, null!);
                };

                nginxProcess.Kill();
            }

            if (isConginxRunning)
            {
                MainPres.IsConginxRunning = false;

                File.Move(MainConst.ConginxPath, MainConst.NginxPath);

                MihomoButtonHoldTimer_Tick(null, EventArgs.Empty);
            }
            else
                MainPres.IsNginxRunning = false;
        }
    }
    private async void MihomoButtonHoldTimer_Tick(object? sender, EventArgs e)
    {
        HoldButtonTimer?.Stop();

        if (!MainPres.IsComihomoRunning && !MainPres.IsMihomoRunning)
        {
            if (!MainPres.IsComihomoExist && !MainPres.IsMihomoExist)
                return;
            if (string.IsNullOrWhiteSpace(MihomoConfs))
                throw new(MainConst._MihomoConfErrorMsg);
            if (!MainPres.IsConginxRunning && !MainPres.IsCoproxyStopping && await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._LaunchProxyPrompt, ButtonEnum.YesNo).ShowWindowDialogAsync(this) != ButtonResult.Yes)
                return;

            if (!File.Exists(MainConst.MihomoConfPath))
                await File.Create(MainConst.MihomoConfPath).DisposeAsync();

            if (MainPres.IsConginxRunning)
                if (MainPres.IsMihomoExist)
                {
                    File.Move(MainConst.MihomoPath, MainConst.ComihomoPath);
                    MainPres.IsComihomoExist = true;
                    MainPres.IsMihomoExist = false;
                }
                else if (MainPres.IsComihomoExist)
                {
                    File.Move(MainConst.ComihomoPath, MainConst.MihomoPath);
                    MainPres.IsComihomoExist = false;
                    MainPres.IsMihomoExist = true;
                }

            try
            {
                if (MainPres.IsComihomoExist)
                    MainPres.IsComihomoIniting = true;
                else
                    MainPres.IsMihomoIniting = true;

                MihomoConfWatcher.EnableRaisingEvents = false;
                await File.WriteAllTextAsync(MainConst.MihomoConfPath, !MainPres.IsConginxRunning ? MihomoConfs :
                    MainPres.IsComihomoExist ? HostsComihomoConfs : ComihomoConfs);

                await Task.Run(() =>
                {
                    if (MainPres.IsComihomoExist)
                    {
                        new ComihomoProc().Run(Path.GetDirectoryName(MainConst.ComihomoPath)!, @$"-d ""{Path.GetDirectoryName(MainConst.MihomoConfPath)}""");
                        MainPres.IsComihomoRunning = true;
                    }
                    else
                    {
                        new MihomoProc().Run(Path.GetDirectoryName(MainConst.MihomoPath)!, @$"-d ""{Path.GetDirectoryName(MainConst.MihomoConfPath)}""");
                        MainPres.IsMihomoRunning = true;
                    }
                });

                while (true)
                {
                    try
                    {
                        await Http.GetAsync<HttpResponseMessage>($"http://localhost:{MihomoMixedPort}", MainClient);

                        break;
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.InnerException is SocketException innerEx && innerEx.SocketErrorCode != SocketError.ConnectionRefused)
                            break;
                    }

                    if (MainPres.IsComihomoExist ? MainPres.IsComihomoRunning : MainPres.IsMihomoRunning)
                        continue;

                    await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._LaunchMihomoErrorMsg).ShowWindowDialogAsync(this);

                    break;
                }
            }
            finally
            {
                await File.WriteAllTextAsync(MainConst.MihomoConfPath, ExtraMihomoConfs);
                MihomoConfWatcher.EnableRaisingEvents = true;

                if (MainPres.IsCoproxyIniting)
                    MainPres.IsCoproxyIniting = false;
                else if (MainPres.IsCoproxyStopping)
                    MainPres.IsCoproxyStopping = false;

                if (MainPres.IsComihomoExist)
                    MainPres.IsComihomoIniting = false;
                else
                    MainPres.IsMihomoIniting = false;
            }
        }
        else
        {
            bool isComihomoRunning = MainPres.IsComihomoRunning;

            foreach (Process mihomoProcess in Process.GetProcessesByName(OperatingSystem.IsWindows() ? Path.GetFileNameWithoutExtension(isComihomoRunning ? MainConst.ComihomoPath : MainConst.MihomoPath) : Path.GetFileName(isComihomoRunning ? MainConst.ComihomoPath : MainConst.MihomoPath)))
            {
                mihomoProcess.Exited += (_, _) => MihomoConfWatcher_Changed(null!, null!);

                mihomoProcess.Kill();
            }

            if (isComihomoRunning)
                MainPres.IsComihomoRunning = false;
            else
                MainPres.IsMihomoRunning = false;

            if (MainPres.IsConginxRunning)
            {
                if (MainPres.IsCoproxyIniting)
                {
                    File.Move(MainConst.MihomoPath, MainConst.ComihomoPath);
                    MainPres.IsComihomoExist = true;
                    MainPres.IsMihomoExist = false;
                }

                MihomoButtonHoldTimer_Tick(null, EventArgs.Empty);

                return;
            }

            if (MainPres.IsComihomoExist)
            {
                File.Move(MainConst.ComihomoPath, MainConst.MihomoPath);
                MainPres.IsComihomoExist = false;
                MainPres.IsMihomoExist = true;

                MainPres.IsCoproxyStopping = false;
            }
            else if (MainPres.IsCoproxyStopping)
                MihomoButtonHoldTimer_Tick(null, EventArgs.Empty);
        }
    }

    private async void EditHostButton_Click(object sender, RoutedEventArgs e)
    {
        Button senderButton = (Button)sender;
        string cealHostPath = senderButton == EditLocalHostButton ? MainConst.LocalHostPath : MainConst.UpstreamHostPath;

        if (!File.Exists(cealHostPath))
            await File.Create(cealHostPath).DisposeAsync();

        if (OperatingSystem.IsWindows())
            try { Process.Start(new ProcessStartInfo(cealHostPath) { UseShellExecute = true }); }
            catch (UnauthorizedAccessException) { Process.Start(new ProcessStartInfo(cealHostPath) { UseShellExecute = true, Verb = "RunAs" }); }
        else
            try { Process.Start("vi", cealHostPath); }
            catch (UnauthorizedAccessException) { Process.Start("sudo", $"vi {cealHostPath}"); }
    }
    private async void EditConfButton_Click(object sender, RoutedEventArgs e)
    {
        Button senderButton = (Button)sender;
        string confPath;

        if (senderButton == EditHostsConfButton)
        {
            confPath = MainConst.HostsConfPath;

            File.SetAttributes(MainConst.HostsConfPath, File.GetAttributes(MainConst.HostsConfPath) & ~FileAttributes.ReadOnly);
        }
        else
        {
            confPath = senderButton == EditNginxConfButton ? MainConst.NginxConfPath : MainConst.MihomoConfPath;

            if (!File.Exists(confPath))
                await File.Create(confPath).DisposeAsync();
        }

        if (OperatingSystem.IsWindows())
            Process.Start(new ProcessStartInfo(confPath) { UseShellExecute = true });
        else
            Process.Start("vi", confPath);
    }
    private async void UpdateUpstreamHostButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (!File.Exists(MainConst.UpstreamHostPath))
                await File.Create(MainConst.UpstreamHostPath).DisposeAsync();

            string upstreamUpstreamHostUrl = (MainPres.UpstreamUrl.StartsWith("http://") || MainPres.UpstreamUrl.StartsWith("https://") ? string.Empty : "https://") + MainPres.UpstreamUrl;
            string upstreamUpstreamHostString = await Http.GetAsync<string>(upstreamUpstreamHostUrl, MainClient);
            string localUpstreamHostString = await File.ReadAllTextAsync(MainConst.UpstreamHostPath);

            try { upstreamUpstreamHostString = Encoding.UTF8.GetString(Convert.FromBase64String(upstreamUpstreamHostString)); }
            catch { }

            if (sender == null && (localUpstreamHostString != upstreamUpstreamHostString && localUpstreamHostString.ReplaceLineEndings() != upstreamUpstreamHostString.ReplaceLineEndings()))
                MainPres.IsUpstreamHostUtd = false;
            else if (sender != null)
                if (localUpstreamHostString == upstreamUpstreamHostString || localUpstreamHostString.ReplaceLineEndings() == upstreamUpstreamHostString.ReplaceLineEndings())
                {
                    MainPres.IsUpstreamHostUtd = true;

                    await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._UpstreamHostUtdMsg).ShowWindowDialogAsync(this);
                }
                else
                {
                    ButtonResult overrideOptionResult = await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._OverrideUpstreamHostPrompt, ButtonEnum.YesNoCancel).ShowWindowDialogAsync(this);

                    if (overrideOptionResult == ButtonResult.Yes)
                    {
                        await File.WriteAllTextAsync(MainConst.UpstreamHostPath, upstreamUpstreamHostString);

                        MainPres.IsUpstreamHostUtd = true;

                        await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._UpdateUpstreamHostSuccessMsg).ShowWindowDialogAsync(this);
                    }
                    else if (overrideOptionResult == ButtonResult.No)
                        try { Process.Start(new ProcessStartInfo(upstreamUpstreamHostUrl) { UseShellExecute = true }); }
                        catch (UnauthorizedAccessException) { Process.Start(new ProcessStartInfo(upstreamUpstreamHostUrl) { UseShellExecute = true, Verb = "RunAs" }); }
                }
        }
        catch when (sender == null) { }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e) => new SettingsWin().ShowDialog(this);
    private async void NoClickButton_Click(object sender, RoutedEventArgs e)
    {
        if (GameFlashInterval <= 10)
        {
            await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._GameReviewEndingMsg).ShowWindowDialogAsync(this);

            return;
        }

        switch (++GameClickTime)
        {
            case 1:
                await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._GameClickOnceMsg).ShowWindowDialogAsync(this);
                return;
            case 2:
                await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._GameClickTwiceMsg).ShowWindowDialogAsync(this);
                return;
            case 3:
                await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._GameClickThreeMsg).ShowWindowDialogAsync(this);
                return;
        }

        if (!MainPres.IsFlashing)
        {
            await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._GameStartMsg).ShowWindowDialogAsync(this);
            MainPres.IsFlashing = true;
            NginxConfWatcher_Changed(null!, null!);

            Random random = new();

            while (GameFlashInterval > 10)
            {
                Position = new(random.Next(0, (int)(Screens.Primary!.Bounds.Width - Bounds.Width)), random.Next(0, (int)(Screens.Primary.Bounds.Height - Bounds.Height)));

                //PaletteHelper paletteHelper = new();
                //Theme newTheme = paletteHelper.GetTheme();
                //Color newPrimaryColor = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
                //bool isLightTheme = random.Next(2) == 0;

                //newTheme.SetPrimaryColor(newPrimaryColor);
                //newTheme.SetBaseTheme(isLightTheme ? BaseTheme.Light : BaseTheme.Dark);
                //paletteHelper.SetTheme(newTheme);

                //foreach (Window currentWindow in Application.Current.Windows)
                //    BorderThemeSetter.SetBorderTheme(currentWindow, isLightTheme);

                //Style newButtonStyle = new(typeof(Button), Application.Current.Resources[typeof(Button)] as Style);
                //(Color? newForegroundColor, Color newAccentForegroundColor) = ForegroundGenerator.GetForeground(newPrimaryColor.R, newPrimaryColor.G, newPrimaryColor.B);

                //newButtonStyle.Setters.Add(new Setter(ForegroundProperty, newForegroundColor.HasValue ? new SolidColorBrush(newForegroundColor.Value) : new DynamicResourceExtension("MaterialDesignBackground")));
                //Application.Current.Resources[typeof(Button)] = newButtonStyle;

                //MainPres.AccentForegroundColor = newAccentForegroundColor;

                if (GameFlashInterval > 100)
                    GameFlashInterval += random.Next(1, 4);

                await Task.Delay(GameFlashInterval);
            }

            MainPres.IsFlashing = false;
            NginxConfWatcher_Changed(null!, null!);
            await MessageBoxManager.GetMessageBoxStandard(string.Empty, MainConst._GameEndingMsg).ShowWindowDialogAsync(this);
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
                await MessageBoxManager.GetMessageBoxStandard(string.Empty, $"{MainConst._GameGradeMsg} {GameFlashInterval}").ShowWindowDialogAsync(this);
        }
    }
    private void AboutButton_Click(object sender, RoutedEventArgs e) => new AboutWin().ShowDialog(this);

    private void ProxyTimer_Tick(object? sender, EventArgs e)
    {
        MainPres.IsConginxExist = File.Exists(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, Path.GetFileName(MainConst.ConginxPath)));
        MainPres.IsNginxExist = File.Exists(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, Path.GetFileName(MainConst.NginxPath)));
        MainPres.IsConginxRunning = Process.GetProcessesByName(OperatingSystem.IsWindows() ? Path.GetFileNameWithoutExtension(MainConst.ConginxPath) : Path.GetFileName(MainConst.ConginxPath)).Length != 0;
        MainPres.IsNginxRunning = Process.GetProcessesByName(OperatingSystem.IsWindows() ? Path.GetFileNameWithoutExtension(MainConst.NginxPath) : Path.GetFileName(MainConst.NginxPath)).Length != 0;
        MainPres.IsComihomoExist = File.Exists(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, Path.GetFileName(MainConst.ComihomoPath)));
        MainPres.IsMihomoExist = File.Exists(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, Path.GetFileName(MainConst.MihomoPath)));
        MainPres.IsComihomoRunning = Process.GetProcessesByName(OperatingSystem.IsWindows() ? Path.GetFileNameWithoutExtension(MainConst.ComihomoPath) : Path.GetFileName(MainConst.ComihomoPath)).Length != 0;
        MainPres.IsMihomoRunning = Process.GetProcessesByName(OperatingSystem.IsWindows() ? Path.GetFileNameWithoutExtension(MainConst.MihomoPath) : Path.GetFileName(MainConst.MihomoPath)).Length != 0;
    }
    private async void CealHostWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        string cealHostName = e.Name!.TrimStart("Cealing-Host-".ToCharArray()).TrimEnd(".json".ToCharArray());

        try
        {
            CealHostRulesDict[cealHostName] = [];

            await using FileStream cealHostStream = new(e.FullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

            if (cealHostStream.Length == 0)
                return;

            JsonDocumentOptions cealHostOptions = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
            JsonElement cealHostArray = JsonDocument.Parse(cealHostStream, cealHostOptions).RootElement;

            foreach (JsonElement cealHostRule in cealHostArray.EnumerateArray())
            {
                List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs = [];
                string? cealHostSni = cealHostRule[1].ValueKind == JsonValueKind.Null ? null :
                    string.IsNullOrWhiteSpace(cealHostRule[1].ToString()) ? $"{cealHostName}{CealHostRulesDict[cealHostName]!.Count}" : cealHostRule[1].ToString().Trim();
                string cealHostIp = string.IsNullOrWhiteSpace(cealHostRule[2].ToString()) ? "127.0.0.1" : cealHostRule[2].ToString().Trim();

                foreach (JsonElement cealHostDomain in cealHostRule[0].EnumerateArray())
                {
                    string[] cealHostDomainPair = cealHostDomain.ToString().Split('^', 2, StringSplitOptions.TrimEntries);

                    if (string.IsNullOrEmpty(cealHostDomainPair[0].TrimStart('#').TrimStart('$')))
                        continue;

                    cealHostDomainPairs.Add((cealHostDomainPair[0], cealHostDomainPair.Length == 2 ? cealHostDomainPair[1] : string.Empty));
                }

                if (cealHostDomainPairs.Count != 0)
                    CealHostRulesDict[cealHostName]!.Add((cealHostDomainPairs, cealHostSni, cealHostIp));
            }
        }
        catch { CealHostRulesDict[cealHostName] = null; }
        finally
        {
            string hostRules = string.Empty;
            string hostResolverRules = string.Empty;
            int nullSniNum = 0;

            foreach (KeyValuePair<string, List<(List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp)>?> cealHostRulesPair in CealHostRulesDict)
                foreach ((List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp) in cealHostRulesPair.Value ?? [])
                {
                    string cealHostSniWithoutNull = cealHostSni ?? $"{cealHostRulesPair.Key}{(cealHostRulesPair.Value ?? []).Count + ++nullSniNum}";
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
            MihomoConfWatcher_Changed(null!, null!);
        }
    }
    private async void NginxConfWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        if (!MainConst.IsAdmin || (!MainPres.IsNginxExist && !MainPres.IsConginxExist))
            return;

        if (!File.Exists(MainConst.NginxConfPath))
            await File.Create(MainConst.NginxConfPath).DisposeAsync();
        if (!Directory.Exists(MainConst.NginxLogsPath))
            Directory.CreateDirectory(MainConst.NginxLogsPath);
        if (!Directory.Exists(MainConst.NginxTempPath))
            Directory.CreateDirectory(MainConst.NginxTempPath);

        NginxHttpPort = 80;
        NginxHttpsPort = 443;

        foreach (IPEndPoint activeTcpListener in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
            if (activeTcpListener.Port == NginxHttpPort)
                NginxHttpPort++;
            else if (activeTcpListener.Port == NginxHttpsPort)
                NginxHttpsPort++;
            else if (activeTcpListener.Port > NginxHttpsPort)
                break;

        await using FileStream nginxConfStream = new(MainConst.NginxConfPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        NginxConfig extraNginxConfig = NginxConfig.Load(ExtraNginxConfs = new StreamReader(nginxConfStream).ReadToEnd());
        int serverIndex = 0;

        foreach (IToken extraNginxConfigToken in extraNginxConfig.GetTokens())
            if (extraNginxConfigToken is GroupToken extraNginxConfigGroupToken && extraNginxConfigGroupToken.Key.Equals("http", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (IToken serverToken in extraNginxConfigGroupToken.Tokens)
                    if (serverToken is GroupToken serverGroupToken && serverGroupToken.Key.Equals("server", StringComparison.InvariantCultureIgnoreCase))
                        ++serverIndex;

                break;
            }

        NginxConfs = extraNginxConfig
            .AddOrUpdate("worker_processes", "auto")
            .AddOrUpdate("events:worker_connections", "65536")
            .AddOrUpdate("http:proxy_set_header", "Host $http_host")
            .AddOrUpdate("http:proxy_ssl_server_name", !MainPres.IsFlashing ? "on" : "off")
            .AddOrUpdate("http:proxy_buffer_size", "14k")
            .AddOrUpdate($"http:server[{serverIndex}]:listen", $"{NginxHttpPort} default_server")
            .AddOrUpdate($"http:server[{serverIndex}]:return", "https://$host$request_uri");

        foreach (List<(List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp)>? cealHostRules in CealHostRulesDict.Values)
            foreach ((List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp) in cealHostRules ?? [])
            {
                string serverName = "~";

                foreach ((string cealHostIncludeDomain, string cealHostExcludeDomain) in cealHostDomainPairs)
                {
                    if (cealHostIncludeDomain.StartsWith('#'))
                        continue;

                    serverName += "^" + (!string.IsNullOrWhiteSpace(cealHostExcludeDomain) ? $"(?!{cealHostExcludeDomain.Replace(".", "\\.").Replace("*", ".*")})" : string.Empty) +
                                  cealHostIncludeDomain.TrimStart('$').Replace(".", "\\.").Replace("*", ".*") + "$|";
                }

                if (serverName == "~")
                    continue;

                ++serverIndex;

                NginxConfs = NginxConfs
                    .AddOrUpdate($"http:server[{serverIndex}]:server_name", serverName.TrimEnd('|'))
                    .AddOrUpdate($"http:server[{serverIndex}]:listen", $"{NginxHttpsPort} ssl")
                    .AddOrUpdate($"http:server[{serverIndex}]:ssl_certificate", Path.GetFileName(MainConst.NginxCertPath))
                    .AddOrUpdate($"http:server[{serverIndex}]:ssl_certificate_key", Path.GetFileName(MainConst.NginxKeyPath))
                    .AddOrUpdate($"http:server[{serverIndex}]:location", "/", true)
                    .AddOrUpdate($"http:server[{serverIndex}]:location:proxy_pass", $"https://{cealHostIp}");

                NginxConfs = cealHostSni == null ?
                    NginxConfs.AddOrUpdate($"http:server[{serverIndex}]:proxy_ssl_server_name", "off") :
                    NginxConfs.AddOrUpdate($"http:server[{serverIndex}]:proxy_ssl_name", cealHostSni);
            }
    }
    private async void MihomoConfWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        if (!MainConst.IsAdmin || (!MainPres.IsMihomoExist && !MainPres.IsComihomoExist))
            return;

        try
        {
            if (!File.Exists(MainConst.MihomoConfPath))
                await File.Create(MainConst.MihomoConfPath).DisposeAsync();

            MihomoMixedPort = 7880;

            foreach (IPEndPoint activeTcpListener in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
                if (activeTcpListener.Port == MihomoMixedPort)
                    MihomoMixedPort++;
                else if (activeTcpListener.Port > MihomoMixedPort)
                    break;

            await using FileStream mihomoConfStream = new(MainConst.MihomoConfPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            Dictionary<string, object> mihomoConfDict = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build()
                .Deserialize<Dictionary<string, object>>(ExtraMihomoConfs = await new StreamReader(mihomoConfStream).ReadToEndAsync()) ?? [];
            Dictionary<string, object> hostsMihomoConfDict = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build()
                .Deserialize<Dictionary<string, object>>(ExtraMihomoConfs = await new StreamReader(mihomoConfStream).ReadToEndAsync()) ?? [];

            mihomoConfDict["mixed-port"] = hostsMihomoConfDict["mixed-port"] = MihomoMixedPort;
            mihomoConfDict["tun"] = hostsMihomoConfDict["tun"] = new
            {
                enable = true,
                stack = "system",
                autoRoute = true,
                autoDetectInterface = true,
                dnsHijack = new[] { "any:53", "tcp://any:53" }
            };
            mihomoConfDict["dns"] = new
            {
                enable = true,
                listen = ":53",
                ipv6 = true,
                nameserver = MainConst.MihomoNameServers
            };
            hostsMihomoConfDict["dns"] = new
            {
                enable = true,
                listen = ":53",
                ipv6 = true
            };

            MihomoConfs = new SerializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build().Serialize(mihomoConfDict);

            Dictionary<string, string> mihomoHostsDict = [];

            foreach (List<(List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, string? cealHostSni, string cealHostIp)>? cealHostRules in CealHostRulesDict.Values)
                foreach ((List<(string cealHostIncludeDomain, string cealHostExcludeDomain)> cealHostDomainPairs, _, _) in cealHostRules ?? [])
                    foreach ((string cealHostIncludeDomain, _) in cealHostDomainPairs)
                    {
                        string cealHostIncludeDomainWithoutWildcard = cealHostIncludeDomain.TrimStart('$').TrimStart('*').TrimStart('.');

                        if (cealHostIncludeDomain.StartsWith('#') || cealHostIncludeDomainWithoutWildcard.Contains('*') || string.IsNullOrWhiteSpace(cealHostIncludeDomainWithoutWildcard))
                            continue;

                        if (cealHostIncludeDomain.TrimStart('$').StartsWith('*'))
                        {
                            mihomoHostsDict.TryAdd($"*.{cealHostIncludeDomainWithoutWildcard}", "127.0.0.1");

                            if (cealHostIncludeDomain.TrimStart('$').StartsWith("*."))
                                continue;
                        }

                        mihomoHostsDict.TryAdd(cealHostIncludeDomainWithoutWildcard, "127.0.0.1");
                    }

            mihomoConfDict["hosts"] = hostsMihomoConfDict["hosts"] = mihomoHostsDict;

            ComihomoConfs = new SerializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build().Serialize(mihomoConfDict);
            HostsComihomoConfs = new SerializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build().Serialize(hostsMihomoConfDict);
        }
        catch { ComihomoConfs = HostsComihomoConfs = MihomoConfs = string.Empty; }
    }
    private void MainWin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.Control)
            return;

        if (e.Key == Key.W)
            Environment.Exit(0);
    }
}