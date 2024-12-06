using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Server.Models
{
    public class Packet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("data")]
        public required string Data { get; set; }
        [BsonElement("timestamp")]
        public required DateTime Timestamp { get; set; }
    }
}
