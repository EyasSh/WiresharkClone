namespace Server.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Server.Models;

public interface IHubService
{
    Task ConnectNotification(string sid, string warningLevel);
    Task ReceiveMetrics(double cpuUsage, double ramUsage, double diskUsage);
    Task ReceivePackets(PacketInfo[] packets);
}
public class SocketService : Hub<IHubService>
{
    private static readonly Dictionary<string, string> _connections = new();
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


    public void GetMetrics()
    {
        var (cpuUsage, ramUsage, diskUsage) = new MetricFetcher().GetSystemMetrics();



        Clients.Caller.ReceiveMetrics(cpuUsage, ramUsage, diskUsage);
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
            _connections.Remove(sid);
        }

        System.Console.WriteLine($"Disconnected: {sid}");
        // Perform any additional logic here if needed
        await base.OnDisconnectedAsync(exception);
    }

}