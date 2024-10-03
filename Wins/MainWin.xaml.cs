using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
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
using OnaCore;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Utils;
using YamlDotNet.RepresentationModel;
using File = System.IO.File;

namespace Sheas_Cealer.Wins;

public partial class MainWin : Window
{
    private static MainPres? MainPres;
    private static readonly HttpClient MainClient = new();
    private static DispatcherTimer? HoldButtonTimer;
    private static readonly DispatcherTimer ProxyTimer = new() { Interval = TimeSpan.FromSeconds(0.1) };
    private static readonly FileSystemWatcher HostWatcher = new(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Host-*.json") { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
    private static readonly Dictionary<string, (string hostRulesFragments, string hostResolverRulesFragments)> CealArgsFragments = [];
    private static string CealArgs = string.Empty;
    private static int GameClickTime = 0;
    private static int GameFlashInterval = 1000;

    internal MainWin(string[] args)
    {
        InitializeComponent();

        DataContext = MainPres = new(args);

        ProxyTimer.Tick += ProxyTimer_Tick;
        ProxyTimer.Start();

        HostWatcher.Changed += HostWatcher_Changed;
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
            throw new Exception(MainConst._HostErrorHint);
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
        string configPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "nginx.conf");
        string logsPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "logs");
        string tempPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "temp");

        if (!MainPres!.IsNginxRunning)
        {
            if (!File.Exists(configPath))
                File.Create(configPath).Dispose();
            if (!Directory.Exists(logsPath))
                Directory.CreateDirectory(logsPath);
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            new NginxProc().ShellRun(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, @"-c nginx.conf");
        }
        else
        {
            foreach (Process mihomoProcess in Process.GetProcessesByName("Cealing-Nginx"))
            {
                mihomoProcess.Kill();
                mihomoProcess.WaitForExit();
            }
        }
    }
    private void MihomoButton_Click(object sender, RoutedEventArgs e)
    {
        RegistryKey proxyKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true)!;
        string configPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "config.yaml");

        if (!MainPres!.IsMihomoRunning)
        {
            YamlStream configStream = [];
            YamlMappingNode configMapNode;
            YamlNode mihomoPortNode;

            if (!File.Exists(configPath))
                File.Create(configPath).Dispose();

            configStream.Load(File.OpenText(configPath));

            try { configMapNode = (YamlMappingNode)configStream.Documents[0].RootNode; }
            catch { throw new Exception(MainConst._ConfErrorHint); }

            if (!configMapNode.Children.TryGetValue("mixed-port", out mihomoPortNode!) && !configMapNode.Children.TryGetValue("port", out mihomoPortNode!))
                mihomoPortNode = "7890";

            proxyKey.SetValue("ProxyEnable", 1);
            proxyKey.SetValue("ProxyServer", "127.0.0.1:" + mihomoPortNode);

            new MihomoProc().ShellRun(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "-d .");
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
            MessageBox.Show(MainConst._UpstreamHostUtdHint);
        else
        {
            MessageBoxResult overrideOptionResult = MessageBox.Show(MainConst._OverrideUpstreamHostPrompt, string.Empty, MessageBoxButton.YesNoCancel);
            if (overrideOptionResult == MessageBoxResult.Yes)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Host-Upstream.json"), newUpstreamHostString);
                MessageBox.Show(MainConst._UpdateUpstreamHostSuccessHint);
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

        string confPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, senderButton == EditNginxConfButton ? "nginx.conf" : "config.json");

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
            string hostRulesFragments = string.Empty;
            string hostResolverRulesFragments = string.Empty;
            int ruleIndex = 0;

            using FileStream hostStream = new(e.FullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            JsonDocumentOptions hostOptions = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
            JsonElement hostArray = JsonDocument.Parse(hostStream, hostOptions).RootElement;

            foreach (JsonElement hostItem in hostArray.EnumerateArray())
            {
                string hostSni = string.IsNullOrWhiteSpace(hostItem[1].ToString()) ? $"{hostName}{ruleIndex}" : hostItem[1].ToString();
                string hostIp = string.IsNullOrWhiteSpace(hostItem[2].ToString()) ? "127.0.0.1" : hostItem[2].ToString();

                foreach (JsonElement hostDomain in hostItem[0].EnumerateArray())
                {
                    if (hostDomain.ToString().StartsWith('^') || hostDomain.ToString().EndsWith('^'))
                        continue;

                    string[] hostDomainPair = hostDomain.ToString().Split('^', 2);

                    hostRulesFragments += $"MAP {hostDomainPair[0]} {hostSni}," + (hostDomainPair.Length == 2 ? $"EXCLUDE {hostDomainPair[1]}," : string.Empty);
                }

                hostResolverRulesFragments += $"MAP {hostSni} {hostIp},";

                ++ruleIndex;
            }

            CealArgsFragments[hostName] = (hostRulesFragments, hostResolverRulesFragments);
        }
        catch { CealArgsFragments.Remove(hostName); }
        finally
        {
            string hostRules = string.Empty;
            string hostResolverRules = string.Empty;

            foreach ((string hostRulesFragments, string hostResolverRulesFragments) CealArgsFragment in CealArgsFragments.Values)
            {
                hostRules += CealArgsFragment.hostRulesFragments;
                hostResolverRules += CealArgsFragment.hostResolverRulesFragments;
            }

            CealArgs = @$"/c @start .\""Uncealed-Browser.lnk"" --host-rules=""{hostRules.TrimEnd(',')}"" --host-resolver-rules=""{hostResolverRules.TrimEnd(',')}"" --test-type --ignore-certificate-errors";
        }
    }
    private void MainWin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.W)
            Application.Current.Shutdown();
    }
}