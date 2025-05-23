using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Newtonsoft.Json;
using Server.DB;
using Server.Models;
namespace Server.Services;

public interface IPacketContext
{
    Task ConnectNotification(string sid, string warningLevel);
    Task ReceivePackets(PacketInfo[] packets);
}
public class PacketHub : Hub<IPacketContext>
{
    private static readonly ConcurrentDictionary<string, string> _connections = new();
    private static IMongoCollection<PacketInfo>? _packetsCollection;
    private static MongoDBWrapper? _mongoDBWrapper;
    public PacketHub(MongoDBWrapper mongoDBWrapper)
    {
        _mongoDBWrapper = mongoDBWrapper;
        _packetsCollection = mongoDBWrapper.Packets;
    }
    /// <summary>
    /// This is the OnConnectedAsync method that will be called when the client establishes a connection to the server.
    /// It will add the connection to the list of connections and send a message to the client with the connection ID.
    /// If there is an error, it will send an error message to the client.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var sid = Context.ConnectionId;
        System.Console.WriteLine($"{Context.ConnectionId} connected to packet hub");
        _connections[sid] = Context?.User?.Identity?.Name ?? string.Empty;
        try
        {
            await Clients.Caller.ConnectNotification(Context?.ConnectionId ?? Guid.NewGuid().ToString(), "ok");
        }
        catch (Exception ex)
        {
            await Clients.Caller.ConnectNotification(ex?.Message ?? "An error occurred in sockets", "err");
        }

    }
    /// <summary>
    /// Retrieves all packets captured by the server.
    /// The packets are captured using the SharpPcap library.
    /// Sends the captured packets to the calling client.
    /// </summary>
    public void GetPackets()
    {
        Queue<PacketInfo> packets = Capturer.StartCapture();
        Analyzer.DetectSynFlood(packets, 150, Analyzer.defaultQuarterWindow);
        Analyzer.DetectUdpFlood(packets, 150, Analyzer.defaultQuarterWindow);
        Analyzer.DetectPortScan(packets, 5, Analyzer.defaultQuarterWindow);
        Analyzer.DetectPingOfDeathV4(packets);
        Analyzer.DetectPingOfDeathV6(packets);
        var suspackets = packets.Where(p => p.isSuspicious == true || p.isMalicious == true).ToList();
        if (suspackets.Count > 0)
        {
            _packetsCollection?.InsertMany(suspackets);
            System.Console.WriteLine($"Inserted {suspackets.Count} suspicious packets into the database");
        }
        else
        {
            System.Console.WriteLine($"No suspicious packets found");
        }

        Clients.Caller.ReceivePackets(packets.ToArray());
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        System.Console.WriteLine($"{Context.ConnectionId} disconnected from packet hub");
        await base.OnDisconnectedAsync(exception);
    }
}