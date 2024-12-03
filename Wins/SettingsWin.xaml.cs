using System;
using System.Windows;
using System.Windows.Input;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Utils;

namespace Sheas_Cealer.Wins;

public partial class SettingsWin : Window
{
    private static SettingsPres? SettingsPres;

    internal SettingsWin()
    {
        InitializeComponent();

        DataContext = SettingsPres = new();
    }
    protected override void OnSourceInitialized(EventArgs e)
    {
        IconRemover.RemoveIcon(this);
        BorderThemeSetter.SetBorderTheme(this, SettingsPres!.IsLightTheme);
    }

    private void ThemesButton_Click(object sender, RoutedEventArgs e) => SettingsPres!.IsLightTheme = SettingsPres.IsLightTheme.HasValue ? SettingsPres.IsLightTheme.Value ? null : true : false;

    private void SettingsWin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
    }
}