using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Server.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] // Ensures correct ObjectId serialization
        public string? Id { get; set; }
        [BsonElement("name")]
        public required string Name { get; set; }
        [BsonElement("email")]
        public required string Email { get; set; }
        [BsonElement("password")]
        public required string Password { get; set; }
    }

}