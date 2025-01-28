using M87.SimulatorCore.Engine;
using M87.SimulatorCore.Models;
using M87.SimulatorCore.Simulation;

namespace M87.QuickSim
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Configurazione iniziale
            var config = new ConfigurationManager
            {
                StockSymbols = new List<string> { "AAPL", "GOOGL", "MSFT" },
                Drift = 0.0001,
                Volatility = 0.01,
                CandleInterval = TimeSpan.FromMinutes(1),
                TickInterval = TimeSpan.FromSeconds(1),
                DeltaTime = TimeSpan.FromMinutes(1)
            };

            // Inizializzare il logger
            var logger = new Logger();

            // Inizializzare i prezzi iniziali
            var initialPrices = new Dictionary<string, double>
            {
                { "AAPL", 150.0 },
                { "GOOGL", 2800.0 },
                { "MSFT", 300.0 }
            };

            // Creare i simulatori di prezzo e le azioni
            var stocks = new List<Stock>();
            foreach (var symbol in config.StockSymbols)
            {
                // Se desideri usare OrderBookPriceSimulator, passare l'order book
                // var orderBook = new OrderBook(symbol);
                // var priceSimulator = new OrderBookPriceSimulator(config.Drift, config.Volatility, orderBook);
                // var stock = new Stock(symbol, initialPrices[symbol], priceSimulator, logger);

                // Per semplicità, utilizziamo GeometricBrownianMotionSimulator
                var priceSimulator = new GeometricBrownianMotionSimulator(config.Drift, config.Volatility);
                var stock = new Stock(symbol, initialPrices[symbol], priceSimulator, logger);
                stocks.Add(stock);
            }

            // Creare il TimeProvider
            var startTime = DateTime.UtcNow;
            var timeProvider = new SimulatorCore.Engine.TimeProvider(startTime, config.TickInterval, config.DeltaTime);

            // Creare il MarketSessionManager
            var sessionManager = new MarketSessionManager(timeProvider, stocks, config.CandleInterval, logger);

            // Avviare la sessione di mercato
            sessionManager.StartSession();

            // Simulare l'inserimento di alcuni ordini
            // Questo è solo un esempio; in un'applicazione reale, gli ordini potrebbero provenire da utenti o da altre fonti
            var order1 = new Order
            {
                StockSymbol = "AAPL",
                Type = OrderType.Limit,
                Side = OrderSide.Buy,
                Price = 149.5,
                Quantity = 100,
                ClientId = "Client1"
            };

            var order2 = new Order
            {
                StockSymbol = "AAPL",
                Type = OrderType.Limit,
                Side = OrderSide.Sell,
                Price = 150.5,
                Quantity = 100,
                ClientId = "Client2"
            };

            // Inviare gli ordini
            foreach (var stock in stocks)
            {
                if (stock.Symbol == order1.StockSymbol)
                {
                    stock.OrderManager.SubmitOrder(order1);
                    stock.OrderManager.SubmitOrder(order2);
                }
            }

            // Eseguire la simulazione per un certo periodo
            Console.WriteLine("Simulazione in corso... Premi Invio per terminare.");
            Console.ReadLine();

            // Fermare la sessione di mercato
            sessionManager.StopSession();
        }
    }
}
