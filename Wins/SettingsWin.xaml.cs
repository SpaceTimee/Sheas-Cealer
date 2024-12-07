using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Props;
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
    private void LangsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsPres!.IsEnglishLang = SettingsPres.IsEnglishLang.HasValue ? SettingsPres.IsEnglishLang.Value ? null : true : false;

        MessageBox.Show(SettingsConst._ChangeLangSuccessMsg);
    }
    private void ColorsButton_Click(object sender, RoutedEventArgs e)
    {
        Random random = new();

        PaletteHelper paletteHelper = new();
        Theme newTheme = paletteHelper.GetTheme();
        Color newColor = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));

        newTheme.SetPrimaryColor(newColor);
        paletteHelper.SetTheme(newTheme);

        Color? foregroundColor = ForegroundGenerator.GetForeground(newColor.R, newColor.G, newColor.B);

        if (foregroundColor.HasValue)
            Application.Current.Resources["MaterialDesignBackground"] = new SolidColorBrush(foregroundColor.Value);
        else
            Application.Current.Resources.Remove("MaterialDesignBackground");

        Settings.Default.PrimaryColor = System.Drawing.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
        Settings.Default.Save();
    }
    private void WeightsButton_Click(object sender, RoutedEventArgs e) => SettingsPres!.IsLightWeight = SettingsPres.IsLightWeight.HasValue ? SettingsPres.IsLightWeight.Value ? null : true : false;

    private void SettingsWin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
    }
}