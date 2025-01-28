using M87.SimulatorCore.Comparers;
using M87.SimulatorCore.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Engine
{
    public class OrderBook
    {
        public string StockSymbol { get; private set; }

        // Utilizzo di SortedSet per mantenere gli ordini ordinati
        public SortedSet<Order> Bids { get; private set; }
        public SortedSet<Order> Asks { get; private set; }

        public event Action<Order, Order> OnOrderMatched;

        public OrderBook(string stockSymbol)
        {
            StockSymbol = stockSymbol;
            Bids = new SortedSet<Order>(new BidComparer());
            Asks = new SortedSet<Order>(new AskComparer());
        }

        public void AddOrder(Order order)
        {
            if (order.Side == OrderSide.Buy)
            {
                Bids.Add(order);
            }
            else
            {
                Asks.Add(order);
            }

            // Trigger matching
            var matchedOrders = MatchingEngine.MatchOrders(this);

            foreach (var match in matchedOrders)
            {
                OnOrderMatched?.Invoke(match.Bid, match.Ask);
            }
        }

        public void RemoveOrder(Order order)
        {
            if (order.Side == OrderSide.Buy)
            {
                Bids.Remove(order);
            }
            else
            {
                Asks.Remove(order);
            }
        }

        // Altri metodi come ModifyOrder, etc., possono essere aggiunti qui
    }
}
