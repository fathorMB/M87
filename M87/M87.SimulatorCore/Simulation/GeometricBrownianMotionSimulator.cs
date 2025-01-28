using M87.SimulatorCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M87.SimulatorCore.Simulation
{
    public class GeometricBrownianMotionSimulator : IPriceSimulator
    {
        private double _drift;
        private double _volatility;
        private Random _random;

        public GeometricBrownianMotionSimulator(double drift, double volatility)
        {
            _drift = drift;
            _volatility = volatility;
            _random = new Random();
        }

        public double SimulateNextPrice(double currentPrice, double deltaTime)
        {
            double epsilon = NormalSample(); // Campione da una distribuzione normale
            return currentPrice * Math.Exp((_drift - 0.5 * Math.Pow(_volatility, 2)) * deltaTime + _volatility * Math.Sqrt(deltaTime) * epsilon);
        }

        private double NormalSample()
        {
            // Metodo per generare un campione da una distribuzione normale
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2);
            return randStdNormal;
        }
    }
}
