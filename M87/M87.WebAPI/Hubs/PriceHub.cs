// File: M87/M87.WebAPI/Hubs/PriceHub.cs

using M87.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace M87.WebAPI.Hubs
{
    public class PriceHub : Hub
    {
        // Metodo per inviare aggiornamenti di prezzo a tutti i client connessi
        public async Task SendPriceUpdate(string stockSymbol, double price, DateTime timestamp)
        {
            Console.WriteLine($"Sending Price Update: {stockSymbol} - {price} - {timestamp}");
            await Clients.All.SendAsync("ReceivePriceUpdate", stockSymbol, price, timestamp);
        }

        // Metodo per inviare aggiornamenti di candela a tutti i client connessi
        public async Task SendCandleUpdate(string stockSymbol, string timeframe, object candle)
        {
            Console.WriteLine($"Sending Candle Update: {stockSymbol} - {timeframe} - {candle}");
            await Clients.All.SendAsync("ReceiveCandleUpdate", stockSymbol, timeframe, candle);
        }
    }
}
