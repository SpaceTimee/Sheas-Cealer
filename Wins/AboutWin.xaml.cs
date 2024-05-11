using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sheas_Cealer.Wins
{
    public partial class AboutWin : Window
    {
        internal AboutWin()
        {
            InitializeComponent();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == VersionButton)
                MessageBox.Show("密码: 3wnj");

            ProcessStartInfo processStartInfo = new(sender == EmailButton ? "mailto:" : string.Empty + (sender as Button)!.ToolTip) { UseShellExecute = true };
            Process.Start(processStartInfo);
        }

        private void AboutWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}