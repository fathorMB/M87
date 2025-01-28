using M87.Contracts;
using M87.SimulatorCore.Interfaces;
using M87.SimulatorCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace M87.SimulatorCore.Engine
{
    public class MarketSessionManager
    {
        private readonly TimeProvider _timeProvider;
        private readonly List<Stock> _stocks;
        private readonly Interfaces.ILogger _logger;
        private readonly IEnumerable<ISimulationEventHandler> _eventHandlers;
        private readonly Dictionary<string, DataAggregator> _dataAggregators; // Chiave: timeframe

        private readonly List<TimeSpan> _timeframes = new List<TimeSpan>
        {
            TimeSpan.Zero, // 1 tick
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(60)
        };

        public MarketSessionManager(TimeProvider timeProvider, List<Stock> stocks, Interfaces.ILogger logger, IEnumerable<ISimulationEventHandler> eventHandlers)
        {
            _timeProvider = timeProvider;
            _stocks = stocks;
            _logger = logger;
            _eventHandlers = eventHandlers;

            _dataAggregators = new Dictionary<string, DataAggregator>();

            // Inizializzazione dei DataAggregators per ogni timeframe
            foreach (var timeframe in _timeframes)
            {
                string key = GetTimeframeKey(timeframe);
                _dataAggregators[key] = new DataAggregator(timeframe);
                _dataAggregators[key].OnCandleCompleted += (candle) => HandleCandleCompleted(candle, key);
            }

            _timeProvider.OnTick += OnTick;

            // Sottoscrizione agli eventi di matching per aggiornare i prezzi
            foreach (var stock in _stocks)
            {
                stock.OrderBook.OnOrderMatched += (bid, ask) =>
                {
                    // Aggiornare il prezzo corrente dello stock
                    stock.UpdatePrice(ask.Price);
                    // Aggiungere il prezzo al DataAggregator del timeframe "tick"
                    _dataAggregators["tick"].AddPrice(_timeProvider.CurrentTime, ask.Price);

                    // Creare l'oggetto PriceUpdate
                    var priceUpdate = new PriceUpdate
                    {
                        StockSymbol = stock.Symbol,
                        Price = ask.Price,
                        Timestamp = _timeProvider.CurrentTime
                    };

                    // Inviare l'aggiornamento agli handler
                    foreach (var handler in _eventHandlers)
                    {
                        handler.OnPriceUpdateAsync(priceUpdate);
                    }
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
            // Simulazione degli aggiornamenti dei prezzi
            foreach (var stock in _stocks)
            {
                double oldPrice = stock.CurrentPrice;
                double newPrice = stock.PriceSimulator.SimulateNextPrice(oldPrice, _timeProvider.DeltaTime.TotalSeconds);
                stock.UpdatePrice(newPrice);
                _logger.Log($"Prezzo {stock.Symbol} aggiornato da {oldPrice} a {newPrice}.");

                var priceUpdate = new PriceUpdate
                {
                    StockSymbol = stock.Symbol,
                    Price = newPrice,
                    Timestamp = _timeProvider.CurrentTime
                };

                foreach (var handler in _eventHandlers)
                {
                    handler.OnPriceUpdateAsync(priceUpdate).Wait();
                }

                // Aggiungere il prezzo a ciascun DataAggregator tranne "tick"
                foreach (var timeframe in _timeframes)
                {
                    if (timeframe == TimeSpan.Zero)
                        continue; // Saltare "tick", già gestito

                    string key = GetTimeframeKey(timeframe);
                    _dataAggregators[key].AddPrice(currentTime, newPrice);
                }
            }
        }

        private void HandleCandleCompleted(Candle candle, string timeframe)
        {
            _logger.Log($"Candela completata per timeframe {timeframe}: O={candle.Open}, H={candle.High}, L={candle.Low}, C={candle.Close}, T={candle.Time}");

            var candleUpdate = new CandleUpdate
            {
                StockSymbol = "AAPL", // Assumendo un solo stock; modificare se necessario
                Timeframe = timeframe,
                Candle = candle
            };

            foreach (var handler in _eventHandlers)
            {
                handler.OnCandleUpdateAsync(candleUpdate).Wait();
            }
        }

        private string GetTimeframeKey(TimeSpan timeframe)
        {
            if (timeframe == TimeSpan.Zero)
                return "tick";
            else if (timeframe == TimeSpan.FromMinutes(1))
                return "1m";
            else if (timeframe == TimeSpan.FromMinutes(5))
                return "5m";
            else if (timeframe == TimeSpan.FromMinutes(15))
                return "15m";
            else if (timeframe == TimeSpan.FromMinutes(30))
                return "30m";
            else if (timeframe == TimeSpan.FromMinutes(60))
                return "60m";
            else
                throw new ArgumentException("Unsupported timeframe");
        }
    }
}
