using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PacketDotNet;
namespace Server.Models
{
    /// <summary>
    /// Represents a packet with its metadata and application-layer data.
    /// </summary>
    public class PacketInfo
    {
        public static Dictionary<string, string> Descriptions = new Dictionary<string, string>
        {
           { "TCP", "Transmission Control Protocol is Used inTCP flood attacks more popularly known as DDoS attacks. TCP flood attacks are a type of denial-of-service attack that targets the TCP protocol. The attacker sends a large number of TCP packets to a target system, overwhelming it and causing it to become unresponsive." },
           { "UDP", "User Datagram Protocol is used in UDP flood attacks. UDP flood attacks are a type of denial-of-service attack that targets the User Datagram Protocol (UDP). The attacker sends a large number of UDP packets to a target system, overwhelming it and causing it to become unresponsive." },
           { "ICMP", "Internet Control Message Protocol is used in Ping of Death attacks in which the attacker sends a packet who's size is bigger than the normal size of an ICP packet over 65,535 bytes." },
            { "ARP", "Address Resolution Protocol is used in ARP spoofing attacks. ARP spoofing attacks are a type of attack that targets the Address Resolution Protocol (ARP). The attacker sends false ARP messages to a target system, causing it to associate the attacker's MAC address with the IP address of a legitimate system." },
        };
        public IPVersion IPVersion { get; set; }
        [JsonIgnore]
        [BsonIgnore]
        public Packet? packet { get; set; }
        public string? SourceIP { get; set; }
        public string? DestinationIP { get; set; }
        public int? SourcePort { get; set; }
        public int? DestinationPort { get; set; }
        public string? Protocol { get; set; }
        public DateTime Timestamp { get; set; }
        public bool isSuspicious { get; set; } = false;
        public bool isMalicious { get; set; } = false;
        public int HeaderLength { get; set; }
        public int TotalLength { get; set; }
        public string? Description { get; set; }
        /// <summary>
        /// Raw application-layer payload bytes (TCP or UDP).
        /// </summary>
        [JsonIgnore]
        [BsonIgnore]
        public byte[]? ApplicationLayerPayload { get; set; }

        /// <summary>
        /// Printable/text form of the application-layer data (e.g. ASCII, UTF-8).
        /// </summary>
        public string? ApplicationLayerText { get; set; }
        public bool hasJsonAppLayerText { get; set; } = false;
    }

}
