using System;
using SharpPcap;
using PacketDotNet;
using SharpPcap.WinpkFilter;

namespace Server.Services
{
    public class Capturer
    {
        private static bool _stopCapture = false;
        public static Stack<Packet> packets = new Stack<Packet>();

        /// <summary>
        /// Starts the capture process for network packets.
        /// This is a continuous loop that will run until <see cref="_stopCapture"/> is set to <c>true</c>.
        /// </summary>
        /// <remarks>
        /// This method will select the first device in the list of available network devices.
        /// It will then open the device in promiscuous mode, with a read timeout of 1000 milliseconds.
        /// The method will then enter an infinite loop, capturing packets one by one.
        /// For each packet, it will parse the packet using <see cref="Packet.ParsePacket"/>, and then process it using <see cref="ProcessPacket"/>, and (optionally) sleep for a small period of time to prevent busy-waiting.
        /// </remarks>
        public static void StartCapture()
        {
            _stopCapture = false;
            // 1. Select a device (for simplicity, pick the first)
            var devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                Console.WriteLine("No devices found!");
                return;
            }

            var device = devices[0];
            Console.WriteLine($"Selected device: {device.Name}\nDescription: {device.Description}");
            // 2. Open the device in promiscuous mode, with a read timeout
            device.Open(DeviceModes.Promiscuous, 1000);

            Console.WriteLine("Starting capture...");

            // 3. Continuous loop for capturing packets
            for (int i = 0; i < 750; i++)
            {
                // If user requested a manual stop from elsewhere, break early.
                if (_stopCapture)
                    break;

                // Attempt to read the next packet
                GetPacketStatus status = device.GetNextPacket(out PacketCapture packetCapture);
                if (status != GetPacketStatus.PacketRead)
                {
                    // If no packet was read this iteration (timeout, error, etc.), loop again
                    continue;
                }

                Console.WriteLine("Above A");
                // (A) Parse the packet
                RawCapture rawCapture = packetCapture.GetPacket();
                Packet parsedPacket = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
                packets.Push(parsedPacket);
                Console.WriteLine("Above B");

                // (B) Process the packet
                ProcessPacket(parsedPacket);
                Console.WriteLine("Above C");
            }

            Console.WriteLine("Stopping capture...");
            StopCapture();
            device.Close();
        }

        /// <summary>
        /// Processes the parsed Packet and prints protocol/addresses.
        /// Extracts IP layer, then checks for TCP, UDP, ICMP, ARP, etc.
        /// </summary>
        private static void ProcessPacket(Packet parsedPacket)
        {
            // Check for IP layer (IPv4 or IPv6)
            var ipPacket = parsedPacket.Extract<IPPacket>();
            if (ipPacket == null)
            {
                // Not IPv4/IPv6, could be ARP at the Ethernet level or something else
                Console.WriteLine("Non-IP packet captured.");
                return;
            }

            if (ipPacket is IPv4Packet)
            {
                Console.WriteLine("IPv4 packet captured.");
            }
            else if (ipPacket is IPv6Packet)
            {
                Console.WriteLine("IPv6 packet captured.");
            }

            var srcIp = ipPacket.SourceAddress;
            var dstIp = ipPacket.DestinationAddress;

            // Check for TCP
            var tcp = parsedPacket.Extract<TcpPacket>();
            if (tcp != null)
            {
                Console.WriteLine($"TCP: {srcIp}:{tcp.SourcePort} -> {dstIp}:{tcp.DestinationPort}");
                return; // done
            }

            // Check for UDP
            var udp = parsedPacket.Extract<UdpPacket>();
            if (udp != null)
            {
                Console.WriteLine($"UDP: {srcIp}:{udp.SourcePort} -> {dstIp}:{udp.DestinationPort}");
                return; // done
            }

            // Check for ICMP
            if (ipPacket.Version == IPVersion.IPv4)
            {
                var icmpV4 = parsedPacket.Extract<IcmpV4Packet>();
                if (icmpV4 != null)
                {
                    Console.WriteLine($"ICMPv4: {srcIp} -> {dstIp}");
                    return;
                }
            }
            else if (ipPacket.Version == IPVersion.IPv6)
            {
                var icmpV6 = parsedPacket.Extract<IcmpV6Packet>();
                if (icmpV6 != null)
                {
                    Console.WriteLine($"ICMPv6: {srcIp} -> {dstIp}");
                    return;
                }
            }

            // Check for ARP
            var arp = parsedPacket.Extract<ArpPacket>();
            if (arp != null)
            {
                Console.WriteLine($"ARP: {arp.SenderProtocolAddress} -> {arp.TargetProtocolAddress}");
                return;
            }

            // If none of the above matched, handle other protocols
            Console.WriteLine($"{ipPacket.Version} non-TCP/UDP/ICMP/ARP packet: {srcIp} -> {dstIp}");
        }

        /// <summary>
        /// Call this if you need a way to stop from outside
        /// for graceful shutdown.
        /// </summary>
        public static void StopCapture()
        {
            _stopCapture = true;
        }
    }
}
