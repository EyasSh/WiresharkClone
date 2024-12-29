namespace Server.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public interface IHubService
{
    Task ConnectNotification(string sid, string warningLevel);
}
public class SocketService : Hub<IHubService>
{
    private static readonly Dictionary<string, string> _connections = new();
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
            await Clients.Caller.ConnectNotification(ex?.Message ?? "An error occurred in sockets", "error");
        }


    }
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