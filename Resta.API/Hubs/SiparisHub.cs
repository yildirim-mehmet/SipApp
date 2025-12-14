using Microsoft.AspNetCore.SignalR;

namespace Resta.API.Hubs
{
    public class SiparisHub : Hub
    {
        // ====================================================
        // EKRAN BAĞLANTISI
        // ====================================================
        // Örn:
        // - ekranId = 1 (Mutfak)
        // - ekranId = 2 (Bar)
        //
        // Client bağlanırken çağırır
        // ====================================================
        public async Task JoinEkranGroup(string ekranId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"EKRAN_{ekranId}");
        }

        // ====================================================
        // MASA BAĞLANTISI
        // ====================================================
        // Aynı masadaki birden fazla müşteri için
        // ====================================================
        public async Task JoinMasaGroup(string masaId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"MASA_{masaId}");
        }

        public async Task LeaveMasaGroup(string masaId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"MASA_{masaId}");
        }
    }
}
