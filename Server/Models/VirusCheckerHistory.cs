using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace Server.Models;
/// <summary>
/// This class represents a history record for virus checks performed on files.
/// It includes properties for the unique identifier, file name, date of the check, and the result of the check.
/// </summary>
public class VirusCheckerHistory
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [BsonElement("result")]
    public string Result { get; set; } = string.Empty;
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;
}