using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Models
{
    public enum OrderSide
    {
        Buy,
        Sell
    }

    public enum OrderType
    {
        Market,
        Limit,
        Stop,
        StopLimit
    }

    public class Order
    {
        public Guid OrderId { get; set; } = Guid.NewGuid();
        public string StockSymbol { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public double Price { get; set; } // Rilevante per limit e stop orders
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ClientId { get; set; }
    }
}
