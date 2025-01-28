using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.Contracts
{
    public class Candle
    {
        public long Time { get; set; } // Unix timestamp in secondi
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }

        public Candle(DateTime dateTime, double open, double high, double low, double close)
        {
            // Converti DateTime in Unix timestamp in secondi
            Time = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
            Open = open;
            High = high;
            Low = low;
            Close = close;
        }
    }
}
