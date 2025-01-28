using Microsoft.AspNetCore.SignalR;

namespace M87.WebAPI.Hubs
{
    public class PriceHub : Hub
    {
        // Metodo per inviare aggiornamenti di prezzo a tutti i client connessi
        public async Task SendPriceUpdate(string stockSymbol, double price, DateTime timestamp)
        {
            await Clients.All.SendAsync("ReceivePriceUpdate", stockSymbol, price, timestamp);
        }
    }
}
