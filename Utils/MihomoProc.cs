using System;
using Microsoft.Win32;
using SheasCore;

namespace Sheas_Cealer.Utils;

internal class MihomoProc : Proc
{
    internal MihomoProc() : base("Cealing-Mihomo.exe") { }

    public override void Process_Exited(object sender, EventArgs e) => Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true)!.SetValue("ProxyEnable", 0);
}