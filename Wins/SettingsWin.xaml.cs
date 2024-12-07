﻿using System;
using System.ComponentModel;
using System.Diagnostics;
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

    private void Enabled_SelfStarting_Click(object sender, RoutedEventArgs e) {
        SelfStarting ss = new();
        if (ss.RunCommand(SelfStarting.Action.add))
            MessageBox.Show("已启用开机自启");
    }
    private void Disabled_SelfStarting_Click(object sender, RoutedEventArgs e) {
        SelfStarting ss = new();
        if (ss.RunCommand(SelfStarting.Action.remove))
            MessageBox.Show("已移除开机自启");
    }
    private class SelfStarting {
        /// <summary>
        /// 自启动注册表路径
        /// </summary>
        readonly string regPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";
        /// <summary>
        /// 应用名
        /// </summary>
        readonly string appName = "SheasCealer";
        /// <summary>
        /// 当前exe文件所在目录
        /// </summary>
        internal string ThisExeFilePath => System.Windows.Forms.Application.ExecutablePath;
        //自启动程序时会添加启动参数，可以在程序启动时加个判断，判断有-selfStarting参数时进行自动最小化程序等操作
        string AddCommands => $"{RemoveCommands} & reg add {regPath} /f /v \"{appName}\" /t REG_SZ /d \"{ThisExeFilePath} -selfStarting\"";// & pause";        
        string RemoveCommands => $"reg delete {regPath} /f /v \"{appName}\"";
        internal enum Action {
            add,remove
        }
        /// <summary>
        /// 运行命令
        /// </summary>
        /// <param name="action">操作方式</param>
        /// <returns>返回布尔值代表执行是否成功</returns>
        internal bool RunCommand(Action action) {
            // 检查当前进程是否以管理员身份运行
            //if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            string commands = "";
            switch (action) {
                case Action.add:
                    commands = AddCommands; break;
                case Action.remove:
                    commands = RemoveCommands; break;
            }

            Process process = new() {
                StartInfo = new ProcessStartInfo {
                    UseShellExecute = true,
                    Verb = "RunAs", // 请求管理员权限
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = $" /c \"{ commands }\""
                }
            };
            bool isTrue = false;
            try {
                process.Start();
                process.WaitForExit();
                isTrue = true;
            } 
            catch (Win32Exception) { System.Windows.Forms.MessageBox.Show("用户取消了授权", "", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error); }
            catch { System.Windows.Forms.MessageBox.Show("发生未知错误！", "", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error); }
            process.Close();
            return isTrue;
        }
    }
}