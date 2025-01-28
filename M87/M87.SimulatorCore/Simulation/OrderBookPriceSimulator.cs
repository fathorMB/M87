using M87.SimulatorCore.Engine;
using M87.SimulatorCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Simulation
{
    public class OrderBookPriceSimulator : IPriceSimulator
    {
        private double _drift;
        private double _volatility;
        private OrderBook _orderBook;
        private Random _random;

        public OrderBookPriceSimulator(double drift, double volatility, OrderBook orderBook)
        {
            _drift = drift;
            _volatility = volatility;
            _orderBook = orderBook;
            _random = new Random();
        }

        public double SimulateNextPrice(double currentPrice, double deltaTime)
        {
            // Calcolare la pressione dal book
            double buyPressure = CalculateBuyPressure();
            double sellPressure = CalculateSellPressure();
            double netPressure = buyPressure - sellPressure;

            // Modificare drift in base alla pressione
            double adjustedDrift = _drift + netPressure * 0.01; // Coefficiente di sensibilità

            // Simulare GBM con drift aggiustato
            double epsilon = NormalSample();
            return currentPrice * Math.Exp((adjustedDrift - 0.5 * Math.Pow(_volatility, 2)) * deltaTime + _volatility * Math.Sqrt(deltaTime) * epsilon);
        }

        private double CalculateBuyPressure()
        {
            // Esempio: somma delle quantità dei bid sopra un certo livello
            return _orderBook.Bids.Take(5).Sum(order => order.Quantity);
        }

        private double CalculateSellPressure()
        {
            // Esempio: somma delle quantità degli ask sotto un certo livello
            return _orderBook.Asks.Take(5).Sum(order => order.Quantity);
        }

        private double NormalSample()
        {
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }
    }
}
