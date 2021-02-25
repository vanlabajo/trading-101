using System.Collections.Generic;
using System.Threading.Tasks;

namespace TradingStrategies
{
    public interface IBroker
    {
        Task<decimal> GetAvailableFundsAsync();
        Task<int> PlaceBracketOrderAsync(string symbol, string entryAction, decimal quantity, decimal entryPrice, decimal targetReward);
        Task<int> PlaceBracketOrderAsync(string symbol, string entryAction, decimal quantity, decimal entryPrice, decimal targetReward, decimal stopPrice);
        Task<List<int>> GetOpenOrderIdsAsync(string symbol);
        Task<bool> CancelOrderAsync(int orderId);
    }
}
