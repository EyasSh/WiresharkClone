using Server.Models;
using PacketDotNet;


namespace Server.Services;
public static class Analyzer
{

    /// <summary>
    /// Detect SYN flood: count TCP SYN packets per source IP in the time window
    /// </summary>
    /// <param name="packets">Enumerable of <see cref="PacketInfo"/> objects representing the packets.</param>
    /// <param name="synThreshold">Threshold of TCP SYN packets per source IP.</param>
    /// <param name="window">Time window in which to detect SYN flood.</param>
    public static void DetectSynFlood(
        IEnumerable<PacketInfo> packets,
        int synThreshold,
        TimeSpan window)
    {
        var cutoff = DateTime.UtcNow - window;
        // filter only TCP SYN packets in the time window
        var synPkts = packets
            .Where(p =>
                p.Timestamp >= cutoff &&
                p.Protocol == "TCP" &&
                p.packet?.Extract<TcpPacket>() is TcpPacket tcp &&
                tcp.Synchronize && !tcp.Acknowledgment)
            .GroupBy(p => p.SourceIP);

        foreach (var grp in synPkts)
        {
            if (grp.Count() > synThreshold)
            {
                Console.WriteLine($"⚠️ SYN-Flood suspected from {grp.Key}: {grp.Count()} SYNs in last {window.TotalSeconds}s");
                foreach (var pi in grp)
                    pi.isSuspicious = true;
            }
        }
    }


    /// <summary>
    /// Detect UDP flood: count packets per source IP in the time window
    /// </summary>
    /// <param name="packets">Enumerable of <see cref="PacketInfo"/> objects</param>
    /// <param name="udpThreshold">Threshold of UDP packets per source IP</param>
    /// <param name="window">Time window in which to detect UDP flood</param>
    public static void DetectUdpFlood(
        IEnumerable<PacketInfo> packets,
        int udpThreshold,
        TimeSpan window)
    {
        var cutoff = DateTime.UtcNow - window;
        var udpPkts = packets
            .Where(p =>
                p.Timestamp >= cutoff &&
                p.Protocol == "UDP")
            .GroupBy(p => p.SourceIP);

        foreach (var grp in udpPkts)
        {
            if (grp.Count() > udpThreshold)
            {
                Console.WriteLine($"⚠️ UDP-Flood suspected from {grp.Key}: {grp.Count()} packets in last {window.TotalSeconds}s");
                foreach (var pi in grp)
                    pi.isSuspicious = true;
            }
        }
    }


    /// <summary>
    /// Detect TCP Port Scan: count unique destination ports per source IP
    /// </summary>
    /// <param name="packets">Enumerable of <see cref="PacketInfo"/> objects representing the packets.</param>
    /// <param name="portScanThreshold">The minimum number of unique destination ports to consider a port scan.</param>
    /// <param name="window">The time window to consider.</param>
    public static void DetectPortScan(
        IEnumerable<PacketInfo> packets,
        int portScanThreshold,
        TimeSpan window)
    {
        var cutoff = DateTime.UtcNow - window;
        var scans = packets
            .Where(p =>
                p.Timestamp >= cutoff &&
                p.Protocol == "TCP")
            .GroupBy(p => p.SourceIP)
            .Select(g => new
            {
                Source = g.Key,
                UniquePorts = g
                    .Where(p => p.DestinationPort.HasValue)
                    .Select(p => p.DestinationPort.HasValue ? p.DestinationPort.Value : 0)
                    .Distinct()
                    .Count()
            });

        foreach (var scan in scans)
        {
            if (scan.UniquePorts > portScanThreshold)
            {
                Console.WriteLine($"⚠️ Port-scan suspected from {scan.Source}: {scan.UniquePorts} distinct ports in last {window.TotalSeconds}s");
                foreach (var pi in packets.Where(p => p.SourceIP == scan.Source))
                    pi.isSuspicious = true;
            }
        }
    }


    /// <summary>
    /// Detects Ping of Death attacks by analyzing packet sizes in an enumerable of <see cref="PacketInfo"/> objects.
    /// A Ping of Death is suspected if an ICMP echo-request packet's total length exceeds the maximum allowable IP size of 65,535 bytes.
    /// If a packet is identified as a Ping of Death, it is marked as suspicious and malicious.
    /// </summary>
    /// <param name="packets">Enumerable of <see cref="PacketInfo"/> objects representing the captured packets to be analyzed.</param>
    public static void DetectPingOfDeathV4(
        IEnumerable<PacketInfo> packets)
    {
        const int MaxIpSize = 65_535;
        foreach (var p in packets)
        {
            // extract IPv4 and ICMPv4 layers
            var ip = p.packet?.Extract<IPv4Packet>();
            var icmp = p.packet?.Extract<IcmpV4Packet>();
            if (ip == null || icmp == null)
                continue;

            // only care about echo-requests (type 8)
            if (icmp.TypeCode == IcmpV4TypeCode.EchoRequest)
            {
                // PacketDotNet gives you the IP header’s TotalLength
                // which includes header + payload
                if (ip.TotalLength > MaxIpSize)
                {
                    Console.WriteLine(
                        $"🚨 Ping-of-Death from {p.SourceIP}: IP.Length={ip.TotalLength}");
                    p.isSuspicious = true;
                    p.isMalicious = true;
                }
            }
        }
    }
    public static void DetectPingOfDeathV6(
        IEnumerable<PacketInfo> packets)
    {
        const int MaxIpSize = 65_535;
        foreach (var p in packets)
        {
            // extract IPv6 and ICMPv6 layers
            var ip = p.packet?.Extract<IPv6Packet>();
            var icmp = p.packet?.Extract<IcmpV6Packet>();
            if (ip == null || icmp == null)
                continue;

            // only care about echo-requests (type 128)
            if (icmp.Type == IcmpV6Type.EchoRequest)
            {
                // PacketDotNet gives you the IP header’s PayloadLength
                // which is just the payload, not including the header
                if (ip.PayloadLength > MaxIpSize)
                {
                    Console.WriteLine(
                        $"🚨 Ping-of-Death from {p.SourceIP}: IP.Length={ip.PayloadLength}");
                    p.isSuspicious = true;
                    p.isMalicious = true;
                }
            }
        }
    }
}


