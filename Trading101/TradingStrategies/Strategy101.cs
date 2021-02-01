//using AutoFinance.Broker.InteractiveBrokers.Constants;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using TradingStrategies.Constants;
//using TradingStrategies.Wrappers;

//namespace TradingStrategies
//{
//    public class Strategy101 : IStrategy
//    {
//        private readonly ILogger<Strategy101> logger;
//        private readonly IBroker broker;
//        private readonly IMarketData marketData;
//        private readonly DateTime easternDateTime;
//        private readonly decimal capitalPerStock;
//        private readonly int riskPercentage;
//        private readonly int stopLossPercentage;

//        public Strategy101(ILogger<Strategy101> logger,
//            IBroker broker,
//            IMarketData marketData,
//            DateTime tradingDayTimeUtc,
//            decimal capitalPerStock,
//            int riskPercentage,
//            int stopLossPercentage)
//        {
//            this.logger = logger;
//            this.broker = broker;
//            this.marketData = marketData;
//            this.capitalPerStock = capitalPerStock;
//            this.riskPercentage = riskPercentage;
//            this.stopLossPercentage = stopLossPercentage;

//            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");            
//            easternDateTime = TimeZoneInfo.ConvertTimeFromUtc(tradingDayTimeUtc, easternZone);

//            /*
//             * Converting Yahoo Finance - regularMarketTime
//             * var utc = DateTimeOffset.FromUnixTimeSeconds(1611325801).UtcDateTime;
//var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
//var est = TimeZoneInfo.ConvertTimeFromUtc(utc, easternZone);
//             */
//        }

//        public async Task<bool> ShouldBuyAsync(string symbol)
//        {
//            try
//            {
//                var quote = await marketData.GetQuoteAsync(symbol);
//                var buyPrice = (quote.High + quote.Low) / 2;
//                var positionSize = GetPositionSize(buyPrice);
//                var fundsNeeded = positionSize.SharesToBuy * buyPrice;

//                var availableFunds = await broker.GetAvailableFundsAsync(Currency.USD);
//                if (availableFunds >= fundsNeeded)
//                {
//                    var positions = await broker.GetPositionsAsync(symbol);
//                    if (positions <= 0)
//                    {
//                        var openOrders = await broker.GetOpenOrdersAsync(symbol);
//                        var openBuyOrders = openOrders.Where(order => order.Action == TwsOrderActions.Buy).ToList();
//                        if (openBuyOrders.Count == 0)
//                        {
//                            if (IsTradingDateTimeWithinUsTradingHours())
//                            {
//                                if (await IsGlobalQuoteAboveEMA9Async(symbol, quote) && await IsGlobalQuoteAboveEMA20Async(symbol, quote))
//                                    return true; // Buy
//                            }
//                            else
//                                logger.LogInformation("Should not buy {0}, because the converted {1} eastern time is not within the US trading hours", symbol, easternDateTime);
//                        }
//                        else
//                            logger.LogInformation("Should not buy {0}, because you currently have {1} open buy orders", symbol, openBuyOrders.Count);
//                    }
//                    else
//                        logger.LogInformation("Should not buy {0}, because you currently own a position of {1}", symbol, positions);
//                }
//                else
//                    logger.LogInformation("Should not buy {0}, because available funds {1} is less than {2}", symbol, availableFunds, fundsNeeded);
//            }
//            catch (Exception ex)
//            {
//                if (ex.Message.Contains("Our standard API call frequency is"))
//                    logger.LogError(new ArgumentException("Max API call frequency reached."), "Error occurred executing ShouldBuyAsync().");
//                else
//                    logger.LogError(ex, "Error occurred executing ShouldBuyAsync().");
//            }

//            return false;
//        }

//        public async Task<bool> ShouldSellAsync(string symbol)
//        {
//            try
//            {
//                var positions = await broker.GetPositionsAsync(symbol);
//                if (positions > 0)
//                {
//                    var openOrders = await broker.GetOpenOrdersAsync(symbol);
//                    var openBuyOrders = openOrders.Where(order => order.Action == TwsOrderActions.Sell).ToList();
//                    if (openBuyOrders.Count == 0)
//                    {
//                        if (IsTradingDateTimeWithinUsTradingHours())
//                        {
//                            var quote = await marketData.GetQuoteAsync(symbol);
//                            if (await IsGlobalQuoteBelowEMA9OrTheOpeningPriceAsync(symbol, quote))
//                                return true;
//                        }
//                        else
//                            logger.LogInformation("Should not sell {0}, because the converted {1} eastern time is not within the US trading hours", symbol, easternDateTime);
//                    }
//                    else
//                        logger.LogInformation("Should not sell {0}, because you currently have {1} open sell orders", symbol, openBuyOrders.Count);
//                }
//                else
//                    logger.LogInformation("Should not sell {0}, because you do not own a position", symbol, positions);
//            }
//            catch (Exception ex)
//            {
//                if (ex.Message.Contains("Our standard API call frequency is"))
//                    logger.LogError(new ArgumentException("Max API call frequency reached."), "Error occurred executing ShouldBuyAsync().");
//                else
//                    logger.LogError(ex, "Error occurred executing ShouldBuyAsync().");
//            }

//            return false;
//        }

//        private bool IsTradingDateTimeWithinUsTradingHours()
//        {
//            if (UsTradingHours.Weekdays.Contains(easternDateTime.DayOfWeek)
//                && easternDateTime.TimeOfDay >= UsTradingHours.Start
//                && easternDateTime.TimeOfDay <= UsTradingHours.End)
//                return true;

//            return false;
//        }

//        private async Task<bool> IsGlobalQuoteAboveEMA9Async(string symbol, Quote quote)
//        {
//            var ema9 = await marketData.GetLatestEmaAsync(symbol, 9);

//            if (ema9 == null)
//            {
//                logger.LogInformation("{0} EMA(9) data is not available", symbol);
//                return false;
//            }
//            else if (quote.Close > ema9.Price)
//            {
//                logger.LogInformation("{0} price {1}, is above EMA(9) {2} at {3}", symbol, quote.Close, ema9.Price, ema9.TradingTime);
//                return true;
//            }
//            else
//            {
//                logger.LogInformation("{0} price {1}, is below EMA(9) {2} at {3}", symbol, quote.Close, ema9.Price, ema9.TradingTime);
//                return false;
//            }
//        }

//        private async Task<bool> IsGlobalQuoteAboveEMA20Async(string symbol, Quote quote)
//        {
//            var ema20 = await marketData.GetLatestEmaAsync(symbol, 20, easternDateTime);

//            if (ema20 == null)
//            {
//                logger.LogInformation("{0} EMA(20) data is not available", symbol);
//                return false;
//            }
//            else if(quote.Close > ema20.Price)
//            {
//                logger.LogInformation("{0} price {1}, is above EMA(20) {2} at {3}", symbol, quote.Close, ema20.Price, ema20.TradingTime);
//                return true;
//            }
//            else
//            {
//                logger.LogInformation("{0} price {1}, is below EMA(20) {2} at {3}", symbol, quote.Close, ema20.Price, ema20.TradingTime);
//                return false;
//            }
//        }

//        private async Task<bool> IsGlobalQuoteBelowEMA9OrTheOpeningPriceAsync(string symbol, Quote quote)
//        {
//            var ema9 = await marketData.GetLatestEmaAsync(symbol, 9, easternDateTime);

//            if (ema9 == null)
//                logger.LogInformation("{0} EMA(9) data is not available", symbol);
//            else if(quote.Close < ema9.Price)
//                logger.LogInformation("{0} price {1}, is below EMA(9) {2} at {3}", symbol, quote.Close, ema9.Price, ema9.TradingTime);
//            else
//                logger.LogInformation("{0} price {1}, is above EMA(9) {2} at {3}", symbol, quote.Close, ema9.Price, ema9.TradingTime);

//            if (quote.Close < quote.Open)
//                logger.LogInformation("{0} price {1}, is below open price {2}", symbol, quote.Close, quote.Open);
//            else
//                logger.LogInformation("{0} price {1}, is above open price {2}", symbol, quote.Close, quote.Open);

//            return (quote.Close < ema9?.Price) || (quote.Close < quote.Open);
//        }

//        public (decimal StopLossLevel, decimal RiskPerTrade, decimal SharesToBuy) GetPositionSize(decimal buyPrice)
//        {
//            var riskPerTrade = capitalPerStock * (riskPercentage / 100m);
//            var stopLossLevel = buyPrice - (buyPrice * (stopLossPercentage / 100m));

//            var sharesToBuy = riskPerTrade / (buyPrice - stopLossLevel);

//            var quantityToBuy = Math.Round(sharesToBuy, 0);

//            return (stopLossLevel, riskPerTrade, quantityToBuy);
//        }

//        public Task<bool> IsBullish(string symbol)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<bool> IsBearish(string symbol)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
