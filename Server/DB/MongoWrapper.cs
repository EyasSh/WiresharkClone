using MongoDB.Bson;
using MongoDB;
using MongoDB.Driver;
using Server.Models;
namespace Server.DB;


public class MongoDBWrapper
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    public IMongoCollection<User> Users { get; init; }
    public IMongoCollection<PacketInfo> Packets { get; init; }
    public IMongoCollection<VirusCheckerHistory> VirusCheckerHistory { get; init; }
    public IMongoCollection<Device> Devices { get; init; }

    public MongoDBWrapper(IConfiguration configuration)
    {
        // Fetch values directly using GetValue<T>
        var connectionString = configuration.GetValue<string>("DB:ConnectionString");
        var databaseName = configuration.GetValue<string>("DB:DatabaseName");

        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("MongoDB configuration is missing or invalid.");
        }

        // Initialize MongoDB client and database
        _client = new MongoClient(connectionString);
        _database = _client.GetDatabase(databaseName);

        // Initialize collections
        Users = _database.GetCollection<User>("Users");
        Packets = _database.GetCollection<PacketInfo>("Packets");
        VirusCheckerHistory = _database.GetCollection<VirusCheckerHistory>("VirusCheckerHistory");
        Devices = _database.GetCollection<Device>("Devices");
    }


    public IMongoDatabase GetDatabase() => _database;
}

// Example POCO classes for MongoDB documents


