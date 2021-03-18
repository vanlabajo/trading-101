using AutoFinance.Broker.InteractiveBrokers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TradingStrategies;
using TradingStrategies.Wrappers;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                {
                    configuration
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsoleApp.Log.txt"))
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
                    services.AddSingleton(appSettings);

                    services.AddSingleton<IBroker, TwsBroker>(sp =>
                    {
                        var factory = new TwsObjectFactory(appSettings.TwsSettings.Host, appSettings.TwsSettings.Port, appSettings.TwsSettings.ClientId);
                        return new TwsBroker(appSettings.TwsSettings.AccountId, factory);
                    });

                    services.AddHttpClient("workerservice", client =>
                    {
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    });

                    services.AddSingleton<IMarketData, AvMarketData>(sp =>
                    {
                        var factory = sp.GetRequiredService<IHttpClientFactory>();
                        return new AvMarketData(appSettings.AvApiKey, factory);
                    });

                    services.AddTransient<ILongStrategy, LongStrategy103>();

                    services.AddHostedService<ConsoleHostedService>();
                })
                .RunConsoleAsync();
        }
    }
}
