using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Sheas_Cealer.Consts;
using SheasCore;

namespace Sheas_Cealer.Utils;

internal class NginxProc : Proc
{
    internal NginxProc() : base(Path.GetFileName(MainConst.NginxPath)) { }

    public override void Process_Exited(object sender, EventArgs e)
    {
        string hostsContent = File.ReadAllText(MainConst.HostsConfPath);
        int hostsConfStartIndex = hostsContent.IndexOf(MainConst.HostsConfStartMarker);
        int hostsConfEndIndex = hostsContent.LastIndexOf(MainConst.HostsConfEndMarker);

        if (hostsConfStartIndex != -1 && hostsConfEndIndex != -1)
            File.WriteAllText(MainConst.HostsConfPath, hostsContent.Remove(hostsConfStartIndex, hostsConfEndIndex - hostsConfStartIndex + MainConst.HostsConfEndMarker.Length));

        using X509Store certStore = new(StoreName.Root, StoreLocation.CurrentUser, OpenFlags.ReadWrite);

        foreach (X509Certificate2 storedCert in certStore.Certificates)
            if (storedCert.Subject == MainConst.NginxRootCertSubjectName)
                certStore.Remove(storedCert);

        certStore.Close();
    }
}