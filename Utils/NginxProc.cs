using System;
using System.IO;
using Sheas_Cealer.Consts;
using SheasCore;

namespace Sheas_Cealer.Utils;

internal class NginxProc : Proc
{
    internal NginxProc() : base("Cealing-Nginx.exe") { }

    public override void Process_Exited(object sender, EventArgs e)
    {
        string hostsContent = File.ReadAllText(MainConst.HostsConfPath);
        int cealingNginxStartIndex = hostsContent.IndexOf(MainConst.HostsConfStartMarker);
        int cealingNginxEndIndex = hostsContent.LastIndexOf(MainConst.HostsConfEndMarker);

        if (cealingNginxStartIndex != -1 && cealingNginxEndIndex != -1)
            File.WriteAllText(MainConst.HostsConfPath, hostsContent.Remove(cealingNginxStartIndex, cealingNginxEndIndex - cealingNginxStartIndex + "# Cealing Nginx End".Length));
    }
}