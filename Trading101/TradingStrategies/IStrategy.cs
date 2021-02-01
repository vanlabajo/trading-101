namespace TradingStrategies
{
    public interface IStrategy
    {
        bool IsBullish();
        bool IsBearish();
        bool ShouldTrade(decimal targetReward);
    }
}
