using System;
using SharpPcap;
using PacketDotNet;
using SharpPcap.WinpkFilter;
using System.Net;
using Server.Models;

namespace Server.Services;

public class Capturer
{
    public static Queue<PacketInfo> packets = new Queue<PacketInfo>();

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
    public static Queue<PacketInfo> StartCapture()
    {
        packets = new Queue<PacketInfo>();

        var devices = CaptureDeviceList.Instance;
        if (devices == null || devices.Count < 1)
        {
            Console.WriteLine("❌ No network devices found!");
            return packets;
        }

        Console.WriteLine($"Devices found: {devices.Count}");

        // 🟢 Use LINQ to select the first matching device
        var targetMacs = new HashSet<string>
        {
            "80AFCA2211A1",
            "EC2E98072E01"
        };

        ILiveDevice? device = devices.FirstOrDefault(d =>
            d != null && !string.IsNullOrEmpty(d.MacAddress?.ToString()) &&
            targetMacs.Contains(d.MacAddress.ToString().ToUpper()));
        System.Console.WriteLine($"{device?.MacAddress?.ToString()}");
        // If no matching device, fall back to the first device
        System.Console.WriteLine("🟢 Using first device");
        device ??= devices.FirstOrDefault();
        System.Console.WriteLine($"{device?.MacAddress?.ToString()}");

        if (device == null)
        {
            Console.WriteLine("❌ No valid device found!");
            return packets;
        }

        Console.WriteLine($"🎯 Using device: {device.MacAddress}\nDescription: {device.Description}");

        try
        {
            device.Open(DeviceModes.Promiscuous, 1000);
            Console.WriteLine("🟢 Starting capture...");

            for (int i = 0; i < 600; i++)
            {
                GetPacketStatus status = device.GetNextPacket(out PacketCapture packetCapture);
                if (status != GetPacketStatus.PacketRead)
                    continue;

                RawCapture rawCapture = packetCapture.GetPacket();
                if (rawCapture == null)
                {
                    Console.WriteLine("⚠️ Received a null raw packet.");
                    continue;
                }

                Packet parsedPacket = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
                if (parsedPacket == null)
                {
                    Console.WriteLine("⚠️ Failed to parse packet.");
                    continue;
                }

                try
                {
                    ProcessPacket(parsedPacket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing packet: {ex.Message}");
                }
            }
           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception in packet capture: {ex.Message}");
        }
        finally
        {
            if (device != null)
            {
                try
                {
                    device.Close();
                    Console.WriteLine("🔴 Device closed successfully.");
                }
                catch (Exception closeEx)
                {
                    Console.WriteLine($"⚠️ Error closing device: {closeEx.Message}");
                }
            }
        }

        return packets;
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
            Console.WriteLine("Non-IP packet captured.");
            return;
        }

        // Prepare a model to store relevant fields
        var info = new PacketInfo
        {
            IPVersion = ipPacket.Version,
            SourceIP = ipPacket.SourceAddress.ToString(),
            DestinationIP = ipPacket.DestinationAddress.ToString(),
            Timestamp = DateTime.Now,  // or from the RawCapture if you need original timestamps
            packet = parsedPacket
        };

        // Extract TCP
        var tcp = parsedPacket.Extract<TcpPacket>();
        if (tcp != null)
        {
            info.SourcePort = tcp.SourcePort;
            info.DestinationPort = tcp.DestinationPort;
            info.Protocol = "TCP";
            Console.WriteLine($"TCP: {info.SourceIP}:{info.SourcePort} -> {info.DestinationIP}:{info.DestinationPort}");
            // Possibly store or enqueue info
            packets.Enqueue(info);
            return;
        }

        // Extract UDP
        var udp = parsedPacket.Extract<UdpPacket>();
        if (udp != null)
        {
            info.SourcePort = udp.SourcePort;
            info.DestinationPort = udp.DestinationPort;
            info.Protocol = "UDP";
            Console.WriteLine($"UDP: {info.SourceIP}:{info.SourcePort} -> {info.DestinationIP}:{info.DestinationPort}");
            // Possibly store or enqueue info
            packets.Enqueue(info);
            return;
        }
        var icmp = parsedPacket.Extract<IcmpV4Packet>();
        if (icmp != null)
        {
            info.Protocol = "ICMP";
            Console.WriteLine($"ICMP: {info.SourceIP} -> {info.DestinationIP}");
            // Possibly store or enqueue info
            packets.Enqueue(info);
            return;
        }
        var arp = parsedPacket.Extract<ArpPacket>();
        if (arp != null)
        {
            info.Protocol = "ARP";
            Console.WriteLine($"ARP: {info.SourceIP} -> {info.DestinationIP}");
            // Possibly store or enqueue info
            packets.Enqueue(info);
            return;
        }
        var icmp6 = parsedPacket.Extract<IcmpV6Packet>();
        if (icmp6 != null)
        {
            info.Protocol = "ICMPv6";
            Console.WriteLine($"ICMPv6: {info.SourceIP} -> {info.DestinationIP}");
            // Possibly store or enqueue info
            packets.Enqueue(info);
            return;
        }

        // If none of the above matched, handle other protocols
        info.Protocol = "Other";
        Console.WriteLine($"{ipPacket.Version} non-TCP/UDP/ICMP/ARP packet: {info.SourceIP} -> {info.DestinationIP}");
        // Possibly store or enqueue info
        packets.Enqueue(info);
    }
}

