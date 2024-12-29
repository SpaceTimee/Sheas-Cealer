using System.Runtime.InteropServices;

namespace Sheas_Cealer.Utils;

internal static partial class DnsFlusher
{
    [LibraryImport("dnsapi.dll")]
    private static partial void DnsFlushResolverCache();

    internal static void FlushDns() => DnsFlushResolverCache();
}
