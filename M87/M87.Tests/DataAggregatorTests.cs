using Xunit;
using M87.SimulatorCore.Engine;
using M87.SimulatorCore.Models;
using System;
using System.Collections.Generic;
using M87.Contracts;

namespace M87.Tests
{
    public class DataAggregatorTests
    {
        [Fact]
        public void AddPrice_ShouldCreateNewCandle_OnFirstPrice()
        {
            // Arrange
            TimeSpan candleInterval = TimeSpan.FromMinutes(1);
            var aggregator = new DataAggregator(candleInterval);
            Candle completedCandle = null;
            aggregator.OnCandleCompleted += (c) => completedCandle = c;

            DateTime timestamp = new DateTime(2025, 1, 1, 9, 30, 0);
            double price = 100.0;

            // Act
            aggregator.AddPrice(timestamp, price);

            // Assert
            Assert.Null(completedCandle);
        }

        [Fact]
        public void AddPrice_ShouldUpdateCurrentCandle()
        {
            // Arrange
            TimeSpan candleInterval = TimeSpan.FromMinutes(1);
            var aggregator = new DataAggregator(candleInterval);
            Candle completedCandle = null;
            aggregator.OnCandleCompleted += (c) => completedCandle = c;

            DateTime timestamp1 = new DateTime(2025, 1, 1, 9, 30, 0);
            double price1 = 100.0;

            DateTime timestamp2 = new DateTime(2025, 1, 1, 9, 30, 30);
            double price2 = 101.0;

            // Act
            aggregator.AddPrice(timestamp1, price1);
            aggregator.AddPrice(timestamp2, price2);

            // Assert
            Assert.Null(completedCandle);
        }

        [Fact]
        public void AddPrice_ShouldCompleteCandle_WhenIntervalExceeded()
        {
            // Arrange
            TimeSpan candleInterval = TimeSpan.FromMinutes(1);
            var aggregator = new DataAggregator(candleInterval);
            List<Candle> completedCandles = new List<Candle>();
            aggregator.OnCandleCompleted += (c) => completedCandles.Add(c);

            DateTime timestamp1 = new DateTime(2025, 1, 1, 9, 30, 0);
            double price1 = 100.0;

            DateTime timestamp2 = new DateTime(2025, 1, 1, 9, 31, 0); // Avanzamento di 1 minuto
            double price2 = 101.0;

            // Act
            aggregator.AddPrice(timestamp1, price1);
            aggregator.AddPrice(timestamp2, price2);

            // Assert
            Assert.Single(completedCandles);
            var candle = completedCandles[0];
            Assert.Equal(timestamp1, UnixTimeStampToDateTime(candle.Time));
            Assert.Equal(100.0, candle.Open);
            Assert.Equal(100.0, candle.High);
            Assert.Equal(100.0, candle.Low);
            Assert.Equal(100.0, candle.Close);
        }

        [Fact]
        public void AddPrice_ShouldUpdateHighLow_Correctly()
        {
            // Arrange
            TimeSpan candleInterval = TimeSpan.FromMinutes(1);
            var aggregator = new DataAggregator(candleInterval);
            List<Candle> completedCandles = new List<Candle>();
            aggregator.OnCandleCompleted += (c) => completedCandles.Add(c);

            DateTime timestamp1 = new DateTime(2025, 1, 1, 9, 30, 0);
            double price1 = 100.0;

            DateTime timestamp2 = new DateTime(2025, 1, 1, 9, 30, 30);
            double price2 = 102.0;

            DateTime timestamp3 = new DateTime(2025, 1, 1, 9, 31, 0);
            double price3 = 101.0;

            // Act
            aggregator.AddPrice(timestamp1, price1);
            aggregator.AddPrice(timestamp2, price2);
            aggregator.AddPrice(timestamp3, price3);

            // Assert
            Assert.Single(completedCandles);
            var candle = completedCandles[0];
            Assert.Equal(timestamp1, UnixTimeStampToDateTime(candle.Time));
            Assert.Equal(100.0, candle.Open);
            Assert.Equal(102.0, candle.High);
            Assert.Equal(100.0, candle.Low);
            Assert.Equal(102.0, candle.Close);
        }

        [Fact]
        public void AddPrice_ShouldHandleMultipleCandleCompletions()
        {
            // Arrange
            TimeSpan candleInterval = TimeSpan.FromMinutes(1);
            var aggregator = new DataAggregator(candleInterval);
            List<Candle> completedCandles = new List<Candle>();
            aggregator.OnCandleCompleted += (c) => completedCandles.Add(c);

            // Simulate prices over 3 minutes
            for (int i = 0; i < 3; i++)
            {
                DateTime timestamp = new DateTime(2025, 1, 1, 9, 30 + i, 0);
                double price = 100.0 + i;
                aggregator.AddPrice(timestamp, price);
            }

            // Assert
            Assert.Equal(2, completedCandles.Count);

            // First candle
            var candle1 = completedCandles[0];
            Assert.Equal(new DateTime(2025, 1, 1, 9, 30, 0), UnixTimeStampToDateTime(candle1.Time));
            Assert.Equal(100.0, candle1.Open);
            Assert.Equal(100.0, candle1.High);
            Assert.Equal(100.0, candle1.Low);
            Assert.Equal(100.0, candle1.Close);

            // Second candle
            var candle2 = completedCandles[1];
            Assert.Equal(new DateTime(2025, 1, 1, 9, 31, 0), UnixTimeStampToDateTime(candle2.Time));
            Assert.Equal(101.0, candle2.Open);
            Assert.Equal(101.0, candle2.High);
            Assert.Equal(101.0, candle2.Low);
            Assert.Equal(101.0, candle2.Close);
        }

        [Fact]
        public void AddPrice_ShouldUpdateClosePrice()
        {
            // Arrange
            TimeSpan candleInterval = TimeSpan.FromMinutes(1);
            var aggregator = new DataAggregator(candleInterval);
            List<Candle> completedCandles = new List<Candle>();
            aggregator.OnCandleCompleted += (c) => completedCandles.Add(c);

            DateTime timestamp1 = new DateTime(2025, 1, 1, 9, 30, 0);
            double price1 = 100.0;

            DateTime timestamp2 = new DateTime(2025, 1, 1, 9, 30, 30);
            double price2 = 102.0;

            DateTime timestamp3 = new DateTime(2025, 1, 1, 9, 30, 45);
            double price3 = 101.0;

            DateTime timestamp4 = new DateTime(2025, 1, 1, 9, 31, 0);
            double price4 = 103.0;

            // Act
            aggregator.AddPrice(timestamp1, price1);
            aggregator.AddPrice(timestamp2, price2);
            aggregator.AddPrice(timestamp3, price3);
            aggregator.AddPrice(timestamp4, price4);

            // Assert
            Assert.Single(completedCandles);
            var candle = completedCandles[0];
            Assert.Equal(timestamp1, UnixTimeStampToDateTime(candle.Time));
            Assert.Equal(100.0, candle.Open);
            Assert.Equal(102.0, candle.High);
            Assert.Equal(100.0, candle.Low);
            Assert.Equal(101.0, candle.Close);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
