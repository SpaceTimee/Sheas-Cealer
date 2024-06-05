using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sheas_Cealer.Utils;

namespace Sheas_Cealer.Wins;

public partial class AboutWin : Window
{
    internal AboutWin() => InitializeComponent();
    protected override void OnSourceInitialized(EventArgs e) => IconRemover.RemoveIcon(this);

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender as Button == VersionButton)
            MessageBox.Show("密码: 3wnj");

        ProcessStartInfo processStartInfo = new(sender as Button == EmailButton ? "mailto:" : string.Empty + (sender as Button)!.ToolTip) { UseShellExecute = true };
        Process.Start(processStartInfo);
    }

    private void AboutWin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
    }
}