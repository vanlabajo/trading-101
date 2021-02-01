using System;

namespace TradingStrategies
{
    public class PositionSizing
    {
        public static (decimal Quantity, decimal TargetProfit) GetPositionSize(decimal capital, decimal riskPercentage, decimal targetRewardPercentage, decimal entryPrice, decimal stopLoss)
        {
            var risk = riskPercentage / 100;
            var capitalRisk = capital * risk;
            var tradeRisk = entryPrice - stopLoss;
            var position = capitalRisk / tradeRisk;
            var quantity = Math.Round(position, 0);

            var tradeReward = tradeRisk * targetRewardPercentage;
            var targetProfit = entryPrice + tradeReward;

            return (Math.Abs(quantity), targetProfit);
        }
    }
}
