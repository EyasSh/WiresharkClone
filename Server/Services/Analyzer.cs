using Server.Models;
using PacketDotNet;


namespace Server.Services
{
    public static class Analyzer
    {
        /// <summary>
        /// Detect TCP SYN-flood: count pure SYN packets (Syn && !Ack) per source IP
        /// </summary>
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
        /// Detect UDP flood: count all UDP packets per source IP
        /// </summary>
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
        /// Detect simple TCP port-scan: count distinct dest ports per source IP
        /// </summary>
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
    }
}
