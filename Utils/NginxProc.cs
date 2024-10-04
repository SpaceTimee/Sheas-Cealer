using System;
using System.IO;
using Microsoft.Win32;
using SheasCore;

namespace Sheas_Cealer.Utils;

internal class NginxProc : Proc
{
    internal NginxProc() : base("Cealing-Nginx.exe") { }

    public override void Process_Exited(object sender, EventArgs e)
    {
        string hostsPath = Path.Combine(Registry.LocalMachine.OpenSubKey(@"\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\DataBasePath")?.GetValue("DataBasePath", null)?.ToString() ?? @"C:\Windows\System32\drivers\etc", "hosts");
        string hostsContent = File.ReadAllText(hostsPath);
        int cealingNginxStartIndex = hostsContent.IndexOf("# Cealing Nginx Start\n");
        int cealingNginxEndIndex = hostsContent.LastIndexOf("# Cealing Nginx End");

        if (cealingNginxStartIndex != -1 && cealingNginxEndIndex != -1)
            File.WriteAllText(hostsPath, hostsContent.Remove(cealingNginxStartIndex, cealingNginxEndIndex - cealingNginxStartIndex + "# Cealing Nginx End".Length));
    }
}