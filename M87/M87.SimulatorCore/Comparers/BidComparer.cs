using M87.SimulatorCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Comparers
{
    public class BidComparer : IComparer<Order>
    {
        public int Compare(Order x, Order y)
        {
            int priceComparison = y.Price.CompareTo(x.Price); // Decrescente per i bid
            if (priceComparison == 0)
            {
                return x.Timestamp.CompareTo(y.Timestamp); // FIFO
            }
            return priceComparison;
        }
    }
}
