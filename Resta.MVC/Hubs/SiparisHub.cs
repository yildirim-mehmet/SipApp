using Microsoft.AspNetCore.SignalR;

namespace Resta.MVC.Hubs
{
    public class SiparisHub : Hub
    {
        public async Task JoinMasa(string masaId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"MASA_{masaId}");
        }
    }
}
