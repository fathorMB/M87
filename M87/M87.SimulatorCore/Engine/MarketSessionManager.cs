using M87.SimulatorCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Engine
{
    public class MarketSessionManager
    {
        private TimeProvider _timeProvider;
        private List<Stock> _stocks;
        private DataAggregator _dataAggregator;
        private Logger _logger;

        public MarketSessionManager(TimeProvider timeProvider, List<Stock> stocks, TimeSpan candleInterval, Logger logger)
        {
            _timeProvider = timeProvider;
            _stocks = stocks;
            _dataAggregator = new DataAggregator(candleInterval);
            _logger = logger;

            _timeProvider.OnTick += OnTick;

            // Sottoscrizione agli eventi di candela completata
            _dataAggregator.OnCandleCompleted += HandleCandleCompleted;

            // Sottoscrizione agli eventi di matching per aggiornare i prezzi
            foreach (var stock in _stocks)
            {
                stock.OrderBook.OnOrderMatched += (bid, ask) =>
                {
                    // Aggiornare il prezzo corrente dello stock
                    stock.UpdatePrice(ask.Price);
                    _dataAggregator.AddPrice(_timeProvider.CurrentTime, ask.Price); // Assicurati di avere il timestamp corretto
                };
            }
        }

        public void StartSession()
        {
            _timeProvider.Start();
            _logger.Log("Sessione di mercato avviata.");
        }

        public void StopSession()
        {
            _timeProvider.Stop();
            _logger.Log("Sessione di mercato terminata.");
        }

        private void OnTick(DateTime currentTime)
        {
            foreach (var stock in _stocks)
            {
                double newPrice = stock.PriceSimulator.SimulateNextPrice(stock.CurrentPrice, _timeProvider.DeltaTime.TotalDays);
                stock.UpdatePrice(newPrice);
                _dataAggregator.AddPrice(currentTime, newPrice);
            }
        }

        private void HandleCandleCompleted(Candle candle)
        {
            // Gestisci la candela completata, ad esempio, salvarla o inviarla a un grafico
            _logger.Log($"Candela completata per {candle.Timestamp}: O={candle.Open}, H={candle.High}, L={candle.Low}, C={candle.Close}");
        }
    }
}
