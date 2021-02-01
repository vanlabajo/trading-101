using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TradingStrategies
{
    public class Strategy102 : IStrategy
    {
        private readonly string symbol;
        private readonly ILogger<Strategy102> logger;
        private readonly IMarketData marketData;

        private Quote quote;
        private decimal intradayHighestHigh;
        private decimal intradayLowestLow;
        private decimal ema9;
        private decimal ema20;
        private decimal sma50;
        private decimal sma200;

        public Strategy102(string symbol,
            ILogger<Strategy102> logger,
            IMarketData marketData)
        {
            this.symbol = symbol;
            this.logger = logger;
            this.marketData = marketData;
        }

        public async Task GetMarketData()
        {
            logger.LogInformation("{0} - Getting market data", symbol);
            quote = await marketData.GetQuoteAsync(symbol);
            logger.LogInformation("{0} - Close: {1}, Open: {2}, Low: {3}, High: {4}", symbol, quote.Close, quote.Open, quote.Low, quote.High);

            var (HighestHigh, LowestLow) = await marketData.GetIntradayHighestHighLowestLowAsync(symbol);
            intradayHighestHigh = HighestHigh;
            intradayLowestLow = LowestLow;
            logger.LogInformation("{0} - Intraday Highest High: {1}, Lowest Low: {2}", symbol, HighestHigh, LowestLow);

            ema9 = await marketData.GetLatestEmaAsync(symbol, 9);
            ema20 = await marketData.GetLatestEmaAsync(symbol, 20);
            sma50 = await marketData.GetLatestSmaAsync(symbol, 50);
            sma200 = await marketData.GetLatestSmaAsync(symbol, 200);
            logger.LogInformation("{0} - Latest EMA(9): {1}, EMA(20): {2}, SMA(50): {3}, SMA(200): {4}", symbol, ema9, ema20, sma50, sma200);
        }

        public bool IsBearish()
        {
            var signals = 0;

            // weak price crossover signal
            if (quote.Close < ema9)
            {
                signals += 1;
            }
            if (quote.Close < ema20)
            {
                signals += 1;
            }
            // strong price crossover signal
            if (quote.Close < sma50)
            {
                signals += 1;
            }
            if (quote.Close < sma200)
            {
                signals += 1;
            }

            if (signals >= 3) return true;
            return false;
        }

        public bool IsBullish()
        {
            var signals = 0;

            // weak price crossover signal
            if (quote.Close > ema9)
            {
                signals += 1;
            }
            if (quote.Close > ema20)
            {
                signals += 1;
            }
            // strong price crossover signal
            if (quote.Close > sma50)
            {
                signals += 1;
            }
            if (quote.Close > sma200)
            {
                signals += 1;
            }

            if (signals >= 3) return true;
            return false;
        }

        public bool ShouldTrade(decimal targetReward)
        {
            if (IsBearish())
            {
                if (intradayLowestLow > targetReward)
                {
                    logger.LogInformation("{0} - Should not trade, target reward {1} is lower than lowest low {2}", symbol, targetReward, intradayLowestLow);
                    return false;
                }
            }
            else if (IsBullish())
            {
                if (intradayHighestHigh < targetReward)
                {
                    logger.LogInformation("{0} - Should not trade, target reward {1} is higher than highest high {2}", symbol, targetReward, intradayHighestHigh);
                    return false;
                }
            }

            return true;
        }

        public Quote GetQuote() => quote;
    }
}
