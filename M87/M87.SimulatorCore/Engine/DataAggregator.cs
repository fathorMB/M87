// File: M87/M87.SimulatorCore/Engine/DataAggregator.cs

using M87.Contracts;
using System;
using System.Collections.Generic;

namespace M87.SimulatorCore.Engine
{
    public class DataAggregator
    {
        private readonly TimeSpan _candleInterval;
        private Candle _currentCandle;
        private DateTime _currentCandleStart;
        private readonly object _lock = new object();

        public event Action<Candle> OnCandleCompleted;

        public DataAggregator(TimeSpan candleInterval)
        {
            _candleInterval = candleInterval;
        }

        public void AddPrice(DateTime timestamp, double price)
        {
            lock (_lock)
            {
                if (_currentCandle == null)
                {
                    _currentCandleStart = AlignToInterval(timestamp, _candleInterval);
                    _currentCandle = new Candle(_currentCandleStart, price, price, price, price);
                    return;
                }

                if (timestamp < _currentCandleStart + _candleInterval)
                {
                    _currentCandle.Close = price;
                    if (price > _currentCandle.High) _currentCandle.High = price;
                    if (price < _currentCandle.Low) _currentCandle.Low = price;
                }
                else
                {
                    // Emit the completed candle
                    OnCandleCompleted?.Invoke(_currentCandle);

                    // Start a new candle aligned to the interval
                    _currentCandleStart = AlignToInterval(timestamp, _candleInterval);
                    _currentCandle = new Candle(_currentCandleStart, price, price, price, price);
                }
            }
        }

        private DateTime AlignToInterval(DateTime timestamp, TimeSpan interval)
        {
            long ticks = timestamp.Ticks - (timestamp.Ticks % interval.Ticks);
            return new DateTime(ticks, timestamp.Kind);
        }
    }
}
