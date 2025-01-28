using Xunit;
using M87.SimulatorCore.Engine;
using M87.SimulatorCore.Models;
using System;
using System.Linq;
using Moq;
using M87.SimulatorCore.Interfaces;

namespace M87.Tests
{
    public class OrderBookTests
    {
        [Fact]
        public void AddOrder_ShouldAddBuyOrderToBids()
        {
            // Arrange
            var orderBook = new OrderBook("AAPL");
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
            orderBook.AddOrder(order);

            // Assert
            Assert.Single(orderBook.Bids);
            Assert.Equal(order, orderBook.Bids.First());
            Assert.Empty(orderBook.Asks);
        }

        [Fact]
        public void AddOrder_ShouldAddSellOrderToAsks()
        {
            // Arrange
            var orderBook = new OrderBook("AAPL");
            var order = new Order
            {
                StockSymbol = "AAPL",
                Type = OrderType.Limit,
                Side = OrderSide.Sell,
                Price = 151.0,
                Quantity = 50,
                ClientId = "Client2"
            };

            // Act
            orderBook.AddOrder(order);

            // Assert
            Assert.Single(orderBook.Asks);
            Assert.Equal(order, orderBook.Asks.First());
            Assert.Empty(orderBook.Bids);
        }

        [Fact]
        public void Bids_ShouldBeSortedDescendingByPrice()
        {
            // Arrange
            var orderBook = new OrderBook("AAPL");
            var order1 = new Order { Price = 150.0, Side = OrderSide.Buy };
            var order2 = new Order { Price = 152.0, Side = OrderSide.Buy };
            var order3 = new Order { Price = 149.0, Side = OrderSide.Buy };

            // Act
            orderBook.AddOrder(order1);
            orderBook.AddOrder(order2);
            orderBook.AddOrder(order3);

            // Assert
            Assert.Equal(3, orderBook.Bids.Count);
            Assert.Equal(152.0, orderBook.Bids.ElementAt(0).Price);
            Assert.Equal(150.0, orderBook.Bids.ElementAt(1).Price);
            Assert.Equal(149.0, orderBook.Bids.ElementAt(2).Price);
        }

        [Fact]
        public void Asks_ShouldBeSortedAscendingByPrice()
        {
            // Arrange
            var orderBook = new OrderBook("AAPL");
            var order1 = new Order { Price = 150.0, Side = OrderSide.Sell };
            var order2 = new Order { Price = 148.0, Side = OrderSide.Sell };
            var order3 = new Order { Price = 151.0, Side = OrderSide.Sell };

            // Act
            orderBook.AddOrder(order1);
            orderBook.AddOrder(order2);
            orderBook.AddOrder(order3);

            // Assert
            Assert.Equal(3, orderBook.Asks.Count);
            Assert.Equal(148.0, orderBook.Asks.ElementAt(0).Price);
            Assert.Equal(150.0, orderBook.Asks.ElementAt(1).Price);
            Assert.Equal(151.0, orderBook.Asks.ElementAt(2).Price);
        }

        [Fact]
        public void AddOrder_ShouldMatchOrders_WhenBidMeetsAsk()
        {
            // Arrange
            var orderBook = new OrderBook("AAPL");
            var bid = new Order
            {
                StockSymbol = "AAPL",
                Type = OrderType.Limit,
                Side = OrderSide.Buy,
                Price = 150.0,
                Quantity = 100,
                ClientId = "Client1"
            };
            var ask = new Order
            {
                StockSymbol = "AAPL",
                Type = OrderType.Limit,
                Side = OrderSide.Sell,
                Price = 149.5,
                Quantity = 100,
                ClientId = "Client2"
            };

            Order matchedBid = null;
            Order matchedAsk = null;
            orderBook.OnOrderMatched += (b, a) =>
            {
                matchedBid = b;
                matchedAsk = a;
            };

            // Act
            orderBook.AddOrder(bid);
            orderBook.AddOrder(ask);

            // Assert
            Assert.Empty(orderBook.Bids);
            Assert.Empty(orderBook.Asks);
            Assert.NotNull(matchedBid);
            Assert.NotNull(matchedAsk);
            Assert.Equal(bid.OrderId, matchedBid.OrderId);
            Assert.Equal(ask.OrderId, matchedAsk.OrderId);
        }

        [Fact]
        public void AddOrder_ShouldPartialMatch_WhenBidQuantityLessThanAsk()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var orderBook = new OrderBook("AAPL");
            var orderManager = new OrderManager(orderBook, mockLogger.Object);

            var bid = new Order
            {
                Price = 150.0,
                Quantity = 50, // Bid Quantity inferiore
                Side = OrderSide.Buy,
                StockSymbol = "AAPL"
            };

            var ask = new Order
            {
                Price = 149.5,
                Quantity = 100, // Ask Quantity maggiore
                Side = OrderSide.Sell,
                StockSymbol = "AAPL"
            };

            // Act
            orderManager.SubmitOrder(bid);
            orderManager.SubmitOrder(ask);

            // Assert
            Assert.Empty(orderBook.Bids); // Il bid è completamente esaurito
            Assert.Single(orderBook.Asks); // Rimane un ask residuo
            var remainingAsk = orderBook.Asks.First();
            Assert.Equal(50, remainingAsk.Quantity); // Residuo dell'ask
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
                Quantity = 100, // Bid Quantity maggiore
                Side = OrderSide.Buy,
                StockSymbol = "AAPL"
            };

            var ask = new Order
            {
                Price = 149.5,
                Quantity = 50, // Ask Quantity minore
                Side = OrderSide.Sell,
                StockSymbol = "AAPL"
            };

            // Act
            orderManager.SubmitOrder(bid);
            orderManager.SubmitOrder(ask);

            // Assert
            Assert.Single(orderBook.Bids);
            var remainingBid = orderBook.Bids.First();
            Assert.Equal(50, remainingBid.Quantity); // Bid residuo
            Assert.Empty(orderBook.Asks); // Ask completamente esaurito
            mockLogger.Verify(logger => logger.Log(It.Is<string>(s => s.Contains("Ordine eseguito"))), Times.Once);
        }

        [Fact]
        public void RemoveOrder_ShouldRemoveSpecifiedOrder()
        {
            // Arrange
            var orderBook = new OrderBook("AAPL");
            var bid = new Order
            {
                StockSymbol = "AAPL",
                Type = OrderType.Limit,
                Side = OrderSide.Buy,
                Price = 150.0,
                Quantity = 100,
                ClientId = "Client1"
            };
            var ask = new Order
            {
                StockSymbol = "AAPL",
                Type = OrderType.Limit,
                Side = OrderSide.Sell,
                Price = 151.0,
                Quantity = 100,
                ClientId = "Client2"
            };

            // Act
            orderBook.AddOrder(bid);
            orderBook.AddOrder(ask);
            orderBook.RemoveOrder(bid);
            orderBook.RemoveOrder(ask);

            // Assert
            Assert.Empty(orderBook.Bids);
            Assert.Empty(orderBook.Asks);
        }
    }
}
