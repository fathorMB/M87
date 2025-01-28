using M87.SimulatorCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Engine
{
    public class DataAggregator
    {
        private TimeSpan _candleInterval;
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
                _currentCandleStart = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, 0);
                _currentCandle = new Candle
                {
                    Timestamp = _currentCandleStart,
                    Open = price,
                    High = price,
                    Low = price,
                    Close = price
                };
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
                // Emissione della candela completata
                OnCandleCompleted?.Invoke(_currentCandle);

                // Inizializzazione della nuova candela
                _currentCandleStart = _currentCandleStart.Add(_candleInterval);
                _currentCandle = new Candle
                {
                    Timestamp = _currentCandleStart,
                    Open = price,
                    High = price,
                    Low = price,
                    Close = price
                };
            }
        }
    }
}
