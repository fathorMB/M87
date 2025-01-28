using Xunit;
using M87.SimulatorCore.Engine;
using M87.SimulatorCore.Models;
using M87.SimulatorCore.Interfaces;
using Moq;
using System.Collections.Generic;

namespace M87.Tests
{
    public class MatchingEngineTests
    {
        [Fact]
        public void MatchOrders_ShouldMatchCompatibleOrders()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var orderBook = new OrderBook("AAPL");
            var orderManager = new OrderManager(orderBook, mockLogger.Object);

            var bid = new Order
            {
                Price = 150.0,
                Quantity = 100,
                Side = OrderSide.Buy,
                StockSymbol = "AAPL"
            };

            var ask = new Order
            {
                Price = 149.5,
                Quantity = 100,
                Side = OrderSide.Sell,
                StockSymbol = "AAPL"
            };

            // Act
            orderManager.SubmitOrder(bid);
            orderManager.SubmitOrder(ask);

            // Assert
            Assert.Empty(orderBook.Bids);
            Assert.Empty(orderBook.Asks);
            mockLogger.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Ordine eseguito"))), Times.Once);
        }

        [Fact]
        public void MatchOrders_ShouldHandlePartialMatches_WhenBidQuantityLessThanAsk()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var orderBook = new OrderBook("AAPL");
            var orderManager = new OrderManager(orderBook, mockLogger.Object);

            var bid = new Order
            {
                Price = 150.0,
                Quantity = 50,
                Side = OrderSide.Buy,
                StockSymbol = "AAPL"
            };

            var ask = new Order
            {
                Price = 149.5,
                Quantity = 100,
                Side = OrderSide.Sell,
                StockSymbol = "AAPL"
            };

            // Act
            orderManager.SubmitOrder(bid);
            orderManager.SubmitOrder(ask);

            // Assert
            Assert.Empty(orderBook.Bids);
            Assert.Single(orderBook.Asks);
            var remainingAsk = orderBook.Asks.First();
            Assert.Equal(50, remainingAsk.Quantity);
            mockLogger.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Ordine eseguito"))), Times.Once);
        }

        [Fact]
        public void MatchOrders_ShouldHandlePartialMatches_WhenAskQuantityLessThanBid()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var orderBook = new OrderBook("AAPL");
            var orderManager = new OrderManager(orderBook, mockLogger.Object);

            var bid = new Order
            {
                Price = 150.0,
                Quantity = 100,
                Side = OrderSide.Buy,
                StockSymbol = "AAPL"
            };

            var ask = new Order
            {
                Price = 149.5,
                Quantity = 50,
                Side = OrderSide.Sell,
                StockSymbol = "AAPL"
            };

            // Act
            orderManager.SubmitOrder(bid);
            orderManager.SubmitOrder(ask);

            // Assert
            Assert.Single(orderBook.Bids);
            var remainingBid = orderBook.Bids.First();
            Assert.Equal(50, remainingBid.Quantity);
            Assert.Empty(orderBook.Asks);
            mockLogger.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Ordine eseguito"))), Times.Once);
        }

        [Fact]
        public void MatchOrders_ShouldNotMatch_WhenNoCompatiblePrices()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var orderBook = new OrderBook("AAPL");
            var orderManager = new OrderManager(orderBook, mockLogger.Object);

            var bid = new Order
            {
                Price = 148.0,
                Quantity = 100,
                Side = OrderSide.Buy,
                StockSymbol = "AAPL"
            };

            var ask = new Order
            {
                Price = 149.5,
                Quantity = 100,
                Side = OrderSide.Sell,
                StockSymbol = "AAPL"
            };

            // Act
            orderManager.SubmitOrder(bid);
            orderManager.SubmitOrder(ask);

            // Assert
            Assert.Single(orderBook.Bids);
            Assert.Single(orderBook.Asks);
            mockLogger.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Ordine eseguito"))), Times.Never);
        }

        [Fact]
        public void MatchOrders_ShouldProcessMultipleMatches()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var orderBook = new OrderBook("AAPL");
            var orderManager = new OrderManager(orderBook, mockLogger.Object);

            var bid1 = new Order { Price = 150.0, Quantity = 100, Side = OrderSide.Buy, StockSymbol = "AAPL" };
            var bid2 = new Order { Price = 151.0, Quantity = 50, Side = OrderSide.Buy, StockSymbol = "AAPL" };

            var ask1 = new Order { Price = 149.0, Quantity = 70, Side = OrderSide.Sell, StockSymbol = "AAPL" };
            var ask2 = new Order { Price = 149.5, Quantity = 80, Side = OrderSide.Sell, StockSymbol = "AAPL" };

            // Act
            orderManager.SubmitOrder(bid1);
            orderManager.SubmitOrder(bid2);
            orderManager.SubmitOrder(ask1);
            orderManager.SubmitOrder(ask2);

            // Assert
            // Dopo abbinamento:
            // bid2 (151.0, 50) si abbina con ask1 (149.0, 70) per 50
            // ask1 rimanente: 20
            // bid1 (150.0, 100) si abbina con ask1 (149.0, 20) per 20
            // ask2 (149.5, 80) si abbina con bid1 (150.0, 80) per 80
            // Bid1 rimanente: 100 - 20 - 80 = 0
            // Ask2 rimanente: 80 - 80 = 0

            // Asserzioni:
            Assert.Empty(orderBook.Bids);
            Assert.Empty(orderBook.Asks);
            mockLogger.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Ordine eseguito"))), Times.Exactly(3));
        }
    }
}
