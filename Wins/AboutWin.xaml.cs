using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Utils;

namespace Sheas_Cealer.Wins;

public partial class AboutWin : Window
{
    private static AboutPres? AboutPres;

    internal AboutWin()
    {
        InitializeComponent();

        AboutPres = new();
    }
    protected override void OnSourceInitialized(EventArgs e)
    {
        IconRemover.RemoveIcon(this);
        BorderThemeSetter.SetBorderTheme(this, AboutPres!.IsLightTheme);
    }

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        Button? senderButton = sender as Button;

        if (senderButton == VersionButton)
            MessageBox.Show($"{AboutConst._ReleasePagePasswordLabel} 3wnj");

        ProcessStartInfo processStartInfo = new(senderButton == EmailButton ? "mailto:" : string.Empty + senderButton!.ToolTip) { UseShellExecute = true };
        Process.Start(processStartInfo);
    }

    private void AboutWin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
    }
}