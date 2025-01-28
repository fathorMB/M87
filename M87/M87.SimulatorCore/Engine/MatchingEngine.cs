using M87.SimulatorCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Engine
{
    public static class MatchingEngine
    {
        public static List<(Order Bid, Order Ask)> MatchOrders(OrderBook orderBook)
        {
            var matchedOrders = new List<(Order, Order)>();

            while (orderBook.Bids.Any() && orderBook.Asks.Any())
            {
                var highestBid = orderBook.Bids.First();
                var lowestAsk = orderBook.Asks.First();

                if (highestBid.Price >= lowestAsk.Price)
                {
                    // Determinare la quantità da scambiare
                    int tradeQuantity = Math.Min(highestBid.Quantity, lowestAsk.Quantity);
                    double tradePrice = lowestAsk.Price; // Prezzo di esecuzione (in genere il prezzo dell'ask)

                    matchedOrders.Add((highestBid, lowestAsk));

                    // Aggiornare le quantità
                    highestBid.Quantity -= tradeQuantity;
                    lowestAsk.Quantity -= tradeQuantity;

                    // Rimuovere gli ordini completamente eseguiti
                    if (highestBid.Quantity == 0)
                    {
                        orderBook.Bids.Remove(highestBid);
                    }
                    if (lowestAsk.Quantity == 0)
                    {
                        orderBook.Asks.Remove(lowestAsk);
                    }

                    // Potrebbe essere necessario aggiornare il prezzo corrente dello stock
                    // ad esempio, notificare il MarketSessionManager per aggiornare il prezzo
                }
                else
                {
                    break; // Non ci sono ulteriori abbinamenti possibili
                }
            }

            return matchedOrders;
        }
    }
}
