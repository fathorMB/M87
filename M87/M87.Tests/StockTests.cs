using Xunit;
using Moq;
using M87.SimulatorCore.Models;
using M87.SimulatorCore.Interfaces;
using M87.SimulatorCore.Engine;

namespace M87.Tests
{
    public class StockTests
    {
        [Fact]
        public void UpdatePrice_ShouldSetCurrentPrice()
        {
            // Arrange
            var mockPriceSimulator = new Mock<IPriceSimulator>();
            var mockLogger = new Mock<ILogger>();
            var stock = new Stock("AAPL", 100.0, mockPriceSimulator.Object, mockLogger.Object);
            double newPrice = 105.0;

            // Act
            stock.UpdatePrice(newPrice);

            // Assert
            Assert.Equal(newPrice, stock.CurrentPrice);
        }

        [Fact]
        public void Stock_ShouldInitializeOrderManager()
        {
            // Arrange
            var mockPriceSimulator = new Mock<IPriceSimulator>();
            var mockLogger = new Mock<ILogger>();
            var stock = new Stock("AAPL", 100.0, mockPriceSimulator.Object, mockLogger.Object);

            // Act
            var orderManager = stock.OrderManager;

            // Assert
            Assert.NotNull(orderManager);
        }

        [Fact]
        public void Stock_ShouldUsePriceSimulatorToSimulatePrice()
        {
            // Arrange
            var mockPriceSimulator = new Mock<IPriceSimulator>();
            mockPriceSimulator.Setup(ps => ps.SimulateNextPrice(It.IsAny<double>(), It.IsAny<double>())).Returns(105.0);

            var mockLogger = new Mock<ILogger>();
            var stock = new Stock("AAPL", 100.0, mockPriceSimulator.Object, mockLogger.Object);
            double deltaTime = 1.0;

            // Act
            double simulatedPrice = stock.PriceSimulator.SimulateNextPrice(stock.CurrentPrice, deltaTime);

            // Assert
            Assert.Equal(105.0, simulatedPrice);
            mockPriceSimulator.Verify(ps => ps.SimulateNextPrice(100.0, deltaTime), Times.Once);
        }
    }
}
