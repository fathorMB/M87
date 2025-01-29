// File: M87/M87.SimulatorCore/Engine/MarketSessionManager.cs

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
        private readonly Dictionary<string, DataAggregator> _dataAggregators;

        private readonly List<TimeSpan> _timeframes = new List<TimeSpan>
        {
            // Removed the "1s" timeframe to prevent excessive CandleUpdates
            // Only include fixed timeframes like "1m", "5m", etc.
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

            foreach (var timeframe in _timeframes)
            {
                string key = GetTimeframeKey(timeframe);
                _dataAggregators[key] = new DataAggregator(timeframe);
                _dataAggregators[key].OnCandleCompleted += (candle) => HandleCandleCompleted(candle, key);
            }

            _timeProvider.OnTick += OnTick;

            foreach (var stock in _stocks)
            {
                stock.OrderBook.OnOrderMatched += (bid, ask) =>
                {
                    stock.UpdatePrice(ask.Price);
                    foreach (var timeframe in _timeframes)
                    {
                        string key = GetTimeframeKey(timeframe);
                        _dataAggregators[key].AddPrice(_timeProvider.CurrentTime, ask.Price);
                    }

                    var priceUpdate = new PriceUpdate
                    {
                        StockSymbol = stock.Symbol,
                        Price = ask.Price,
                        Timestamp = _timeProvider.CurrentTime
                    };

                    foreach (var handler in _eventHandlers)
                    {
                        handler.OnPriceUpdateAsync(priceUpdate);
                    }
                };
            }
        }

        public void StartSession() => _timeProvider.Start();
        public void StopSession() => _timeProvider.Stop();

        private void OnTick(DateTime currentTime)
        {
            foreach (var stock in _stocks)
            {
                double newPrice = stock.PriceSimulator.SimulateNextPrice(stock.CurrentPrice, _timeProvider.DeltaTime.TotalSeconds);
                stock.UpdatePrice(newPrice);

                _logger.Log($"Prezzo {stock.Symbol} aggiornato a {newPrice}.");

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

                foreach (var timeframe in _timeframes)
                {
                    string key = GetTimeframeKey(timeframe);
                    _dataAggregators[key].AddPrice(currentTime, newPrice);
                }
            }
        }

        private void HandleCandleCompleted(Candle candle, string timeframe)
        {
            _logger.Log($"Candela completata {timeframe}: O={candle.Open}, H={candle.High}, L={candle.Low}, C={candle.Close}, T={candle.Time}");

            var candleUpdate = new CandleUpdate
            {
                StockSymbol = "AAPL", // Consider making this dynamic if handling multiple stocks
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
            if (timeframe == TimeSpan.FromSeconds(1))
                return "1s";
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
