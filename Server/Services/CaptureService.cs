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
        // 1. Select a device (for simplicity, pick the first)
        var devices = CaptureDeviceList.Instance;
        if (devices.Count < 1)
        {
            Console.WriteLine("No devices found!");
            return packets;
        }
        Console.WriteLine($"Devices found: {devices.Count}");
        var device = devices[5];
        Console.WriteLine($"Selected device: {device.MacAddress}\nDescription: {device.Description}");
        // 2. Open the device in promiscuous mode, with a read timeout
        device.Open(DeviceModes.Promiscuous, 1000);

        Console.WriteLine("Starting capture...");

        // 3. Continuous loop for capturing packets
        for (int i = 0; i < 100; i++)
        {

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
            Console.WriteLine("Above B");

            // (B) Process the packet
            ProcessPacket(parsedPacket);
            Console.WriteLine("Above C");
        }
        device.Close();
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

        // ... etc. for ICMP, ARP, etc. ...

        // If none of the above matched, handle other protocols
        info.Protocol = "Other";
        Console.WriteLine($"{ipPacket.Version} non-TCP/UDP/ICMP/ARP packet: {info.SourceIP} -> {info.DestinationIP}");
        // Possibly store or enqueue info
        packets.Enqueue(info);
    }
}

