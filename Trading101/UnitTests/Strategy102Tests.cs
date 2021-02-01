using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using TradingStrategies;
using Xunit;

namespace UnitTests
{
    public class Strategy102Tests
    {
        [Fact]
        public async Task IsBearish()
        {
            var symbol = "MSFT";
            var loggerMock = new Mock<ILogger<Strategy102>>();
            var marketDataMock = new Mock<IMarketData>();

            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9)).ReturnsAsync(101.1m); // Above the market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 20)).ReturnsAsync(102m); // Above the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 50)).ReturnsAsync(103m); // Above the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 200)).ReturnsAsync(104m); // Above the market price

            var strategy = new Strategy102(symbol, loggerMock.Object, marketDataMock.Object);
            await strategy.GetMarketData();
            var isBearish = strategy.IsBearish();

            Assert.True(isBearish);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT - Latest EMA(9): 101.1, EMA(20): 102, SMA(50): 103, SMA(200): 104", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Theory]
        [InlineData(99.9, 101, 102, 103, true)] // Strong bearish because 3 indicators are above the market price
        [InlineData(99.9, 99.8, 102, 103, false)] // Weak bearish because only 2 indicators are above the market price
        [InlineData(99.9, 98.7, 98.9, 103, false)] // Strong bullish because 3 indicators are above the market price
        public async Task IsBearishTheory(decimal ema9, decimal ema20, decimal sma50, decimal sma200, bool expected)
        {
            var symbol = "MSFT";
            var loggerMock = new Mock<ILogger<Strategy102>>();
            var marketDataMock = new Mock<IMarketData>();

            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9)).ReturnsAsync(ema9);
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 20)).ReturnsAsync(ema20);
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 50)).ReturnsAsync(sma50);
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 200)).ReturnsAsync(sma200);

            var strategy = new Strategy102(symbol, loggerMock.Object, marketDataMock.Object);
            await strategy.GetMarketData();
            var isBearish = strategy.IsBearish();

            Assert.Equal(expected, isBearish);
        }

        [Fact]
        public async Task IsBullish()
        {
            var symbol = "MSFT";
            var loggerMock = new Mock<ILogger<Strategy102>>();
            var marketDataMock = new Mock<IMarketData>();

            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9)).ReturnsAsync(99.9m); // Below the market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 20)).ReturnsAsync(99.8m); // Below the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 50)).ReturnsAsync(99.7m); // Below the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 200)).ReturnsAsync(99.6m); // Below the market price

            var strategy = new Strategy102(symbol, loggerMock.Object, marketDataMock.Object);
            await strategy.GetMarketData();
            var isBullish = strategy.IsBullish();

            Assert.True(isBullish);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT - Latest EMA(9): 99.9, EMA(20): 99.8, SMA(50): 99.7, SMA(200): 99.6", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Theory]
        [InlineData(99.9, 101, 102, 103, false)] // Strong bearish because 3 indicators are above the market price
        [InlineData(99.9, 99.8, 102, 103, false)] // Weak bearish because only 2 indicators are above the market price
        [InlineData(99.9, 98.7, 98.9, 103, true)] // Strong bullish because 3 indicators are above the market price
        public async Task IsBullishTheory(decimal ema9, decimal ema20, decimal sma50, decimal sma200, bool expected)
        {
            var symbol = "MSFT";
            var loggerMock = new Mock<ILogger<Strategy102>>();
            var marketDataMock = new Mock<IMarketData>();

            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9)).ReturnsAsync(ema9);
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 20)).ReturnsAsync(ema20);
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 50)).ReturnsAsync(sma50);
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 200)).ReturnsAsync(sma200);

            var strategy = new Strategy102(symbol, loggerMock.Object, marketDataMock.Object);
            await strategy.GetMarketData();
            var isBullish = strategy.IsBullish();

            Assert.Equal(expected, isBullish);
        }

        [Fact]
        public async Task ShouldShortTrade()
        {
            var symbol = "MSFT";
            var loggerMock = new Mock<ILogger<Strategy102>>();
            var marketDataMock = new Mock<IMarketData>();

            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9)).ReturnsAsync(101.1m); // Above the market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 20)).ReturnsAsync(102m); // Above the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 50)).ReturnsAsync(103m); // Above the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 200)).ReturnsAsync(104m); // Above the market price
            marketDataMock.Setup(mock => mock.GetIntradayHighestHighLowestLowAsync(symbol)).ReturnsAsync((101m, 98m));

            var strategy = new Strategy102(symbol, loggerMock.Object, marketDataMock.Object);
            await strategy.GetMarketData();
            var isBearish = strategy.IsBearish();

            Assert.True(isBearish);

            var shouldTrade = strategy.ShouldTrade(99.7m);
            Assert.True(shouldTrade); // Should trade as the lowest low is 98.0 and is lower than our target reward
        }

        [Theory]
        [InlineData(10.1, 10.2, true)]
        [InlineData(88.4, 88.5, true)]
        [InlineData(53.6, 53.4, false)]
        [InlineData(23.4, 22.4, false)]
        public async Task ShouldShortTradeTheory(decimal lowestLow, decimal targetProfit, bool expected)
        {
            var symbol = "MSFT";
            var loggerMock = new Mock<ILogger<Strategy102>>();
            var marketDataMock = new Mock<IMarketData>();

            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9)).ReturnsAsync(101.1m); // Above the market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 20)).ReturnsAsync(102m); // Above the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 50)).ReturnsAsync(103m); // Above the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 200)).ReturnsAsync(104m); // Above the market price
            marketDataMock.Setup(mock => mock.GetIntradayHighestHighLowestLowAsync(symbol)).ReturnsAsync((101m, lowestLow));

            var strategy = new Strategy102(symbol, loggerMock.Object, marketDataMock.Object);
            await strategy.GetMarketData();
            var isBearish = strategy.IsBearish();

            Assert.True(isBearish);

            var shouldTrade = strategy.ShouldTrade(targetProfit);
            Assert.Equal(expected, shouldTrade);
        }

        [Fact]
        public async Task ShouldLongTrade()
        {
            var symbol = "MSFT";
            var loggerMock = new Mock<ILogger<Strategy102>>();
            var marketDataMock = new Mock<IMarketData>();

            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9)).ReturnsAsync(99.9m); // Below the market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 20)).ReturnsAsync(99.8m); // Below the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 50)).ReturnsAsync(99.7m); // Below the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 200)).ReturnsAsync(99.6m); // Below the market price
            marketDataMock.Setup(mock => mock.GetIntradayHighestHighLowestLowAsync(symbol)).ReturnsAsync((101m, 98m));

            var strategy = new Strategy102(symbol, loggerMock.Object, marketDataMock.Object);
            await strategy.GetMarketData();
            var isBullish = strategy.IsBullish();

            Assert.True(isBullish);

            var shouldTrade = strategy.ShouldTrade(100.9m);
            Assert.True(shouldTrade); // Should trade as the highest high is 101 and is higher than our target reward
        }

        [Theory]
        [InlineData(10.3, 10.2, true)]
        [InlineData(88.7, 88.5, true)]
        [InlineData(53.1, 53.4, false)]
        [InlineData(22.4, 22.4, true)]
        [InlineData(22.3, 22.4, false)]
        public async Task ShouldLongTradeTheory(decimal highestHigh, decimal targetProfit, bool expected)
        {
            var symbol = "MSFT";
            var loggerMock = new Mock<ILogger<Strategy102>>();
            var marketDataMock = new Mock<IMarketData>();

            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9)).ReturnsAsync(99.9m); // Below the market price
            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 20)).ReturnsAsync(99.8m); // Below the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 50)).ReturnsAsync(99.7m); // Below the market price
            marketDataMock.Setup(mock => mock.GetLatestSmaAsync(symbol, 200)).ReturnsAsync(99.6m); // Below the market price
            marketDataMock.Setup(mock => mock.GetIntradayHighestHighLowestLowAsync(symbol)).ReturnsAsync((highestHigh, 98m));

            var strategy = new Strategy102(symbol, loggerMock.Object, marketDataMock.Object);
            await strategy.GetMarketData();
            var isBullish = strategy.IsBullish();

            Assert.True(isBullish);

            var shouldTrade = strategy.ShouldTrade(targetProfit);
            Assert.Equal(expected, shouldTrade);
        }
    }
}
