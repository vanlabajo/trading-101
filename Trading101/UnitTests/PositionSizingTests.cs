using TradingStrategies;
using Xunit;

namespace UnitTests
{
    public class PositionSizingTests
    {
        [Fact]
        public void ShouldGetCorrectPositionSize()
        {
            var capital = 500m;
            var riskPercentage = 1m; // 1 percent
            var targetRewardPercentage = 1.5m; // 1.5:1 reward risk ratio
            var entryPrice = 5.2m;
            var stopLoss = 5m;

            var positionSize = PositionSizing.GetPositionSize(capital, riskPercentage, targetRewardPercentage, entryPrice, stopLoss);

            Assert.Equal(25m, positionSize.Quantity);
            Assert.Equal(5.5m, positionSize.TargetProfit);
        }


        [Theory]
        [InlineData(5.2, 5.4, 25, 4.9)]
        [InlineData(6.78, 6.58, 25, 7.08)]
        [InlineData(13.5, 13.3, 25, 13.8)]
        [InlineData(16.4, 16.6, 25, 16.1)]
        public void ShouldGetCorrectPositionSizeTheory(decimal entryPrice, decimal stopLoss, decimal expectedQuantity, decimal expectedTargetProfit)
        {
            var capital = 500m;
            var riskPercentage = 1m; // 1 percent
            var targetRewardPercentage = 1.5m; // 1.5:1 reward risk ratio

            var positionSize = PositionSizing.GetPositionSize(capital, riskPercentage, targetRewardPercentage, entryPrice, stopLoss);

            Assert.Equal(expectedQuantity, positionSize.Quantity);
            Assert.Equal(expectedTargetProfit, positionSize.TargetProfit);
        }
    }
}
