using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TradingStrategies
{
    public class LongStrategy103 : ILongStrategy
    {
        private readonly ILogger<LongStrategy103> logger;
        private readonly IMarketData marketData;

        private Dictionary<string, (decimal highestHigh,
            decimal lowestLow,
            decimal ema9,
            decimal ema20,
            decimal sma50,
            decimal sma200)> symbols;

        public LongStrategy103(ILogger<LongStrategy103> logger,
            IMarketData marketData)
        {
            this.logger = logger;
            this.marketData = marketData;

            symbols = new Dictionary<string, (decimal highestHigh, decimal lowestLow, decimal ema9, decimal ema20, decimal sma50, decimal sma200)>();
        }

        public bool IsBullish(string symbol, decimal entryPrice)
        {
            var result = Task.Run<Task>(async () => await GetMarketDataAsync(symbol)).ConfigureAwait(false);

            _ = result.GetAwaiter().GetResult();

            var signals = 0;

            // weak price crossover signal
            if (entryPrice > symbols[symbol].ema9)
            {
                signals += 1;
            }
            if (entryPrice > symbols[symbol].ema20)
            {
                signals += 1;
            }
            // strong price crossover signal
            if (entryPrice > symbols[symbol].sma50)
            {
                signals += 1;
            }
            if (entryPrice > symbols[symbol].sma200)
            {
                signals += 1;
            }

            if (signals >= 3) return true;
            return false;
        }

        public bool ShouldTrade(string symbol, decimal targetReward)
        {
            if (symbols[symbol].highestHigh < targetReward)
            {
                logger.LogInformation("{0} - Should not trade, target reward {1} is higher than highest high {2}", symbol, targetReward, symbols[symbol].highestHigh);
                return false;
            }

            return true;
        }

        private async Task GetMarketDataAsync(string symbol)
        {
            logger.LogInformation("{0} - Getting market data", symbol);

            var (HighestHigh, LowestLow) = await marketData.GetIntradayHighestHighLowestLowAsync(symbol);
            var intradayHighestHigh = HighestHigh;
            var intradayLowestLow = LowestLow;
            logger.LogInformation("{0} - Intraday Highest High: {1}, Lowest Low: {2}", symbol, HighestHigh, LowestLow);

            var ema9 = await marketData.GetLatestEmaAsync(symbol, 9);
            var ema20 = await marketData.GetLatestEmaAsync(symbol, 20);
            var sma50 = await marketData.GetLatestSmaAsync(symbol, 50);
            var sma200 = await marketData.GetLatestSmaAsync(symbol, 200);
            logger.LogInformation("{0} - Latest EMA(9): {1}, EMA(20): {2}, SMA(50): {3}, SMA(200): {4}", symbol, ema9, ema20, sma50, sma200);

            symbols[symbol] = (intradayHighestHigh, intradayLowestLow, ema9, ema20, sma50, sma200);
        }
    }
}
