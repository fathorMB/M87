using M87.SimulatorCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Interfaces
{
    public interface IOrderBook
    {
        string StockSymbol { get; }
        SortedSet<Order> Bids { get; }
        SortedSet<Order> Asks { get; }

        event Action<Order, Order> OnOrderMatched;

        void AddOrder(Order order);
        void RemoveOrder(Order order);
    }
}
