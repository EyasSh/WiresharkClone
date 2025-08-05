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
    private static readonly TimeSpan EmailInterval = TimeSpan.FromMinutes(30);
    private readonly EmailService _emailService;
    private static readonly ConcurrentDictionary<string, DateTime> _lastEmailSent = new();
    public PacketHub(MongoDBWrapper mongoDBWrapper, EmailService emailService)
    {
        _mongoDBWrapper = mongoDBWrapper;
        _packetsCollection = mongoDBWrapper.Packets;
        _emailService = emailService;
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
    public async Task GetPackets(string email)
    {
        System.Console.WriteLine($"Email: {email}");
        // 1. Capture & analyze
        var packets = Capturer.StartCapture();
        Analyzer.DetectSynFlood(packets, 150, Analyzer.defaultQuarterWindow);
        Analyzer.DetectUdpFlood(packets, 150, Analyzer.defaultQuarterWindow);
        Analyzer.DetectPortScan(packets, 5, Analyzer.defaultQuarterWindow);
        Analyzer.DetectPingOfDeathV4(packets);
        Analyzer.DetectPingOfDeathV6(packets);

        var suspackets = packets
            .Where(p => p.isSuspicious || p.isMalicious)
            .ToList();

        // 2. Persist
        if (suspackets.Count > 0)
        {
            _packetsCollection?.InsertMany(suspackets);
            Console.WriteLine($"Inserted {suspackets.Count} suspicious packets into the database");
        }
        else
        {
            Console.WriteLine("No suspicious packets found");
        }

        // 3. Send packets to client immediately
        Console.WriteLine($"Sending {packets.Count} packets to client {Context.ConnectionId}");
        await Clients.Caller.ReceivePackets(packets.ToArray());

        // 4. Now decide if we should email
        if (suspackets.Count > 0)
        {
            var lastSentExists = _lastEmailSent.TryGetValue(email, out var lastSent);
            var shouldSend = !lastSentExists || (DateTime.UtcNow - lastSent >= EmailInterval);

            if (shouldSend)
            {
                var pdfBytes = PdfGenerator.GenerateFlaggedPacketsPdf(suspackets);
                if (pdfBytes != null)
                {
                    // fire-and-forget:
                    _ = _emailService.SendEmailWithAttachmentAsync(
                            email,
                            "Suspicious Packets Detected",
                            "Please find attached the PDF report of suspicious packets detected.",
                            pdfBytes,
                            "suspicious_packets.pdf"
                        )
                        .ContinueWith(t =>
                        {
                            if (t.IsCompletedSuccessfully)
                            {
                                _lastEmailSent[email] = DateTime.UtcNow;
                                Console.WriteLine($"Sent email notification to {email}.");
                            }
                            else
                            {
                                Console.Error.WriteLine($"Email failed: {t.Exception}");
                            }
                        });


                }
            }
        }
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        System.Console.WriteLine($"{Context.ConnectionId} disconnected from packet hub");
        await base.OnDisconnectedAsync(exception);
    }
}