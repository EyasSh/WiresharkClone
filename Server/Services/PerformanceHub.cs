namespace Server.Services;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Newtonsoft.Json;
using Server.DB;
using Server.Models;

public interface IHubService
{
    Task ConnectNotification(string sid, string warningLevel);
    Task ReceiveMetrics(double cpuUsage, double ramUsage, double diskUsage);
    Task ReceivePackets(PacketInfo[] packets);
}
public class SocketService : Hub<IHubService>
{
    // shared across *all* hub instances
    private static long _globalCount = 0;

    MongoDBWrapper? _mongoDBWrapper;
    private readonly IConfiguration _conf;
    private readonly EmailService _emailService;
    IMongoCollection<Device>? _devicesCollection;
    private static readonly ConcurrentDictionary<string, string> _connections = new();
    private static ConcurrentQueue<Device> _devices = new();
    public SocketService(MongoDBWrapper mongoDBWrapper, IConfiguration conf, EmailService emailService)
    {
        _mongoDBWrapper = mongoDBWrapper;
        _devicesCollection = mongoDBWrapper.Devices;
        _conf = conf;
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
        _connections[sid] = Context?.User?.Identity?.Name ?? string.Empty;
        try
        {
            await Clients.Caller.ConnectNotification(Context?.ConnectionId ?? Guid.NewGuid().ToString(), "ok");
            var devices = await _devicesCollection.Find(_ => true).ToListAsync();
            if (devices != null && devices.Count > 0)
            {
                _globalCount = devices[0].Counter;
                foreach (var device in devices)
                {
                    _devices.Enqueue(device);
                }

            }

        }
        catch (Exception ex)
        {
            await Clients.Caller.ConnectNotification(ex?.Message ?? "An error occurred in sockets", "err");
        }


    }
    /// <summary>
    /// Retrieves system metrics such as CPU, RAM, and disk usage percentages.
    /// Sends the retrieved metrics to the calling client.
    /// <list type="">
    /// <item><term>cpuUsage</term><description>The CPU usage percentage Which is compatible with Windows and Linux.</description></item>
    /// <item><term>ramUsage</term><description>The RAM usage percentage Windows ONLY.</description></item>
    /// <item><term>diskUsage</term><description>The disk usage percentage Windows ONLY.</description>
    /// </item>
    /// </list>
    /// </summary>
    public async Task GetMetrics(string email, string name)
    {
        // atomically bump
        var invocationNumber = Interlocked.Increment(ref _globalCount);
        var (cpuUsage, ramUsage, diskUsage) = new MetricFetcher().GetSystemMetrics();
        var cpu = _devices.Where(device => device.Name == "CPU").FirstOrDefault();
        var ram = _devices.Where(device => device.Name == "RAM").FirstOrDefault();
        var disk = _devices.Where(device => device.Name == "Disk").FirstOrDefault();

        if (invocationNumber == 1)
        {


            Console.WriteLine(invocationNumber);
            if (cpu != null && ram != null && disk != null)
            {
                cpu.AverageUsage = cpuUsage;
                ram.AverageUsage = ramUsage;
                disk.AverageUsage = diskUsage;

                cpu.Counter++;
                ram.Counter++;
                disk.Counter++;

                cpu.Sum += cpuUsage;
                ram.Sum += ramUsage;
                disk.Sum += diskUsage;


            }
            else
            {
                System.Console.WriteLine("One or more devices are null, cannot update metrics.");
                await Clients.Caller.ReceiveMetrics(cpuUsage, ramUsage, diskUsage);
                System.Console.WriteLine($"isCpuNull: {cpu == null}, isRamNull: {ram == null}, isDiskNull: {disk == null}");
                return;
            }

        }
        else
        {
            if (cpu == null || ram == null || disk == null)
            {
                System.Console.WriteLine("One or more devices are null, cannot update metrics.");
                await Clients.Caller.ReceiveMetrics(cpuUsage, ramUsage, diskUsage);
                System.Console.WriteLine($"isCpuNull: {cpu == null}, isRamNull: {ram == null}, isDiskNull: {disk == null}");
                return;
            }
            cpu.Sum += cpuUsage;
            ram.Sum += ramUsage;
            disk.Sum += diskUsage;

            cpu.Counter++;
            ram.Counter++;
            disk.Counter++;

            User u = new User { Name = name, Email = email, Password = "dummy", date = DateOnly.FromDateTime(DateTime.Now) };
            if ((cpuUsage > cpu.AverageUsage && ramUsage > ram.AverageUsage) || diskUsage > disk.AverageUsage)
            {
                System.Console.WriteLine("Usage is above average, sending email report.");
                var pdf = PdfGenerator.GeneratePerformancePdf((cpuUsage, ramUsage, diskUsage), u, cpu.AverageUsage, ram.AverageUsage, disk.AverageUsage);
                await _emailService.SendEmailWithAttachmentAsync(email, $"{name}, Usage Report for {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}",
               $@"
                <html>
                <body>
                Dear {name},
                <p>This is a Performance Report for {DateOnly.FromDateTime(DateTime.Now)}.</p>
                <p>We usually send these requests when usage is above average. </p>
                <p>Thank you for using our service.</p>
                <p>Best regards,</p>
                <p>The Wire Tracer Team</p>
                </body>
                </html>
            ", pdf, Guid.NewGuid().ToString() + "" + DateOnly.FromDateTime(DateTime.Now) + "" + ".pdf");
            }
            cpu.AverageUsage = cpu.Sum / cpu.Counter;
            ram.AverageUsage = ram.Sum / ram.Counter;
            disk.AverageUsage = disk.Sum / disk.Counter;

        }


        await Clients.Caller.ReceiveMetrics(cpuUsage, ramUsage, diskUsage);
    }


    /// <summary>
    /// This is the OnDisconnectedAsync method that will be called when the client disconnects from the server.
    /// It will remove the connection from the list of connections and write a message to the console.
    /// If there is an error, it will write the exception message to the console.
    /// </summary>
    /// <param name="exception">Optional exception that may be related to the disconnection.</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var sid = Context.ConnectionId;
        if (_connections.ContainsKey(sid))
        {
            _connections.TryRemove(sid, out _);
        }

        System.Console.WriteLine($"Disconnected: {sid}");
        foreach (var device in _devices)
        {
            if (device.Counter == 0)
            {
                device.AverageUsage = 0;
            }
            else
            {
                device.AverageUsage = device.Sum / device.Counter;
            }
            await _devicesCollection.ReplaceOneAsync(d => d.Id == device.Id, device);
        }
        // Perform any additional logic here if needed
        await base.OnDisconnectedAsync(exception);
    }

}