using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sheas_Cealer
{
    public partial class AboutWindow : Window
    {
        internal AboutWindow()
        {
            InitializeComponent();
        }
        private void AboutWin_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateButton.Content = "版本号: " + Assembly.GetExecutingAssembly().GetName().Version!.ToString()[0..^2];
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == UpdateButton)
                    MessageBox.Show("密码: 3wnj");

                ProcessStartInfo processStartInfo = new(sender == EmailButton ? "mailto:" : string.Empty + ((Button)sender).ToolTip) { UseShellExecute = true };
                Process.Start(processStartInfo);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }

        private void AboutWin_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}