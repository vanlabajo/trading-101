using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using TradingStrategies.Wrappers;
using Xunit;

namespace IntegrationTests
{
    public class AvMarketDataTests
    {
        private string avApiKey;
        private ServiceProvider serviceProvider;

        public AvMarketDataTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<AvMarketDataTests>()
                .Build();

            var services = new ServiceCollection();
            services.AddHttpClient();

            serviceProvider = services.BuildServiceProvider();

            avApiKey = configuration["AvApiKey"];
        }

        [Fact]
        public async Task GetQuoteAsync()
        {
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var marketData = new AvMarketData(avApiKey, factory);

            var quote = await marketData.GetQuoteAsync("MSFT");

            Assert.NotNull(quote);
        }

        [Fact]
        public async Task GetLatestEmaAsync()
        {
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var marketData = new AvMarketData(avApiKey, factory);

            var ema9 = await marketData.GetLatestEmaAsync("MSFT", 9);

            Assert.True(ema9 > 0);
        }

        [Fact]
        public async Task GetLatestSmaAsync()
        {
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var marketData = new AvMarketData(avApiKey, factory);

            var sma9 = await marketData.GetLatestSmaAsync("MSFT", 9);

            Assert.True(sma9 > 0);
        }

        [Fact]
        public async Task GetIntradayHighestHighLowestLowAsync()
        {
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var marketData = new AvMarketData(avApiKey, factory);

            var (HighestHigh, LowestLow) = await marketData.GetIntradayHighestHighLowestLowAsync("MSFT");

            Assert.True(HighestHigh != 0);
            Assert.True(LowestLow != 0);
            Assert.True(HighestHigh > LowestLow);
        }
    }
}
