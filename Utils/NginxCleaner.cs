using Sheas_Cealer.Consts;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Sheas_Cealer.Utils;

internal static class NginxCleaner
{
    internal static async Task Clean()
    {
        string hostsContent = await File.ReadAllTextAsync(MainConst.HostsConfPath);
        int hostsConfStartIndex = hostsContent.IndexOf(MainConst.HostsConfStartMarker, StringComparison.Ordinal);
        int hostsConfEndIndex = hostsContent.LastIndexOf(MainConst.HostsConfEndMarker, StringComparison.Ordinal);

        if (hostsConfStartIndex != -1 && hostsConfEndIndex != -1)
            await File.WriteAllTextAsync(MainConst.HostsConfPath, hostsContent.Remove(hostsConfStartIndex, hostsConfEndIndex - hostsConfStartIndex + MainConst.HostsConfEndMarker.Length));

        using X509Store certStore = new(StoreName.Root, StoreLocation.LocalMachine, OpenFlags.ReadWrite);

        foreach (X509Certificate2 storedCert in certStore.Certificates)
            if (storedCert.Subject == MainConst.NginxRootCertSubjectName)
                while (true)
                    try
                    {
                        certStore.Remove(storedCert);

                        break;
                    }
                    catch { }

        certStore.Close();
    }
}