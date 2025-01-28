namespace M87.Contracts
{
    public class PriceUpdate
    {
        public string StockSymbol { get; set; }
        public double Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
