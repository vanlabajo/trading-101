using AutoFinance.Broker.InteractiveBrokers;
using AutoFinance.Broker.InteractiveBrokers.Constants;
using AutoFinance.Broker.InteractiveBrokers.EventArgs;
using IBApi;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradingStrategies.Constants;
using TradingStrategies.Wrappers;
using Xunit;

namespace IntegrationTests
{
    /// <summary>
    /// These tests require that TWS be running on the local machine.
    /// They also require that the API is enabled, and that 127.0.0.1 and 0:0:0:0:0:0:0:1 are in the Allowed Hosts section of the TWS configuration.
    /// The setting can be found in the TWS window under "Account -> Global Settings -> API".
    /// </summary>
    public class TwsBrokerTests
    {
        private string twsAccountId;

        public TwsBrokerTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<TwsBrokerTests>()
                .Build();

            twsAccountId = configuration["TwsAccountId"];
        }

        [Fact]
        public async Task GetAvailableFundsAsync()
        {
            var twsObjectFactory = new TwsObjectFactory("localhost", 7497, 1);
            var broker = new TwsBroker(twsAccountId, twsObjectFactory);

            var availableFunds = await broker.GetAvailableFundsAsync();

            Assert.True(availableFunds > 0);

            await twsObjectFactory.TwsController.DisconnectAsync();
        }

        [Fact]
        public async Task PlaceBracketOrderAsync()
        {
            var twsObjectFactory = new TwsObjectFactory("localhost", 7497, 1);
            var broker = new TwsBroker(twsAccountId, twsObjectFactory);

            var entryOrderId = await broker.PlaceBracketOrderAsync("MSFT", "BUY", 1m, 238.93m, 239.33m);
            Assert.True(entryOrderId > 0);

            Thread.Sleep(1000); // TWS takes some time to put the order in the portfolio. Wait for it.

            var openOrders = await broker.GetOpenOrderIdsAsync("MSFT");

            Assert.True(openOrders.Count == 2);

            var result = await broker.CancelOrderAsync(entryOrderId);
            Assert.True(result);

            await twsObjectFactory.TwsController.DisconnectAsync();
        }

        [Fact]
        public async Task PlaceBracketOrderWithStopLossAsync()
        {
            var twsObjectFactory = new TwsObjectFactory("localhost", 7497, 1);
            var broker = new TwsBroker(twsAccountId, twsObjectFactory);

            var entryOrderId = await broker.PlaceBracketOrderAsync("MSFT", "BUY", 1m, 238.93m, 239.33m, 238.73m);
            Assert.True(entryOrderId > 0);

            Thread.Sleep(1000); // TWS takes some time to put the order in the portfolio. Wait for it.

            var openOrders = await broker.GetOpenOrderIdsAsync("MSFT");

            Assert.True(openOrders.Count == 3);

            var result = await broker.CancelOrderAsync(entryOrderId);
            Assert.True(result);

            await twsObjectFactory.TwsController.DisconnectAsync();
        }

        [Fact]
        public async Task GetClosePriceAsync()
        {
            var twsObjectFactory = new TwsObjectFactory("localhost", 7497, 1);
            var broker = new TwsBroker(twsAccountId, twsObjectFactory);

            var price = await broker.GetClosePriceAsync("MSFT");

            Thread.Sleep(1000); // TWS takes some time. Wait for it.

            Assert.True(price > 0);
        }

        [Fact]
        public async Task GetLastPriceAsync()
        {
            var twsObjectFactory = new TwsObjectFactory("localhost", 7497, 1);
            var broker = new TwsBroker(twsAccountId, twsObjectFactory);

            var price = await broker.GetLastPriceAsync("MSFT");

            Thread.Sleep(1000); // TWS takes some time. Wait for it.

            Assert.True(price > 0);
        }
    }
}
