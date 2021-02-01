using System.Collections.Generic;
using System.Threading.Tasks;

namespace TradingStrategies
{
    public interface IBroker
    {
        Task<decimal> GetAvailableFundsAsync(string currency);
        Task<int> PlaceBracketOrderAsync(string symbol, string entryAction, decimal quantity, decimal entryPrice, decimal stopPrice, decimal targetReward);
        Task<List<int>> GetOpenOrderIdsAsync(string symbol);
        Task<bool> CancelOrderAsync(int orderId);
    }
}
