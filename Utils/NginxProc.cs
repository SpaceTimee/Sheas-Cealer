using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
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

        using X509Store certStore = new(StoreName.Root, StoreLocation.CurrentUser, OpenFlags.ReadWrite);

        foreach (X509Certificate2 cert in certStore.Certificates)
            if (cert.Subject == "CN=Cealing Cert Root")
                certStore.Remove(cert);
    }
}