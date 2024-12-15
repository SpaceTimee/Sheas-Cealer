using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Sheas_Cealer.Consts;

namespace Sheas_Cealer.Utils
{
    internal static class NginxCleaner
    {
        private static readonly object IsCleaningLock = new();

        internal static void Clean()
        {
            if (!Monitor.TryEnter(IsCleaningLock))
                return;

            try
            {
                string hostsContent = File.ReadAllText(MainConst.HostsConfPath);
                int hostsConfStartIndex = hostsContent.IndexOf(MainConst.HostsConfStartMarker);
                int hostsConfEndIndex = hostsContent.LastIndexOf(MainConst.HostsConfEndMarker);

                if (hostsConfStartIndex != -1 && hostsConfEndIndex != -1)
                    File.WriteAllText(MainConst.HostsConfPath, hostsContent.Remove(hostsConfStartIndex, hostsConfEndIndex - hostsConfStartIndex + MainConst.HostsConfEndMarker.Length));

                using X509Store certStore = new(StoreName.Root, StoreLocation.CurrentUser, OpenFlags.ReadWrite);

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
            finally { Monitor.Exit(IsCleaningLock); }
        }
    }
}