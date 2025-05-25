using System;
using SharpPcap;
using PacketDotNet;
using SharpPcap.WinpkFilter;
using System.Net;
using Server.Models;
using System.Text;
using System.Collections.Concurrent;

namespace Server.Services;

public class Capturer
{
    public static ConcurrentQueue<PacketInfo> packets = new ConcurrentQueue<PacketInfo>();

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
    public static ConcurrentQueue<PacketInfo> StartCapture()
    {
        packets = new ConcurrentQueue<PacketInfo>();

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
            var endTime = DateTime.UtcNow + Analyzer.defaultWindow;
            while (DateTime.UtcNow < endTime)
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
        // 1) IP layer
        var ipPacket = parsedPacket.Extract<IPPacket>();
        if (ipPacket == null)
        {
            Console.WriteLine("Non-IP packet captured.");
            return;
        }

        // 2) Base info
        var info = new PacketInfo
        {
            IPVersion = ipPacket.Version,
            SourceIP = ipPacket.SourceAddress.ToString(),
            DestinationIP = ipPacket.DestinationAddress.ToString(),
            Timestamp = DateTime.Now,
            packet = parsedPacket,
            HeaderLength = parsedPacket.HeaderData.Length,
            TotalLength = parsedPacket.TotalPacketLength
        };

        // 3) Grab L7 bytes once
        var raw = PacketParser.GetApplicationLayerPayload(parsedPacket);

        // 4) Dispatch by L4
        if (parsedPacket.Extract<TcpPacket>() is TcpPacket tcp) HandleTcp(parsedPacket, tcp, raw, info);
        else if (parsedPacket.Extract<UdpPacket>() is UdpPacket udp) HandleUdp(parsedPacket, udp, raw, info);
        else if (parsedPacket.Extract<IcmpV4Packet>() is var icmp4) HandleIcmp4(info, icmp4);
        else if (parsedPacket.Extract<IcmpV6Packet>() is var icmp6) HandleIcmp6(info, icmp6);
        else if (parsedPacket.Extract<ArpPacket>() is var arp) HandleArp(info, arp);
        else
        {
            info.Protocol = "Other";
            Console.WriteLine($"{ipPacket.Version} non-TCP/UDP/ICMP/ARP packet: {info.SourceIP} -> {info.DestinationIP}");
            packets.Enqueue(info);
        }
    }

    // --- Protocol handlers ---

    /// <summary>
    /// Handles a TCP packet by setting the appropriate fields in the <see cref="PacketInfo"/> instance
    /// and decoding the application-layer payload.
    /// </summary>
    /// <param name="pkt">The parsed packet.</param>
    /// <param name="tcp">The TCP layer.</param>
    /// <param name="raw">The raw application-layer payload.</param>
    /// <param name="info">The <see cref="PacketInfo"/> instance to populate.</param>

    static void HandleTcp(Packet pkt, TcpPacket tcp, byte[]? raw, PacketInfo info)
    {
        info.SourcePort = tcp.SourcePort;
        info.DestinationPort = tcp.DestinationPort;
        info.Protocol = "TCP";
        info.Description = PacketInfo.Descriptions["TCP"];
        Console.WriteLine($"TCP: {info.SourceIP}:{info.SourcePort} → {info.DestinationIP}:{info.DestinationPort}");

        DecodeAndStoreAppLayer(raw, tcp.SourcePort, tcp.DestinationPort, info);

        packets.Enqueue(info);
    }

    /// <summary>
    /// Handles a UDP packet by setting the appropriate fields in the <see cref="PacketInfo"/> instance
    /// and decoding the application-layer payload.
    /// </summary>
    /// <param name="pkt">The parsed packet.</param>
    /// <param name="udp">The UDP layer.</param>
    /// <param name="raw">The raw application-layer payload.</param>
    /// <param name="info">The <see cref="PacketInfo"/> instance to populate.</param>
    static void HandleUdp(Packet pkt, UdpPacket udp, byte[]? raw, PacketInfo info)
    {
        info.SourcePort = udp.SourcePort;
        info.DestinationPort = udp.DestinationPort;
        info.Protocol = "UDP";
        info.Description = PacketInfo.Descriptions["UDP"];
        Console.WriteLine($"UDP: {info.SourceIP}:{info.SourcePort} → {info.DestinationIP}:{info.DestinationPort}");

        DecodeAndStoreAppLayer(raw, udp.SourcePort, udp.DestinationPort, info);

        packets.Enqueue(info);
    }

    /// <summary>
    /// Handles ICMPv4 packets by populating the protocol and description fields in the given PacketInfo 
    /// and enqueues the packet information for further processing.
    /// </summary>
    /// <param name="info">The PacketInfo object to populate with protocol details.</param>
    /// <param name="icmp4">The ICMPv4 packet to process.</param>

    static void HandleIcmp4(PacketInfo info, IcmpV4Packet icmp4)
    {
        if (icmp4 == null) return;
        info.Protocol = "ICMP";
        info.Description = PacketInfo.Descriptions["ICMP"];
        Console.WriteLine($"ICMPv4: {info.SourceIP} → {info.DestinationIP}");
        packets.Enqueue(info);
    }

    /// <summary>
    /// Handles ICMPv6 packets by setting the protocol and description fields and adding the packet to the queue.
    /// </summary>
    /// <param name="info">The packet info to populate.</param>
    /// <param name="icmp6">The ICMPv6 packet.</param>
    static void HandleIcmp6(PacketInfo info, IcmpV6Packet icmp6)
    {
        if (icmp6 == null) return;
        info.Protocol = "ICMPv6";
        info.Description = PacketInfo.Descriptions["ICMP"];
        Console.WriteLine($"ICMPv6: {info.SourceIP} → {info.DestinationIP}");
        packets.Enqueue(info);
    }

    /// <summary>
    /// Handles ARP packets by setting the protocol and description fields and adding the packet to the queue.
    /// </summary>
    /// <param name="info">The packet info to populate.</param>
    /// <param name="arp">The ARP packet.</param>
    static void HandleArp(PacketInfo info, ArpPacket arp)
    {
        if (arp == null) return;
        info.Protocol = "ARP";
        info.Description = PacketInfo.Descriptions["ARP"];
        Console.WriteLine($"ARP: {info.SourceIP} → {info.DestinationIP}");
        packets.Enqueue(info);
    }

    // --- Shared L7 decoder ---

    /// <summary>
    /// Decodes the raw application-layer payload and stores it in a PacketInfo instance.
    /// The decoder checks for JSON, TLS on 443, DNS on 53/5353, SSDP on 1900, and falls back to
    /// readable text or a binary marker.
    /// </summary>
    /// <param name="raw">The raw application-layer payload to decode.</param>
    /// <param name="srcPort">The source port of the packet.</param>
    /// <param name="dstPort">The destination port of the packet.</param>
    /// <param name="info">The PacketInfo instance to store the decoded application-layer text.</param>
    static void DecodeAndStoreAppLayer(byte[]? raw, int srcPort, int dstPort, PacketInfo info)
    {
        // A) TLS on 443: label it immediately, even if raw is empty/null
        if (srcPort == 443 || dstPort == 443)
        {
            info.ApplicationLayerText = "[TLS-encrypted payload]";
            Console.WriteLine(info.ApplicationLayerText);
            return;
        }

        // B) If there really is no payload, bail out
        if (raw == null || raw.Length == 0)
            return;

        // C) JSON?
        if (PacketParser.TryExtractJsonText(raw, out var json))
        {
            info.ApplicationLayerText = json;
            info.hasJsonAppLayerText = true;
            Console.WriteLine("JSON: " + json);
            return;
        }

        // D) DNS on 53/5353
        if (srcPort is 53 or 5353 || dstPort is 53 or 5353)
        {
            var dnsText = PacketParser.FormatMdnsMessage(raw);
            info.ApplicationLayerText = dnsText;
            Console.WriteLine(dnsText);
            return;
        }

        // E) SSDP on 1900
        if (srcPort == 1900 || dstPort == 1900)
        {
            var ssdp = Encoding.UTF8.GetString(raw).Trim();
            info.ApplicationLayerText = ssdp;
            Console.WriteLine("SSDP: " + ssdp.Replace("\r\n", " | "));
            return;
        }

        // F) Fallback: readable text or binary
        var latin = PacketParser.ExtractLatinText(raw);
        if (!string.IsNullOrEmpty(latin))
        {
            info.ApplicationLayerText = latin;
            Console.WriteLine("L7 Text: " + latin);
        }
        else
        {
            info.ApplicationLayerText = "[binary payload]";
            Console.WriteLine(info.ApplicationLayerText);
        }
    }


}

