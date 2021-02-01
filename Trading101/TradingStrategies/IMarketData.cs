using System.Threading.Tasks;

namespace TradingStrategies
{
    public interface IMarketData
    {
        Task<Quote> GetQuoteAsync(string symbol);
        Task<decimal> GetLatestEmaAsync(string symbol, int timePeriod);
        Task<decimal> GetLatestSmaAsync(string symbol, int timePeriod);
        Task<(decimal HighestHigh, decimal LowestLow)> GetIntradayHighestHighLowestLowAsync(string symbol);
    }
}
