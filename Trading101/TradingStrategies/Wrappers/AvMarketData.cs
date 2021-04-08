using AlphaVantage.Net.Common.Exceptions;
using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Core.Client;
using AlphaVantage.Net.Stocks.Client;
using AlphaVantage.Net.TechnicalIndicators;
using AlphaVantage.Net.TechnicalIndicators.Client;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TradingStrategies.Wrappers
{
    public class AvMarketData : IMarketData
    {
        private readonly string apiKey;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly AsyncRetryPolicy retryPolicy;

        public AvMarketData(string apiKey, IHttpClientFactory httpClientFactory)
        {
            this.apiKey = apiKey;
            this.httpClientFactory = httpClientFactory;

            retryPolicy = Policy
                .Handle<AlphaVantageParsingException>()
                .WaitAndRetryAsync(3, i => TimeSpan.FromMinutes(1));
        }

        public async Task<(decimal HighestHigh, decimal LowestLow)> GetIntradayHighestHighLowestLowAsync(string symbol)
        {
            using var alphaVantageClient = new AlphaVantageClient(apiKey, httpClientFactory.CreateClient("workerservice"));
            using var stocksClient = alphaVantageClient.Stocks();

            var highestHigh = 0m;
            var lowestLow = 0m;

            await retryPolicy.ExecuteAsync(async () =>
            {
                var intraday = await stocksClient.GetTimeSeriesAsync(symbol, Interval.Min60);
                if (intraday?.DataPoints.Count > 0)
                {
                    var dp = intraday.DataPoints
                        .Where(dp => dp.Time.Date >= DateTime.Now.AddDays(-7).Date)
                        .ToList();

                    highestHigh = dp.Select(dp => dp.HighestPrice).Max();
                    lowestLow = dp.Select(dp => dp.LowestPrice).Min();
                }
            });

            return (highestHigh, lowestLow);
        }

        public async Task<decimal> GetLatestEmaAsync(string symbol, int timePeriod)
        {
            using var alphaVantageClient = new AlphaVantageClient(apiKey, httpClientFactory.CreateClient("workerservice"));

            var emaLatest = 0m;

            await retryPolicy.ExecuteAsync(async () =>
            {
                var ema = await alphaVantageClient.GetTechIndicatorTimeSeriesAsync(symbol, TechIndicatorType.EMA, Interval.Min1, new Dictionary<string, string>()
                {
                    {"time_period", $"{timePeriod}"},
                    {"series_type", "close"}
                });

                if (ema?.DataPoints.Count > 0)
                {
                    var emaDataPoint = ema.DataPoints.First().Parameters?.FirstOrDefault();
                    if (emaDataPoint != null) emaLatest = emaDataPoint.ParameterValue;
                }
            });

            return emaLatest;
        }

        public async Task<decimal> GetLatestSmaAsync(string symbol, int timePeriod)
        {
            using var alphaVantageClient = new AlphaVantageClient(apiKey, httpClientFactory.CreateClient("workerservice"));

            var smaLatest = 0m;

            await retryPolicy.ExecuteAsync(async () =>
            {
                var sma = await alphaVantageClient.GetTechIndicatorTimeSeriesAsync(symbol, TechIndicatorType.SMA, Interval.Min1, new Dictionary<string, string>()
                {
                    {"time_period", $"{timePeriod}"},
                    {"series_type", "close"}
                });

                if (sma?.DataPoints.Count > 0)
                {
                    var smaDataPoint = sma.DataPoints.First().Parameters?.FirstOrDefault();
                    if (smaDataPoint != null) smaLatest = smaDataPoint.ParameterValue;
                }
            });

            return smaLatest;
        }

        public async Task<Quote> GetQuoteAsync(string symbol)
        {
            using var alphaVantageClient = new AlphaVantageClient(apiKey, httpClientFactory.CreateClient("workerservice"));
            using var stocksClient = alphaVantageClient.Stocks();

            Quote quote = null;

            await retryPolicy.ExecuteAsync(async () =>
            {
                var globalQuote = await stocksClient.GetGlobalQuoteAsync(symbol);
                if (globalQuote != null)
                {
                    quote = new Quote(globalQuote.OpeningPrice, globalQuote.Price, globalQuote.HighestPrice, globalQuote.LowestPrice);
                }
            });

            return quote;
        }
    }
}
