using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using File = System.IO.File;

namespace Sheas_Cealer
{
    public partial class MainWindow : Window
    {
        private static string? CEALING_ARGUMENT;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Task.Run(() =>
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
                });
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowBox.Focus();

                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.BrowserPath))
                    ShowBox.Text = Properties.Settings.Default.BrowserPath;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }

        private void ShowBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ShowBox.Foreground != Foreground)
                {
                    //PlaceHold状态
                    ShowBox.Text = string.Empty;
                    ShowBox.Foreground = Foreground;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }
        private void ShowBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ShowBox.Text))
                {
                    ShowBox.Text = "(填入任意以 Chromium 为内核的浏览器的路径)";
                    ShowBox.Foreground = new SolidColorBrush(Color.FromRgb(191, 205, 219));
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
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

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new() { Filter = "EXE (*.exe)|*.exe" };

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

                string fileName = Path.GetFileNameWithoutExtension(ShowBox.Text);
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName == fileName)
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                }

                new Cmd().ShellRun(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, CEALING_ARGUMENT);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        private void MainWin_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.W)
                Environment.Exit(0);
        }
    }
}