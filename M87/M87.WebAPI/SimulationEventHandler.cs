using M87.Contracts;
using M87.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class SimulationEventHandler : ISimulationEventHandler
{
    private readonly IHubContext<PriceHub> _hubContext;
    private readonly ILogger<SimulationEventHandler> _logger;

    public SimulationEventHandler(IHubContext<PriceHub> hubContext, ILogger<SimulationEventHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task OnPriceUpdateAsync(PriceUpdate priceUpdate)
    {
        _logger.LogInformation($"Invio aggiornamento prezzo: {priceUpdate.StockSymbol} - {priceUpdate.Price} - {priceUpdate.Timestamp}");
        await _hubContext.Clients.All.SendAsync("ReceivePriceUpdate", priceUpdate.StockSymbol, priceUpdate.Price, priceUpdate.Timestamp);
    }

    public async Task OnCandleUpdateAsync(CandleUpdate candleUpdate)
    {
        _logger.LogInformation($"Invio aggiornamento candela: {candleUpdate.StockSymbol} - {candleUpdate.Timeframe} - {candleUpdate.Candle.Time}");
        await _hubContext.Clients.All.SendAsync("ReceiveCandleUpdate", candleUpdate.StockSymbol, candleUpdate.Timeframe, candleUpdate.Candle);
    }
}
