using EasyUpdateFromGithub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using YamlDotNet.Core;
using MessageBox = System.Windows.MessageBox;

namespace Sheas_Cealer.Utils {
	internal class AutoCheckUpdate:Window {
		/// <summary>
		/// 对话框标题
		/// </summary>
		const string msgboxTitle = "更新检查";
		readonly UpdateFromGithub ufg = new() {
			RepositoryURL = "https://github.com/SpaceTimee/Sheas-Cealer",
			ProgramVersion= Assembly.GetExecutingAssembly().GetName().Version!.ToString(),
			EasySetCacheDir= "SheasCealer",
		};
		/// <summary>
		/// 表示当前是否正在执行程序更新函数，如果正在执行，则为true，阻止多次调用函数
		/// </summary>
		bool isCheckingUpdate=false;
		/// <summary>
		/// 程序更新函数
		/// </summary>
		/// <param name="isAuto">是否是自动检查更新，如果不是，则在检查完后反馈</param>
		internal async void ProgramUpdateAsync(bool isAuto = false) {
			if (!isCheckingUpdate) {
				isCheckingUpdate = true;
				try {
					UpdateFromGithub.CheckUpdateValue cuv = await ufg.CheckUpdateAsync();
					//获取Release页面latest版本中文件名符合正则表达式的文件
					UpdateFromGithub.InfoOfDownloadFile iodf=await ufg.GetDownloadFileInfoAsync(fileRegex:new(@"Sheas-Cealer-Zip-.+"));
					if (cuv.HaveUpdate) {
						switch (MessageBox.Show(
@$"检查到可用的更新，是否进行更新？
当前版本: V{ufg.ProgramVersion}
最新版本: {cuv.LatestVersionStr}
发布时间: {cuv.PublishedTime_Local}
大小: {iodf.Size}"
										, msgboxTitle, MessageBoxButton.YesNo, MessageBoxImage.Information)) {
							case MessageBoxResult.Yes:
								static void errorMsg() {
									MessageBox.Show("下载更新失败！", msgboxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
								}
								try {
									//下载目标文件
									UpdateFromGithub.InfoOfInstall? ioi = await ufg.DownloadReleaseAsync(iodf);
									if (ioi != null) {
										if (MessageBox.Show("最新版本下载完毕，是否执行安装？", msgboxTitle, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes) {
											ufg.InstallFile(ioi, waitTime: 900);
											this.Close();//执行安装时需要退出主程序
										}
									}
									else 
										errorMsg();
								} catch { errorMsg(); }
								break;
							default:
								break;
						}
					}
					else if (!isAuto)
						MessageBox.Show("当前已是最新版本", msgboxTitle, MessageBoxButton.OK, MessageBoxImage.Information);
				} catch {
					if (!isAuto)
						MessageBox.Show("更新检查失败！", msgboxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
				}
				isCheckingUpdate = false;
			}
		}
	}
}
