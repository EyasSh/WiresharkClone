using Microsoft.AspNetCore.SignalR;

namespace Server.Services
{
    public interface IChatHubService
    {
        Task ReceiveMessage(string user, string message);
    }
    public class ChatHub : Hub<IChatHubService>
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message);
        }
    }
}