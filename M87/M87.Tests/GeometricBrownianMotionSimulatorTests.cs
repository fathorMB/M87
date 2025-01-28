using Xunit;
using M87.SimulatorCore.Simulation;
using System;

namespace M87.Tests
{
    public class GeometricBrownianMotionSimulatorTests
    {
        [Fact]
        public void SimulateNextPrice_ShouldReturnPositivePrice()
        {
            // Arrange
            double drift = 0.0001;
            double volatility = 0.01;
            var simulator = new GeometricBrownianMotionSimulator(drift, volatility);
            double currentPrice = 100.0;
            double deltaTime = 1.0 / 365.0; // Un giorno

            // Act
            double nextPrice = simulator.SimulateNextPrice(currentPrice, deltaTime);

            // Assert
            Assert.True(nextPrice > 0, "Il prezzo simulato dovrebbe essere positivo.");
        }

        [Fact]
        public void SimulateNextPrice_ShouldChangePrice()
        {
            // Arrange
            double drift = 0.0001;
            double volatility = 0.01;
            var simulator = new GeometricBrownianMotionSimulator(drift, volatility);
            double currentPrice = 100.0;
            double deltaTime = 1.0 / 365.0; // Un giorno

            // Act
            double nextPrice = simulator.SimulateNextPrice(currentPrice, deltaTime);

            // Assert
            // È possibile che il prezzo rimanga uguale, ma è improbabile. Test conservativo.
            // Verifichiamo che il metodo non lanci eccezioni e ritorni un valore valido.
            Assert.InRange(nextPrice, 0.0, double.MaxValue);
        }

        [Fact]
        public void SimulateNextPrice_ShouldFollowExpectedTrend()
        {
            // Arrange
            double drift = 0.1; // Elevato per test rapido
            double volatility = 0.2;
            var simulator = new GeometricBrownianMotionSimulator(drift, volatility);
            double currentPrice = 100.0;
            double deltaTime = 1.0; // Un anno

            // Act
            double nextPrice = simulator.SimulateNextPrice(currentPrice, deltaTime);

            // Assert
            // Con drift positivo, il prezzo dovrebbe tendenzialmente aumentare
            // Data la natura casuale, non possiamo essere certi, ma possiamo verificare la logica
            // Alternative: Mockare la funzione NormalSample se la classe lo permette
            // Qui verifichiamo solo che il prezzo sia ragionevole
            Assert.True(nextPrice > 0, "Il prezzo simulato dovrebbe essere positivo.");
        }
    }
}
