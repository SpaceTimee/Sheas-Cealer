using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using OnaCore;
using File = System.IO.File;

namespace Sheas_Cealer
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient MAIN_CLIENT = new();    //当前窗口使用的唯一的 HttpClient
        private static readonly FileSystemWatcher CEALING_HOST_WATCHER = new(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Host.json") { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
        [GeneratedRegex(@"\r")] private static partial Regex HOST_REGEX();
        private static string? CEALING_ARGUMENT;

        internal MainWindow(string[] args)
        {
            InitializeComponent();

            Task.Run(() =>
            {
                if (args.Length > 0)
                    Dispatcher.Invoke(() => PathBox.Text = args[0]);
                else if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.BrowserPath))
                    Dispatcher.Invoke(() => PathBox.Text = Properties.Settings.Default.BrowserPath);

                CEALING_HOST_WATCHER.Changed += CEALING_HOST_WATCHER_Changed;
                CEALING_HOST_WATCHER_Changed(null!, null!);
            });
        }
        private void MainWin_Loaded(object sender, RoutedEventArgs e) => PathBox.Focus();
        private void MainWin_Closing(object sender, CancelEventArgs e) => Environment.Exit(0);

        private void PathBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PathBox.Foreground != Foreground)
            {
                //PlaceHold状态
                PathBox.Text = string.Empty;
                PathBox.Foreground = Foreground;
            }
        }
        private void PathBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PathBox.Text))
            {
                PathBox.Text = "(填入任意以 Chromium 为内核的浏览器的路径)";
                PathBox.Foreground = new SolidColorBrush(Color.FromRgb(191, 205, 219));
            }
        }
        private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StartButton == null)
                return;

            if (File.Exists(PathBox.Text) && Path.GetFileName(PathBox.Text).EndsWith(".exe"))
            {
                StartButton.IsEnabled = true;

                Properties.Settings.Default.BrowserPath = PathBox.Text;
                Properties.Settings.Default.Save();
            }
            else
                StartButton.IsEnabled = false;
        }

        private void MainWin_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Link;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }
        private void MainWin_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                PathBox.Text = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new() { Filter = "浏览器 (*.exe)|*.exe" };

            if (openFileDialog.ShowDialog() == true)
            {
                PathBox.Focus();
                PathBox.Text = openFileDialog.FileName;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CEALING_ARGUMENT))
                throw new Exception("规则无法识别，请检查伪造规则是否含有语法错误");
            if (MessageBox.Show("启动前将关闭所选浏览器的所有进程，是否继续？", string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            IWshShortcut uncealedBrowserShortcut = (IWshShortcut)new WshShell().CreateShortcut(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, @"Uncealed-Browser.lnk"));
            uncealedBrowserShortcut.TargetPath = PathBox.Text;
            uncealedBrowserShortcut.Description = "Created By Sheas Cealer";
            uncealedBrowserShortcut.Save();

            foreach (Process browserProcess in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(PathBox.Text)))
            {
                browserProcess.Kill();
                browserProcess.WaitForExit();
            }

            new Command().ShellRun(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, CEALING_ARGUMENT);
        }

        private void HostEditButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo processStartInfo = new(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, @"Cealing-Host.json")) { UseShellExecute = true };
            Process.Start(processStartInfo);
        }
        private async void HostUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            const string hostUrl = @"https://gitlab.com/SpaceTimee/Cealing-Host/raw/main/Cealing-Host.json";

            string hostUpdateString = await Http.GetAsync<string>(hostUrl, MAIN_CLIENT);
            StreamReader hostLocalStreamReader = new(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, @"Cealing-Host.json"));
            string hostLocalString = hostLocalStreamReader.ReadToEnd();
            hostLocalStreamReader.Close();

            if (HOST_REGEX().Replace(hostLocalString, string.Empty) == hostUpdateString)
                MessageBox.Show("本地伪造规则和上游一模一样");
            else
            {
                MessageBoxResult overrideResult = MessageBox.Show("本地伪造规则和上游略有不同，需要覆盖本地吗? 否则只为你打开上游规则的网页", "", MessageBoxButton.YesNoCancel);
                if (overrideResult == MessageBoxResult.Yes)
                {
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, @"Cealing-Host.json"), hostUpdateString);
                    MessageBox.Show("更新已完成");
                }
                else if (overrideResult == MessageBoxResult.No)
                    Process.Start(new ProcessStartInfo(hostUrl) { UseShellExecute = true });
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e) => new AboutWindow().ShowDialog();

        private void CEALING_HOST_WATCHER_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                string hostRules = string.Empty, hostResolverRules = string.Empty;
                int ruleIndex = 0;
                FileStream hostStream = new(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, @"Cealing-Host.json"), FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                JArray hostJArray = JArray.Parse(new StreamReader(hostStream).ReadToEnd());

                foreach (var hostJToken in hostJArray)
                {
                    if (string.IsNullOrWhiteSpace(hostJToken[1]!.ToString()))
                        hostJToken[1] = "c" + ruleIndex;

                    hostResolverRules += "MAP " + hostJToken[1]!.ToString() + " " + hostJToken[2]!.ToString() + ",";
                    foreach (var hostName in hostJToken[0]!)
                        hostRules += "MAP " + hostName.ToString() + " " + hostJToken[1] + ",";

                    ++ruleIndex;
                }

                CEALING_ARGUMENT = @"/c @start .\""Uncealed-Browser.lnk"" --host-rules=""" + hostRules[0..^1] + @""" --host-resolver-rules=""" + hostResolverRules[0..^1] + @""" --test-type --ignore-certificate-errors";
            }
            catch { CEALING_ARGUMENT = string.Empty; }
        }

        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.W)
                Environment.Exit(0);
        }
    }
}