using Xunit;
using Moq;
using M87.SimulatorCore.Engine;
using M87.SimulatorCore.Models;
using M87.SimulatorCore.Interfaces;
using System;

namespace M87.Tests
{
    public class OrderManagerTests
    {
        [Fact]
        public void SubmitOrder_ShouldAddValidOrderToOrderBook()
        {
            // Arrange
            var mockOrderBook = new Mock<IOrderBook>();
            var mockLogger = new Mock<ILogger>();
            var orderManager = new OrderManager(mockOrderBook.Object, mockLogger.Object);

            var order = new Order
            {
                StockSymbol = "AAPL",
                Type = OrderType.Limit,
                Side = OrderSide.Buy,
                Price = 150.0,
                Quantity = 100,
                ClientId = "Client1"
            };

            // Act
            orderManager.SubmitOrder(order);

            // Assert
            mockOrderBook.Verify(ob => ob.AddOrder(order), Times.Once);
            mockLogger.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Ordine inserito"))), Times.Once);
        }

        [Theory]
        [InlineData(0, 150.0)]   // Quantity <= 0
        [InlineData(-10, 150.0)]
        [InlineData(100, 0.0)]    // Price <= 0 for Limit Order
        [InlineData(100, -50.0)]
        public void SubmitOrder_ShouldRejectInvalidOrders(int quantity, double price)
        {
            // Arrange
            var mockOrderBook = new Mock<IOrderBook>();
            var mockLogger = new Mock<ILogger>();
            var orderManager = new OrderManager(mockOrderBook.Object, mockLogger.Object);

            var order = new Order
            {
                StockSymbol = "AAPL",
                Type = OrderType.Limit,
                Side = OrderSide.Sell,
                Price = price,
                Quantity = quantity,
                ClientId = "Client2"
            };

            // Act
            orderManager.SubmitOrder(order);

            // Assert
            mockOrderBook.Verify(ob => ob.AddOrder(It.IsAny<Order>()), Times.Never);
            mockLogger.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Ordine non valido"))), Times.Once);
        }

        [Fact]
        public void HandleOrderMatched_ShouldLogExecutedOrder()
        {
            // Arrange
            var mockOrderBook = new Mock<IOrderBook>();
            var mockLogger = new Mock<ILogger>();
            var orderManager = new OrderManager(mockOrderBook.Object, mockLogger.Object);

            // Simulare l'abbinamento degli ordini
            var bid = new Order
            {
                OrderId = Guid.NewGuid(),
                StockSymbol = "AAPL",
                Type = OrderType.Limit,
                Side = OrderSide.Buy,
                Price = 150.0,
                Quantity = 100,
                ClientId = "Client1",
                Timestamp = DateTime.UtcNow
            };
            var ask = new Order
            {
                OrderId = Guid.NewGuid(),
                StockSymbol = "AAPL",
                Type = OrderType.Limit,
                Side = OrderSide.Sell,
                Price = 149.5,
                Quantity = 100,
                ClientId = "Client2",
                Timestamp = DateTime.UtcNow
            };

            // Simulare l'evento OnOrderMatched
            mockOrderBook.Raise(ob => ob.OnOrderMatched += null, bid, ask);

            // Assert
            mockLogger.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Ordine eseguito"))), Times.Once);
        }
    }
}
