using System.Threading.Tasks;

namespace TradingStrategies
{
    public interface IStrategy
    {
        bool IsBullish();
        bool IsBearish();
        bool ShouldTrade(decimal targetReward);
    }

    public interface ILongStrategy
    {
        Task GetMarketDataAsync(string symbol);
        bool IsBullish(string symbol, decimal entryPrice);
        bool ShouldTrade(string symbol, decimal targetReward);
    }
}
