using M87.Contracts;
using M87.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace M87.WebAPI
{
    public class SimulationEventHandler : ISimulationEventHandler
    {
        private readonly IHubContext<PriceHub> _hubContext;

        public SimulationEventHandler(IHubContext<PriceHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task OnPriceUpdateAsync(PriceUpdate priceUpdate)
        {
            // Invia l'aggiornamento tramite SignalR a tutti i client connessi
            await _hubContext.Clients.All.SendAsync("ReceivePriceUpdate", priceUpdate.StockSymbol, priceUpdate.Price, priceUpdate.Timestamp);
        }
    }
}
