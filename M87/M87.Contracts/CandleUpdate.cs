using System;

namespace M87.Contracts
{
    public class CandleUpdate
    {
        public string StockSymbol { get; set; }
        public string Timeframe { get; set; } // Esempi: "tick", "1m", "5m", "15m", "30m", "60m"
        public Candle Candle { get; set; }
    }
}
