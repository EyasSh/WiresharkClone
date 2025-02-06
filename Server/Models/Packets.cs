using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PacketDotNet;
namespace Server.Models
{
    public class PacketInfo
    {
        public IPVersion IPVersion { get; set; }
        public string? SourceIP { get; set; }
        public string? DestinationIP { get; set; }
        public int? SourcePort { get; set; }
        public int? DestinationPort { get; set; }
        public string? Protocol { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
