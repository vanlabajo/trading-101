using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TradingStrategies;

namespace ConsoleApp
{
    internal class ConsoleHostedService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IHostApplicationLifetime appLifetime;
        private readonly AppSettings appSettings;
        private readonly IMarketData marketData;
        private readonly IBroker broker;
        private readonly ILongStrategy longStrategy;

        public ConsoleHostedService(ILogger<ConsoleHostedService> logger,
            IHostApplicationLifetime appLifetime,
            AppSettings appSettings,
            IMarketData marketData,
            IBroker broker,
            ILongStrategy longStrategy)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.appSettings = appSettings;
            this.marketData = marketData;
            this.broker = broker;
            this.longStrategy = longStrategy;
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
                        var capital = await broker.GetAvailableFundsAsync();
                        if (capital > 500) capital = 500;
                        logger.LogInformation("{0} - available funds in broker account.", capital);

                        var availableFunds = capital;

                        foreach (var stock in appSettings.StockPicks)
                        {
                            try
                            {
                                var lastPrice = await broker.GetLastPriceAsync(stock);
                                logger.LogInformation("{0} - Last Price: {1}", stock, lastPrice);

                                if (longStrategy.IsBullish(stock, lastPrice))
                                {
                                    var stopLoss = lastPrice - 0.2m;
                                    var positionSize = PositionSizing.GetPositionSize(capital, 1m, 1.5m, lastPrice, stopLoss);

                                    if (longStrategy.ShouldTrade(stock, positionSize.TargetProfit))
                                    {
                                        var fundsNeeded = lastPrice * positionSize.Quantity;
                                        if (availableFunds >= fundsNeeded)
                                        {
                                            try
                                            {
                                                await broker.PlaceBracketOrderAsync(stock, "BUY", positionSize.Quantity, lastPrice, positionSize.TargetProfit);
                                            }
                                            catch (Exception) { }

                                            logger.LogInformation("{0} - buying {1} for {2}, selling for {3}.", stock, positionSize.Quantity, lastPrice, positionSize.TargetProfit);
                                            availableFunds -= fundsNeeded;
                                            logger.LogInformation("{0} - available funds in broker account.", availableFunds);
                                        }
                                        else logger.LogError("{0} available funds is less than the needed {1}", availableFunds, fundsNeeded);
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