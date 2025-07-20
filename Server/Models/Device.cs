using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Server.Models;
/// <summary>
/// This class represents a hardware device model with properties for its unique identifier, name, and average usage.
/// </summary>

public class Device
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("averageUsage")]
    public double AverageUsage { get; set; } = 0.0;
    [BsonElement("counter")]
    public int Counter { get; set; } = 0;
    [BsonElement("sum")]
    public double Sum { get; set; } = 0.0;
    [BsonElement("sumOfDeviations")]
    public double SumOfDeviations { get; set; } = 0.0;
    [BsonElement("numOfDeviations")]
    public int NumOfDeviations { get; set; } = 0;
}
