using M87.SimulatorCore.Engine;
using M87.SimulatorCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Models
{
    public class Stock
    {
        public string Symbol { get; private set; }
        public double CurrentPrice { get; private set; }
        public IPriceSimulator PriceSimulator { get; private set; }
        public OrderBook OrderBook { get; private set; }
        public OrderManager OrderManager { get; set; }

        public Stock(string symbol, double initialPrice, IPriceSimulator priceSimulator, ILogger logger)
        {
            Symbol = symbol;
            CurrentPrice = initialPrice;
            PriceSimulator = priceSimulator;
            OrderBook = new OrderBook(symbol);
            OrderManager = new OrderManager(OrderBook, logger);
        }

        public void UpdatePrice(double newPrice)
        {
            CurrentPrice = newPrice;
            // Potrebbe includere logica aggiuntiva, come notifiche agli observer
        }
    }
}
