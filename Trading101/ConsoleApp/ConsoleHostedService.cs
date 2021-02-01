using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TradingStrategies;
using TradingStrategies.Constants;

namespace ConsoleApp
{
    internal class ConsoleHostedService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IHostApplicationLifetime appLifetime;
        private readonly AppSettings appSettings;
        private readonly IMarketData marketData;
        private readonly IBroker broker;
        private readonly ILogger<Strategy102> strategyLogger;

        public ConsoleHostedService(ILogger<ConsoleHostedService> logger,
            IHostApplicationLifetime appLifetime,
            AppSettings appSettings,
            IMarketData marketData,
            IBroker broker,
            ILogger<Strategy102> strategyLogger)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.appSettings = appSettings;
            this.marketData = marketData;
            this.broker = broker;
            this.strategyLogger = strategyLogger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting application.");

            appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var capital = await broker.GetAvailableFundsAsync(Currency.USD);
                        logger.LogInformation("{0} - available funds in broker account.", capital);

                        capital = 500m;
                        logger.LogInformation("{0} - faked available funds in broker account.", capital);

                        foreach (var stock in appSettings.StockPicks)
                        {
                            try
                            {
                                var strategy = new Strategy102(stock, strategyLogger, marketData);
                                await strategy.GetMarketData();
                                var quote = strategy.GetQuote();

                                if (strategy.IsBullish())
                                {
                                    var entryPrice = quote.Close + 0.02m;
                                    var stopLoss = entryPrice - 0.2m;
                                    var positionSize = PositionSizing.GetPositionSize(capital, 1m, 1.5m, entryPrice, stopLoss);

                                    if (strategy.ShouldTrade(positionSize.TargetProfit))
                                    {
                                        var fundsNeeded = entryPrice * positionSize.Quantity;
                                        if (capital >= fundsNeeded)
                                        {
                                            var success = false;
                                            var entryOrderId = 0;
                                            try
                                            {
                                                entryOrderId = await broker.PlaceBracketOrderAsync(stock, "BUY", positionSize.Quantity, entryPrice, stopLoss, positionSize.TargetProfit);
                                            }
                                            catch (Exception) { }

                                            if (entryOrderId > 0) success = true;
                                            else
                                            {
                                                var openOrders = await broker.GetOpenOrderIdsAsync(stock);
                                                if (openOrders.Count == 3) success = true;
                                            }

                                            if (success)
                                            {
                                                logger.LogInformation("{0} - placed LONG bracket order.", stock);
                                                capital -= fundsNeeded;
                                                logger.LogInformation("{0} - available funds in broker account.", capital);
                                            }
                                        }
                                        else logger.LogError("{0} available funds is less than the needed {1}", capital, fundsNeeded);
                                    }
                                }
                            }
                            catch (AggregateException ae)
                            {
                                foreach (var ex in ae.InnerExceptions)
                                    logger.LogError("{0}: {1}", ex.GetType().Name, ex.Message);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError("{0}: {1}", ex.GetType().Name, ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unhandled exception!");
                    }
                    finally
                    {
                        logger.LogInformation("Stopping application.");
                        appLifetime.StopApplication();
                    }
                });
            });

            return Task.CompletedTask;

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}