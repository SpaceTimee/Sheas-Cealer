using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using File = System.IO.File;

namespace Sheas_Cealer
{
    public partial class MainWindow : Window
    {
        private static string? CEALING_ARGUMENT;
        private static readonly FileSystemWatcher CEALING_HOST_WATCHER = new(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "Cealing-Host.json") { EnableRaisingEvents = true, NotifyFilter = NotifyFilters.LastWrite };
        private static readonly DispatcherTimer MONITOR_TIMER = new() { Interval = new TimeSpan(1000000) };  //0.1s

        internal MainWindow(string[] args)
        {
            InitializeComponent();

            try
            {
                Task.Run(() =>
                {
                    MONITOR_TIMER.Tick += MONITOR_TIMER_Tick;
                    MONITOR_TIMER.Start();

                    if (args.Length > 0)
                        Dispatcher.Invoke(() => ShowBox.Text = args[0]);
                    else if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.BrowserPath))
                        Dispatcher.Invoke(() => ShowBox.Text = Properties.Settings.Default.BrowserPath);

                    CEALING_HOST_WATCHER.Changed += CEALING_HOST_WATCHER_Changed;
                    CEALING_HOST_WATCHER_Changed(null!, null!);
                });
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
        private void MainWin_Loaded(object sender, RoutedEventArgs e) => ShowBox.Focus();
        private void MainWin_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Environment.Exit(0);

        private void ShowBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ShowBox.Foreground != Foreground)
            {
                //PlaceHold状态
                ShowBox.Text = string.Empty;
                ShowBox.Foreground = Foreground;
            }
        }
        private void ShowBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ShowBox.Text))
            {
                ShowBox.Text = "(填入任意以 Chromium 为内核的浏览器的路径)";
                ShowBox.Foreground = new SolidColorBrush(Color.FromRgb(191, 205, 219));
            }
        }
        private void ShowBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                if (OpenButton == null)
                    return;

                if (File.Exists(ShowBox.Text) && Path.GetFileName(ShowBox.Text).EndsWith(".exe"))
                {
                    OpenButton.IsEnabled = true;

                    Properties.Settings.Default.BrowserPath = ShowBox.Text;
                    Properties.Settings.Default.Save();
                }
                else
                    OpenButton.IsEnabled = false;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }

        private void MainWin_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                e.Handled = true;

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effects = DragDropEffects.Link;
                else
                    e.Effects = DragDropEffects.None;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }
        private void MainWin_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    ShowBox.Text = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new() { Filter = "浏览器 (*.exe)|*.exe" };

                if (openFileDialog.ShowDialog() == true)
                {
                    ShowBox.Focus();
                    ShowBox.Text = openFileDialog.FileName;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessStartInfo processStartInfo = new(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, @"Cealing-Host.json")) { UseShellExecute = true };
                Process.Start(processStartInfo);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }
        private void ClashButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey proxyKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true)!;
                if (ClashButton.Content.ToString() == "代理")
                {
                    proxyKey.SetValue("ProxyEnable", 1);
                    proxyKey.SetValue("ProxyServer", "127.0.0.1:7880");

                    new Clash().ShellRun(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "-d .");
                }
                else
                {
                    proxyKey.SetValue("ProxyEnable", 0);

                    foreach (Process clashProcess in Process.GetProcessesByName("Cealing-Clash"))
                        clashProcess.Kill();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("启动前将关闭所选浏览器的所有进程，是否继续？", string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;

                IWshShortcut uncealedBrowserShortcut = (IWshShortcut)new WshShell().CreateShortcut(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, @"Uncealed-Browser.lnk"));
                uncealedBrowserShortcut.TargetPath = ShowBox.Text;
                uncealedBrowserShortcut.Description = "Created By Sheas Cealer";
                uncealedBrowserShortcut.Save();

                foreach (Process browserProcess in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ShowBox.Text)))
                {
                    browserProcess.Kill();
                    browserProcess.WaitForExit();
                }

                new Cmd().ShellRun(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, CEALING_ARGUMENT);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }
        private void AboutButton_Click(object sender, RoutedEventArgs e) => new AboutWindow().ShowDialog();

        private void MONITOR_TIMER_Tick(object? sender, EventArgs e)
        {
            if (Process.GetProcessesByName("Cealing-Clash").Length == 0)
                ClashButton.Content = "代理";
            else
                ClashButton.Content = "停代";
        }

        private void CEALING_HOST_WATCHER_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                string hostRules = string.Empty, hostResolverRules = string.Empty;
                FileStream hostStream = new(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, @"Cealing-Host.json"), FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                JArray hostJArray = JArray.Parse(new StreamReader(hostStream).ReadToEnd());

                foreach (var hostJToken in hostJArray)
                {
                    hostResolverRules += "MAP " + hostJToken[1]!.ToString() + " " + hostJToken[2]!.ToString() + ",";
                    foreach (var hostName in hostJToken[0]!)
                        hostRules += "MAP " + hostName.ToString() + " " + hostJToken[1] + ",";
                }

                CEALING_ARGUMENT = @"/c @start .\""Uncealed-Browser.lnk"" --host-rules=""" + hostRules[0..^1] + @""" --host-resolver-rules=""" + hostResolverRules[0..^1] + @""" --test-type --ignore-certificate-errors";
            }
            catch { return; }
        }

        private void MainWin_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.W)
                Environment.Exit(0);
        }
    }
}