//using Microsoft.Extensions.Logging;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using TradingStrategies;
//using TradingStrategies.Constants;
//using TradingStrategies.Wrappers;
//using Xunit;

//namespace UnitTests
//{
//    public class Strategy101Tests
//    {
//        [Fact]
//        public async Task ShouldBuyUnderNormalCircumstances()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 200m;

//            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
//            var fakeTradingDayInEst = TimeZoneInfo.ConvertTimeFromUtc(fakeTradingDay, easternZone);

//            Assert.Equal(new DateTime(2021, 1, 6, 9, 30, 0), fakeTradingDayInEst);

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetAvailableFundsAsync(Currency.USD)).ReturnsAsync(200m);
//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(0d);
//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>());
//            brokerMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price

//            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price
//            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9, fakeTradingDayInEst)).ReturnsAsync(new TechnicalIndicator(99m, fakeTradingDayInEst)); // Below the market price
//            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 20, fakeTradingDayInEst)).ReturnsAsync(new TechnicalIndicator(98m, fakeTradingDayInEst)); // Below the market price

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldBuy = await strategy.ShouldBuyAsync(symbol);

//            Assert.True(shouldBuy);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is above EMA(9) 99 at 01/06/2021 09:30:00", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is above EMA(20) 98 at 01/06/2021 09:30:00", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldNotBuyIfAvailableFundsIsBelowFundsNeededFor2PercentPositionSize()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 500m;

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetAvailableFundsAsync(Currency.USD)).ReturnsAsync(90m);
//            brokerMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price

//            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldBuy = await strategy.ShouldBuyAsync(symbol);

//            Assert.False(shouldBuy);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("Should not buy MSFT, because available funds 90 is less than 99.5", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldNotBuyIfCurrentlyOwnTheStock()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 500m;

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetAvailableFundsAsync(Currency.USD)).ReturnsAsync(200m);
//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(1d);
//            brokerMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price

//            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldBuy = await strategy.ShouldBuyAsync(symbol);

//            Assert.False(shouldBuy);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("Should not buy MSFT, because you currently own a position of 1", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldNotBuyIfCurrentlyHaveOpenBuyOrder()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 500m;

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetAvailableFundsAsync(Currency.USD)).ReturnsAsync(200m);
//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(0d);
//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>
//            {
//                new Order { Action = "BUY", Symbol = "MSFT" }
//            });
//            brokerMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price

//            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldBuy = await strategy.ShouldBuyAsync(symbol);

//            Assert.False(shouldBuy);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("Should not buy MSFT, because you currently have 1 open buy orders", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldNotBuyIfOutsideUnitedStatesTradingHours()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeCapital = 500m;

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetAvailableFundsAsync(Currency.USD)).ReturnsAsync(200m);
//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(0d);
//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>());
//            brokerMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price

//            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price

//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 0, 0), DateTimeKind.Local); // 10PM
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldBuy = await strategy.ShouldBuyAsync(symbol);

//            Assert.False(shouldBuy);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("Should not buy MSFT, because the converted 01/06/2021 09:00:00 eastern time is not within the US trading hours", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);


//            fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 7, 6, 0, 0), DateTimeKind.Local); // 6AM
//            fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();

//            strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            shouldBuy = await strategy.ShouldBuyAsync(symbol);

//            Assert.False(shouldBuy);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("Should not buy MSFT, because the converted 01/06/2021 17:00:00 eastern time is not within the US trading hours", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldNotBuyIfMarketPriceIsBelowEma9()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 500m;

//            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
//            var fakeTradingDayInEst = TimeZoneInfo.ConvertTimeFromUtc(fakeTradingDay, easternZone);

//            Assert.Equal(new DateTime(2021, 1, 6, 9, 30, 0), fakeTradingDayInEst);

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetAvailableFundsAsync(Currency.USD)).ReturnsAsync(200m);
//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(0d);
//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>());
//            brokerMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price

//            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price
//            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9, fakeTradingDayInEst)).ReturnsAsync(new TechnicalIndicator(101m, fakeTradingDayInEst)); // Above the market price
//            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 20, fakeTradingDayInEst)).ReturnsAsync(new TechnicalIndicator(98m, fakeTradingDayInEst)); // Below the market price

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldBuy = await strategy.ShouldBuyAsync(symbol);

//            Assert.False(shouldBuy);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is below EMA(9) 101 at 01/06/2021 09:30:00", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is above EMA(20) 98 at 01/06/2021 09:30:00", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Never);
//        }

//        [Fact]
//        public async Task ShouldNotBuyIfMarketPriceIsBelowEma20()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 500m;

//            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
//            var fakeTradingDayInEst = TimeZoneInfo.ConvertTimeFromUtc(fakeTradingDay, easternZone);

//            Assert.Equal(new DateTime(2021, 1, 6, 9, 30, 0), fakeTradingDayInEst);

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetAvailableFundsAsync(Currency.USD)).ReturnsAsync(200m);
//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(0d);
//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>());
//            brokerMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price

//            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // Market price
//            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9, fakeTradingDayInEst)).ReturnsAsync(new TechnicalIndicator(99m, fakeTradingDayInEst)); // Below the market price
//            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 20, fakeTradingDayInEst)).ReturnsAsync(new TechnicalIndicator(100m, fakeTradingDayInEst)); // Equal the market price

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldBuy = await strategy.ShouldBuyAsync(symbol);

//            Assert.False(shouldBuy);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is above EMA(9) 99 at 01/06/2021 09:30:00", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is below EMA(20) 100 at 01/06/2021 09:30:00", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldSellIfMarketPriceIsBelowEma9()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 200m;

//            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
//            var fakeTradingDayInEst = TimeZoneInfo.ConvertTimeFromUtc(fakeTradingDay, easternZone);

//            Assert.Equal(new DateTime(2021, 1, 6, 9, 30, 0), fakeTradingDayInEst);

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(1d);
//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>());
//            brokerMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // 100 market price

//            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // 100 market price
//            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9, fakeTradingDayInEst)).ReturnsAsync(new TechnicalIndicator(101m, fakeTradingDayInEst)); // Above the market price

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldSell = await strategy.ShouldSellAsync(symbol);

//            Assert.True(shouldSell);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is below EMA(9) 101 at 01/06/2021 09:30:00", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is above open price 99", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldSellIfMarketPriceIsBelowOpenPrice()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 200m;

//            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
//            var fakeTradingDayInEst = TimeZoneInfo.ConvertTimeFromUtc(fakeTradingDay, easternZone);

//            Assert.Equal(new DateTime(2021, 1, 6, 9, 30, 0), fakeTradingDayInEst);

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(1d);
//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>());
//            brokerMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(101m, 100m, 101m, 98m)); // 100 market price, 101 opening price, indication of RED candle

//            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(101m, 100m, 101m, 98m)); // 100 market price, 101 opening price, indication of RED candle
//            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9, fakeTradingDayInEst)).ReturnsAsync(new TechnicalIndicator(99m, fakeTradingDayInEst)); // Below the market price

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldSell = await strategy.ShouldSellAsync(symbol);

//            Assert.True(shouldSell);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is above EMA(9) 99 at 01/06/2021 09:30:00", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is below open price 101", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldNotSellIfCurrentlyDoNotOwnTheStock()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 200m;

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(0d);

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldSell = await strategy.ShouldSellAsync(symbol);

//            Assert.False(shouldSell);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("Should not sell MSFT, because you do not own a position", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldNotSellIfCurrentlyHaveOpenSellOrder()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 200m;

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(1d);
//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>
//            {
//                new Order { Action = "SELL", Symbol = "MSFT" }
//            });

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldSell = await strategy.ShouldSellAsync(symbol);

//            Assert.False(shouldSell);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("Should not sell MSFT, because you currently have 1 open sell orders", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldNotSellIfOutsideUnitedStatesTradingHours()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeCapital = 200m;

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(1d);
//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>());


//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 0, 0), DateTimeKind.Local); // 10PM
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldSell = await strategy.ShouldSellAsync(symbol);

//            Assert.False(shouldSell);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("Should not sell MSFT, because the converted 01/06/2021 09:00:00 eastern time is not within the US trading hours", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);


//            fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 7, 6, 0, 0), DateTimeKind.Local); // 6AM
//            fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();

//            strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            shouldSell = await strategy.ShouldSellAsync(symbol);

//            Assert.False(shouldSell);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("Should not sell MSFT, because the converted 01/06/2021 17:00:00 eastern time is not within the US trading hours", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldNotSellIfMarketPriceIsAboveEma9()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 200m;

//            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
//            var fakeTradingDayInEst = TimeZoneInfo.ConvertTimeFromUtc(fakeTradingDay, easternZone);

//            Assert.Equal(new DateTime(2021, 1, 6, 9, 30, 0), fakeTradingDayInEst);

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(1d);
//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>());
//            brokerMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // 100 market price

//            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // 100 market price
//            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9, fakeTradingDayInEst)).ReturnsAsync(new TechnicalIndicator(99m, fakeTradingDayInEst)); // Below the market price

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldSell = await strategy.ShouldSellAsync(symbol);

//            Assert.False(shouldSell);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is above EMA(9) 99 at 01/06/2021 09:30:00", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is above open price 99", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task ShouldNotSellIfMarketPriceIsAboveOpenPrice()
//        {
//            var symbol = "MSFT";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();
//            var fakeCapital = 200m;

//            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
//            var fakeTradingDayInEst = TimeZoneInfo.ConvertTimeFromUtc(fakeTradingDay, easternZone);

//            Assert.Equal(new DateTime(2021, 1, 6, 9, 30, 0), fakeTradingDayInEst);

//            loggerMock.Setup(mock => mock.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

//            brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(1d);
//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>());
//            brokerMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // 100 market price, 99 opening price, indication of GREEN candle

//            marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(new Quote(99m, 100m, 101m, 98m)); // 100 market price, 99 opening price, indication of GREEN candle
//            marketDataMock.Setup(mock => mock.GetLatestEmaAsync(symbol, 9, fakeTradingDayInEst)).ReturnsAsync(new TechnicalIndicator(99m, fakeTradingDayInEst)); // Below the market price

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, fakeCapital, 2, 10);
//            var shouldSell = await strategy.ShouldSellAsync(symbol);

//            Assert.False(shouldSell);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is above EMA(9) 99 at 01/06/2021 09:30:00", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);

//            loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => string.Equals("MSFT price 100, is above open price 99", o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once);
//        }

//        [Fact]
//        public void ShouldCalculatePositionSizeCorrectly()
//        {
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeLocalTradingDay = DateTime.SpecifyKind(new DateTime(2021, 1, 6, 22, 30, 0), DateTimeKind.Local);
//            var fakeTradingDay = fakeLocalTradingDay.ToUniversalTime();

//            var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, 500m, 2, 10);
//            var positionSize = strategy.GetPositionSize(99.5m);

//            Assert.Equal(10m, positionSize.RiskPerTrade);
//            Assert.Equal(89.55m, positionSize.StopLossLevel);
//            Assert.Equal(1m, positionSize.SharesToBuy);

//            strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, fakeTradingDay, 250m, 2, 10);
//            positionSize = strategy.GetPositionSize(16.04m);

//            Assert.Equal(5m, positionSize.RiskPerTrade);
//            Assert.Equal(14.436m, positionSize.StopLossLevel);
//            Assert.Equal(3m, positionSize.SharesToBuy);
//        }

//        [Fact(Skip = "Move to integration testing")]
//        public async Task PerformanceAgainstRealMarketData()
//        {
//            var symbol = "MARA";
//            var loggerMock = new Mock<ILogger<Strategy101>>();
//            var brokerMock = new Mock<IBroker>();
//            var marketDataMock = new Mock<IMarketData>();
//            var fakeCapital = 200m;
//            var fakePosition = 0d;

//            brokerMock.Setup(mock => mock.GetOpenOrdersAsync(symbol)).ReturnsAsync(new List<Order>());

//            #region December 7, EMA (9) Setup

//            var december7 = new DateTime(2020, 12, 7, 0, 0, 0);
//            marketDataMock.SetupSequence(mock => mock.GetLatestEmaAsync(symbol, 9, It.Is<DateTime>(m => m.Date == december7.Date)))
//                .ReturnsAsync(new TechnicalIndicator(5.4607m, december7 + new TimeSpan(09, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.4726m, december7 + new TimeSpan(09, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5031m, december7 + new TimeSpan(10, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5115m, december7 + new TimeSpan(10, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5152m, december7 + new TimeSpan(10, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5267m, december7 + new TimeSpan(10, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5282m, december7 + new TimeSpan(11, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5345m, december7 + new TimeSpan(11, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5436m, december7 + new TimeSpan(11, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5510m, december7 + new TimeSpan(11, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5685m, december7 + new TimeSpan(12, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6068m, december7 + new TimeSpan(12, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6255m, december7 + new TimeSpan(12, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6594m, december7 + new TimeSpan(12, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6965m, december7 + new TimeSpan(13, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.7495m, december7 + new TimeSpan(13, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.7556m, december7 + new TimeSpan(13, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.7425m, december7 + new TimeSpan(13, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.7060m, december7 + new TimeSpan(14, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6784m, december7 + new TimeSpan(14, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6757m, december7 + new TimeSpan(14, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6686m, december7 + new TimeSpan(14, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6569m, december7 + new TimeSpan(15, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6215m, december7 + new TimeSpan(15, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6142m, december7 + new TimeSpan(15, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5953m, december7 + new TimeSpan(15, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5783m, december7 + new TimeSpan(16, 00, 0)));

//            #endregion

//            #region December 8, EMA (9) Setup

//            var december8 = new DateTime(2020, 12, 8, 0, 0, 0);
//            marketDataMock.SetupSequence(mock => mock.GetLatestEmaAsync(symbol, 9, It.Is<DateTime>(m => m.Date == december8.Date)))
//                .ReturnsAsync(new TechnicalIndicator(5.3856m, december8 + new TimeSpan(09, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.3755m, december8 + new TimeSpan(09, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.3412m, december8 + new TimeSpan(10, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.3180m, december8 + new TimeSpan(10, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2874m, december8 + new TimeSpan(10, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2729m, december8 + new TimeSpan(10, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2533m, december8 + new TimeSpan(11, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2306m, december8 + new TimeSpan(11, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2305m, december8 + new TimeSpan(11, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2301m, december8 + new TimeSpan(11, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2341m, december8 + new TimeSpan(12, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2363m, december8 + new TimeSpan(12, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2510m, december8 + new TimeSpan(12, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2578m, december8 + new TimeSpan(12, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2811m, december8 + new TimeSpan(13, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2880m, december8 + new TimeSpan(13, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2974m, december8 + new TimeSpan(13, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2969m, december8 + new TimeSpan(13, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2916m, december8 + new TimeSpan(14, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2872m, december8 + new TimeSpan(14, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2858m, december8 + new TimeSpan(14, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2796m, december8 + new TimeSpan(14, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2737m, december8 + new TimeSpan(15, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2660m, december8 + new TimeSpan(15, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2443m, december8 + new TimeSpan(15, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2235m, december8 + new TimeSpan(15, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2128m, december8 + new TimeSpan(16, 00, 0)));

//            #endregion

//            #region December 9, EMA (9) Setup

//            var december9 = new DateTime(2020, 12, 9, 0, 0, 0);
//            marketDataMock.SetupSequence(mock => mock.GetLatestEmaAsync(symbol, 9, It.Is<DateTime>(m => m.Date == december9.Date)))
//                .ReturnsAsync(new TechnicalIndicator(5.0843m, december9 + new TimeSpan(09, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1333m, december9 + new TimeSpan(09, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1637m, december9 + new TimeSpan(10, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1519m, december9 + new TimeSpan(10, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1455m, december9 + new TimeSpan(10, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1345m, december9 + new TimeSpan(10, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1395m, december9 + new TimeSpan(11, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1355m, december9 + new TimeSpan(11, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1424m, december9 + new TimeSpan(11, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1469m, december9 + new TimeSpan(11, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1596m, december9 + new TimeSpan(12, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1606m, december9 + new TimeSpan(12, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1545m, december9 + new TimeSpan(12, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1332m, december9 + new TimeSpan(12, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1166m, december9 + new TimeSpan(13, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1013m, december9 + new TimeSpan(13, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0629m, december9 + new TimeSpan(13, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0183m, december9 + new TimeSpan(13, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9305m, december9 + new TimeSpan(14, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8843m, december9 + new TimeSpan(14, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8824m, december9 + new TimeSpan(14, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8640m, december9 + new TimeSpan(14, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8586m, december9 + new TimeSpan(15, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8608m, december9 + new TimeSpan(15, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8463m, december9 + new TimeSpan(15, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8331m, december9 + new TimeSpan(15, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8204m, december9 + new TimeSpan(16, 00, 0)));

//            #endregion

//            #region December 10, EMA (9) Setup

//            var december10 = new DateTime(2020, 12, 10, 0, 0, 0);
//            marketDataMock.SetupSequence(mock => mock.GetLatestEmaAsync(symbol, 9, It.Is<DateTime>(m => m.Date == december10.Date)))
//                .ReturnsAsync(new TechnicalIndicator(4.6165m, december10 + new TimeSpan(09, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.6192m, december10 + new TimeSpan(09, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.6653m, december10 + new TimeSpan(10, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.6643m, december10 + new TimeSpan(10, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.6834m, december10 + new TimeSpan(10, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7097m, december10 + new TimeSpan(10, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7378m, december10 + new TimeSpan(11, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7545m, december10 + new TimeSpan(11, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7973m, december10 + new TimeSpan(11, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8221m, december10 + new TimeSpan(11, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8394m, december10 + new TimeSpan(12, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8475m, december10 + new TimeSpan(12, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8420m, december10 + new TimeSpan(12, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8526m, december10 + new TimeSpan(12, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8589m, december10 + new TimeSpan(13, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8731m, december10 + new TimeSpan(13, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8845m, december10 + new TimeSpan(13, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8856m, december10 + new TimeSpan(13, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8935m, december10 + new TimeSpan(14, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9038m, december10 + new TimeSpan(14, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9200m, december10 + new TimeSpan(14, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9680m, december10 + new TimeSpan(14, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9974m, december10 + new TimeSpan(15, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0339m, december10 + new TimeSpan(15, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0751m, december10 + new TimeSpan(15, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1011m, december10 + new TimeSpan(15, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1189m, december10 + new TimeSpan(16, 00, 0)));

//            #endregion

//            #region December 11, EMA (9) Setup

//            var december11 = new DateTime(2020, 12, 11, 0, 0, 0);
//            marketDataMock.SetupSequence(mock => mock.GetLatestEmaAsync(symbol, 9, It.Is<DateTime>(m => m.Date == december11.Date)))
//                .ReturnsAsync(new TechnicalIndicator(4.9262m, december11 + new TimeSpan(09, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9209m, december11 + new TimeSpan(09, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9467m, december11 + new TimeSpan(10, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9761m, december11 + new TimeSpan(10, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9899m, december11 + new TimeSpan(10, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9889m, december11 + new TimeSpan(10, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9821m, december11 + new TimeSpan(11, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9727m, december11 + new TimeSpan(11, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9502m, december11 + new TimeSpan(11, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9202m, december11 + new TimeSpan(11, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9041m, december11 + new TimeSpan(12, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8736m, december11 + new TimeSpan(12, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8449m, december11 + new TimeSpan(12, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8177m, december11 + new TimeSpan(12, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8163m, december11 + new TimeSpan(13, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8080m, december11 + new TimeSpan(13, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8043m, december11 + new TimeSpan(13, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8051m, december11 + new TimeSpan(13, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8061m, december11 + new TimeSpan(14, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8088m, december11 + new TimeSpan(14, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8092m, december11 + new TimeSpan(14, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8015m, december11 + new TimeSpan(14, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7991m, december11 + new TimeSpan(15, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7953m, december11 + new TimeSpan(15, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7961m, december11 + new TimeSpan(15, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7889m, december11 + new TimeSpan(15, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7811m, december11 + new TimeSpan(16, 00, 0)));

//            #endregion

//            #region December 7, EMA (20) Setup

//            marketDataMock.SetupSequence(mock => mock.GetLatestEmaAsync(symbol, 20, It.Is<DateTime>(m => m.Date == december7.Date)))
//                .ReturnsAsync(new TechnicalIndicator(5.4841m, december7 + new TimeSpan(09, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.4875m, december7 + new TimeSpan(09, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5006m, december7 + new TimeSpan(10, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5049m, december7 + new TimeSpan(10, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5073m, december7 + new TimeSpan(10, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5135m, december7 + new TimeSpan(10, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5155m, december7 + new TimeSpan(11, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5197m, december7 + new TimeSpan(11, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5254m, december7 + new TimeSpan(11, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5307m, december7 + new TimeSpan(11, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5410m, december7 + new TimeSpan(12, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5618m, december7 + new TimeSpan(12, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5750m, december7 + new TimeSpan(12, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.5959m, december7 + new TimeSpan(12, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6197m, december7 + new TimeSpan(13, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6522m, december7 + new TimeSpan(13, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6644m, december7 + new TimeSpan(13, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6668m, december7 + new TimeSpan(13, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6567m, december7 + new TimeSpan(14, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6482m, december7 + new TimeSpan(14, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6498m, december7 + new TimeSpan(14, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6489m, december7 + new TimeSpan(14, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6452m, december7 + new TimeSpan(15, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6294m, december7 + new TimeSpan(15, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6252m, december7 + new TimeSpan(15, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6152m, december7 + new TimeSpan(15, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.6052m, december7 + new TimeSpan(16, 00, 0)));

//            #endregion

//            #region December 8, EMA (20) Setup

//            marketDataMock.SetupSequence(mock => mock.GetLatestEmaAsync(symbol, 20, It.Is<DateTime>(m => m.Date == december8.Date)))
//                .ReturnsAsync(new TechnicalIndicator(5.4131m, december8 + new TimeSpan(09, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.4056m, december8 + new TimeSpan(09, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.3864m, december8 + new TimeSpan(10, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.3711m, december8 + new TimeSpan(10, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.3515m, december8 + new TimeSpan(10, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.3385m, december8 + new TimeSpan(10, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.3229m, december8 + new TimeSpan(11, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.3054m, december8 + new TimeSpan(11, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2983m, december8 + new TimeSpan(11, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2916m, december8 + new TimeSpan(11, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2877m, december8 + new TimeSpan(12, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2836m, december8 + new TimeSpan(12, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2861m, december8 + new TimeSpan(12, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2860m, december8 + new TimeSpan(12, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2944m, december8 + new TimeSpan(13, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2964m, december8 + new TimeSpan(13, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.3001m, december8 + new TimeSpan(13, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2996m, december8 + new TimeSpan(13, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2968m, december8 + new TimeSpan(14, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2942m, december8 + new TimeSpan(14, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2929m, december8 + new TimeSpan(14, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2893m, december8 + new TimeSpan(14, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2855m, december8 + new TimeSpan(15, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2807m, december8 + new TimeSpan(15, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2690m, december8 + new TimeSpan(15, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2567m, december8 + new TimeSpan(15, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.2485m, december8 + new TimeSpan(16, 00, 0)));

//            #endregion

//            #region December 9, EMA (20) Setup

//            marketDataMock.SetupSequence(mock => mock.GetLatestEmaAsync(symbol, 20, It.Is<DateTime>(m => m.Date == december9.Date)))
//                .ReturnsAsync(new TechnicalIndicator(5.0320m, december9 + new TimeSpan(09, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0603m, december9 + new TimeSpan(09, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0817m, december9 + new TimeSpan(10, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0839m, december9 + new TimeSpan(10, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0873m, december9 + new TimeSpan(10, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0876m, december9 + new TimeSpan(10, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0945m, december9 + new TimeSpan(11, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0969m, december9 + new TimeSpan(11, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1038m, december9 + new TimeSpan(11, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1097m, december9 + new TimeSpan(11, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1192m, december9 + new TimeSpan(12, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1236m, december9 + new TimeSpan(12, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1242m, december9 + new TimeSpan(12, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1169m, december9 + new TimeSpan(12, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1106m, december9 + new TimeSpan(13, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.1038m, december9 + new TimeSpan(13, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0853m, december9 + new TimeSpan(13, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0620m, december9 + new TimeSpan(13, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0160m, december9 + new TimeSpan(14, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9858m, december9 + new TimeSpan(14, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9753m, december9 + new TimeSpan(14, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9577m, december9 + new TimeSpan(14, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9462m, december9 + new TimeSpan(15, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9389m, december9 + new TimeSpan(15, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9245m, december9 + new TimeSpan(15, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9108m, december9 + new TimeSpan(15, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8974m, december9 + new TimeSpan(16, 00, 0)));

//            #endregion

//            #region December 10, EMA (20) Setup

//            marketDataMock.SetupSequence(mock => mock.GetLatestEmaAsync(symbol, 20, It.Is<DateTime>(m => m.Date == december10.Date)))
//                .ReturnsAsync(new TechnicalIndicator(4.7038m, december10 + new TimeSpan(09, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.6968m, december10 + new TimeSpan(09, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7114m, december10 + new TimeSpan(10, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7065m, december10 + new TimeSpan(10, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7116m, december10 + new TimeSpan(10, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7214m, december10 + new TimeSpan(10, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7337m, december10 + new TimeSpan(11, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7421m, december10 + new TimeSpan(11, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7636m, december10 + new TimeSpan(11, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7786m, december10 + new TimeSpan(11, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7910m, december10 + new TimeSpan(12, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.7995m, december10 + new TimeSpan(12, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8014m, december10 + new TimeSpan(12, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8104m, december10 + new TimeSpan(12, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8174m, december10 + new TimeSpan(13, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8281m, december10 + new TimeSpan(13, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8378m, december10 + new TimeSpan(13, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8428m, december10 + new TimeSpan(13, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8506m, december10 + new TimeSpan(14, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8596m, december10 + new TimeSpan(14, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8715m, december10 + new TimeSpan(14, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8990m, december10 + new TimeSpan(14, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9196m, december10 + new TimeSpan(15, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9444m, december10 + new TimeSpan(15, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9725m, december10 + new TimeSpan(15, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9947m, december10 + new TimeSpan(15, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(5.0133m, december10 + new TimeSpan(16, 00, 0)));

//            #endregion

//            #region December 11, EMA (20) Setup

//            marketDataMock.SetupSequence(mock => mock.GetLatestEmaAsync(symbol, 20, It.Is<DateTime>(m => m.Date == december11.Date)))
//                .ReturnsAsync(new TechnicalIndicator(4.9030m, december11 + new TimeSpan(09, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9027m, december11 + new TimeSpan(09, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9167m, december11 + new TimeSpan(10, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9336m, december11 + new TimeSpan(10, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9442m, december11 + new TimeSpan(10, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9481m, december11 + new TimeSpan(10, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9487m, december11 + new TimeSpan(11, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9474m, december11 + new TimeSpan(11, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9391m, december11 + new TimeSpan(11, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9259m, december11 + new TimeSpan(11, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9177m, december11 + new TimeSpan(12, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.9019m, december11 + new TimeSpan(12, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8855m, december11 + new TimeSpan(12, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8687m, december11 + new TimeSpan(12, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8632m, december11 + new TimeSpan(13, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8548m, december11 + new TimeSpan(13, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8486m, december11 + new TimeSpan(13, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8447m, december11 + new TimeSpan(13, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8414m, december11 + new TimeSpan(14, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8393m, december11 + new TimeSpan(14, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8366m, december11 + new TimeSpan(14, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8303m, december11 + new TimeSpan(14, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8264m, december11 + new TimeSpan(15, 00, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8220m, december11 + new TimeSpan(15, 15, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8199m, december11 + new TimeSpan(15, 30, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8142m, december11 + new TimeSpan(15, 45, 0)))
//                .ReturnsAsync(new TechnicalIndicator(4.8081m, december11 + new TimeSpan(16, 00, 0)));

//            #endregion

//            var quotes = new List<Quote>
//            {
//                new Quote(5.4m, 5.51m, 6.04m, 5.39m), // December 7, Monday
//                new Quote(5.42m, 5.18m, 5.49m, 5.07m),
//                new Quote(5.23m, 4.78m, 5.34m, 4.52m),
//                new Quote(4.51m, 5.22m, 5.29m, 4.5m),
//                new Quote(4.98m, 4.75m, 5.17m, 4.66m) // December 11, Friday
//            };
//            var start = new DateTime(2020, 12, 7, 0, 0, 0);
//            var end = new DateTime(2020, 12, 11, 0, 0, 0);
//            var day = 0;
//            var trades = new List<string>();

//            while (start < end)
//            {
//                var tradingDay = start + new TimeSpan(22, 30, 0);

//                brokerMock.Setup(mock => mock.GetAvailableFundsAsync(Currency.USD)).ReturnsAsync(fakeCapital);
//                brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(fakePosition);

//                marketDataMock.Setup(mock => mock.GetQuoteAsync(symbol)).ReturnsAsync(quotes[day]);

//                var strategy = new Strategy101(loggerMock.Object, brokerMock.Object, marketDataMock.Object, tradingDay.ToUniversalTime(), fakeCapital, 2, 10);

//                for (int i = 0; i < 25; i++) // simulate 25 moving averages
//                {
//                    if (await strategy.ShouldBuyAsync(symbol))
//                    {
//                        var quote = await marketDataMock.Object.GetQuoteAsync(symbol);
//                        var buyPrice = (quote.High + quote.Low) / 2m;
//                        var position = strategy.GetPositionSize(buyPrice);
//                        fakePosition = Convert.ToDouble(position.SharesToBuy);
//                        fakeCapital -= position.SharesToBuy * buyPrice;

//                        brokerMock.Setup(mock => mock.GetAvailableFundsAsync(Currency.USD)).ReturnsAsync(fakeCapital);
//                        brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(fakePosition);

//                        trades.Add($"Bought {fakePosition} for {position.SharesToBuy * buyPrice} - {tradingDay}");
//                    }
//                    else if (await strategy.ShouldSellAsync(symbol))
//                    {
//                        var quote = await marketDataMock.Object.GetQuoteAsync(symbol);
//                        var midPrice = (quote.High + quote.Low) / 2;

//                        trades.Add($"Selling {fakePosition} for {Convert.ToDecimal(fakePosition) * midPrice} - {tradingDay}");

//                        fakeCapital += Convert.ToDecimal(fakePosition) * midPrice;
//                        fakePosition = 0d;

//                        brokerMock.Setup(mock => mock.GetAvailableFundsAsync(Currency.USD)).ReturnsAsync(fakeCapital);
//                        brokerMock.Setup(mock => mock.GetPositionsAsync(symbol)).ReturnsAsync(fakePosition);
//                    }
//                }                

//                start = start.AddDays(1);
//                day += 1;
//            }

//            Assert.True(trades.Count >= 0);
//        }
//    }
//}
