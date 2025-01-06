using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Props;
using Sheas_Cealer.Utils;
using Microsoft.Win32;

namespace Sheas_Cealer.Wins;

public partial class SettingsWin : Window
{
    private readonly SettingsPres SettingsPres;

    internal SettingsWin()
    {
        InitializeComponent();

        DataContext = SettingsPres = new();
    }
    protected override void OnSourceInitialized(EventArgs e)
    {
        IconRemover.RemoveIcon(this);
        BorderThemeSetter.SetBorderTheme(this, SettingsPres.IsLightTheme);
    }

    private void ThemesButton_Click(object sender, RoutedEventArgs e) => SettingsPres.IsLightTheme = SettingsPres.IsLightTheme.HasValue ? SettingsPres.IsLightTheme.Value ? null : true : false;
    private void LangsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsPres.IsEnglishLang = SettingsPres.IsEnglishLang.HasValue ? SettingsPres.IsEnglishLang.Value ? null : true : false;

        MessageBox.Show(SettingsConst._ChangeLangSuccessMsg);
    }
    private void ColorsButton_Click(object sender, RoutedEventArgs e)
    {
        Random random = new();
        PaletteHelper paletteHelper = new();
        Theme newTheme = paletteHelper.GetTheme();
        Color newPrimaryColor = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));

        newTheme.SetPrimaryColor(newPrimaryColor);
        paletteHelper.SetTheme(newTheme);

        Style newButtonStyle = new(typeof(Button), Application.Current.Resources[typeof(Button)] as Style);
        (Color? newForegroundColor, Color newAccentForegroundColor) = ForegroundGenerator.GetForeground(newPrimaryColor.R, newPrimaryColor.G, newPrimaryColor.B);

        newButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, newForegroundColor.HasValue ? new SolidColorBrush(newForegroundColor.Value) : new DynamicResourceExtension("MaterialDesignBackground")));
        Application.Current.Resources[typeof(Button)] = newButtonStyle;

        SettingsPres.AccentForegroundColor = newAccentForegroundColor;

        Settings.Default.PrimaryColor = System.Drawing.Color.FromArgb(newPrimaryColor.A, newPrimaryColor.R, newPrimaryColor.G, newPrimaryColor.B);
        Settings.Default.Save();
    }
    private void WeightsButton_Click(object sender, RoutedEventArgs e) => SettingsPres.IsLightWeight = SettingsPres.IsLightWeight.HasValue ? SettingsPres.IsLightWeight.Value ? null : true : false;

    private void SettingsWin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
    }

	readonly SelfStarting selfStarting = new();
    private void SelfStartingToggle_Click(object sender, RoutedEventArgs e) {
        //在此处为了方便演示，就直接使用了按钮名字进行判断，不建议这么做
        if( SelfStartingToggle.Content as string == "启用>>禁用 程序开机自启动") {
            selfStarting.Setting = false;
            SelfStartingToggle.Content = "禁用>>启用 程序开机自启动";
		}
        else {
			selfStarting.Setting = true;
			SelfStartingToggle.Content = "启用>>禁用 程序开机自启动";
		}
    }

	private class SelfStarting {
        /// <summary>
        /// 自启动注册表路径
        /// </summary>
        readonly string regPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        /// <summary>
        /// 应用名
        /// </summary>
        readonly string appName = "SheasCealer";
        /// <summary>
        /// 当前exe文件所在目录
        /// </summary>
        internal string ThisExeFilePath => System.Windows.Forms.Application.ExecutablePath;

        /// <summary>
        /// 设置开机自启
        /// </summary>
		internal bool Setting {
			get {
				using RegistryKey? key = Registry.CurrentUser.OpenSubKey(regPath, false);
				return key?.GetValue(appName) != null;
			}
			set {
				void DeleteReg() {
					using RegistryKey? key = Registry.CurrentUser.OpenSubKey(regPath, true);
					key?.DeleteValue(appName, false);
				}
				switch (value) {
					case true: {
							DeleteReg();
							using RegistryKey? key = Registry.CurrentUser.OpenSubKey(regPath, true);
							//自启动程序时会添加启动参数，可以在程序启动时加个判断，判断有-selfStarting参数时进行自动最小化程序等操作
							key?.SetValue(appName, $"{ThisExeFilePath} -selfStarting", RegistryValueKind.String);
							break;
						}
					case false:
						DeleteReg();
						break;
				}
			}
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e) => 
        //检查当前选项状态
        SelfStartingToggle.Content = selfStarting.Setting ? "启用>>禁用 程序开机自启动" : "禁用>>启用 程序开机自启动";
}