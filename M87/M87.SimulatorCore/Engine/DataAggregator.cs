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

        public event Action<Candle> OnCandleCompleted;

        public DataAggregator(TimeSpan candleInterval)
        {
            _candleInterval = candleInterval;
        }

        public void AddPrice(DateTime timestamp, double price)
        {
            if (_currentCandle == null)
            {
                _currentCandleStart = timestamp;
                _currentCandle = new Candle(_currentCandleStart, price, price, price, price);
                return;
            }

            if (timestamp - _currentCandleStart < _candleInterval)
            {
                _currentCandle.Close = price;
                if (price > _currentCandle.High) _currentCandle.High = price;
                if (price < _currentCandle.Low) _currentCandle.Low = price;
            }
            else
            {
                OnCandleCompleted?.Invoke(_currentCandle);
                _currentCandleStart = timestamp;
                _currentCandle = new Candle(_currentCandleStart, price, price, price, price);
            }
        }
    }
}
