using AutoFinance.Broker.InteractiveBrokers.Constants;
using AutoFinance.Broker.InteractiveBrokers.Controllers;
using AutoFinance.Broker.InteractiveBrokers.EventArgs;
using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TradingStrategies.Wrappers
{
    public class TwsBroker : IBroker
    {
        private readonly string accountId;
        private readonly ITwsObjectFactory twsObjectFactory;

        public TwsBroker(string accountId, ITwsObjectFactory twsObjectFactory)
        {
            this.accountId = accountId;
            this.twsObjectFactory = twsObjectFactory;
        }

        public async Task<decimal> GetAvailableFundsAsync()
        {
            var twsController = twsObjectFactory.TwsController;

            await twsController.EnsureConnectedAsync();

            var accountDetails = await twsController.GetAccountDetailsAsync(accountId);
            if (accountDetails.IsEmpty || !accountDetails.ContainsKey("CashBalance")) return 0;

            return decimal.Parse(accountDetails["CashBalance"]);
        }

        public async Task<int> PlaceBracketOrderAsync(string symbol, string entryAction, decimal quantity, decimal entryPrice, decimal targetReward)
        {
            var twsController = twsObjectFactory.TwsController;

            await twsController.EnsureConnectedAsync();

            var contractDetails = await twsController.GetContractAsync(new IBApi.Contract
            {
                SecType = TwsContractSecType.Stock,
                Symbol = symbol,
                Exchange = TwsExchange.Smart,
                PrimaryExch = TwsExchange.Island
            });

            if (contractDetails == null || contractDetails.Count == 0) return 0;

            var contract = contractDetails.First().Contract;

            int entryOrderId = await twsController.GetNextValidIdAsync();
            var takeProfitOrderId = await twsController.GetNextValidIdAsync();

            var entryOrder = new Order()
            {
                Action = entryAction,
                OrderType = TwsOrderType.Limit,
                TotalQuantity = Convert.ToDouble(quantity),
                LmtPrice = Convert.ToDouble(entryPrice),
                Tif = TwsTimeInForce.GoodTillClose,
                Transmit = false
            };

            var takeProfit = new Order()
            {
                Action = TwsOrderActions.Reverse(entryAction),
                OrderType = TwsOrderType.Limit,
                TotalQuantity = Convert.ToDouble(quantity),
                LmtPrice = Convert.ToDouble(targetReward),
                ParentId = entryOrderId,
                Tif = TwsTimeInForce.GoodTillClose,
                Transmit = true
            };

            var entryOrderAckTask = twsController.PlaceOrderAsync(entryOrderId, contract, entryOrder);
            var takeProfitOrderAckTask = twsController.PlaceOrderAsync(takeProfitOrderId, contract, takeProfit);

            Task.WaitAll(entryOrderAckTask, takeProfitOrderAckTask);

            var result = entryOrderAckTask.Result && takeProfitOrderAckTask.Result;

            if (result) return entryOrderId;
            else return 0;
        }

        public async Task<int> PlaceBracketOrderAsync(string symbol, string entryAction, decimal quantity, decimal entryPrice, decimal targetReward, decimal stopPrice)
        {
            var twsController = twsObjectFactory.TwsController;

            await twsController.EnsureConnectedAsync();

            var contractDetails = await twsController.GetContractAsync(new IBApi.Contract
            {
                SecType = TwsContractSecType.Stock,
                Symbol = symbol,
                Exchange = TwsExchange.Smart,
                PrimaryExch = TwsExchange.Island
            });

            if (contractDetails == null || contractDetails.Count == 0) return 0;

            var contract = contractDetails.First().Contract;

            int entryOrderId = await twsController.GetNextValidIdAsync();
            var takeProfitOrderId = await twsController.GetNextValidIdAsync();
            var stopOrderId = await twsController.GetNextValidIdAsync();

            var entryOrder = new Order()
            {
                Action = entryAction,
                OrderType = TwsOrderType.Limit,
                TotalQuantity = Convert.ToDouble(quantity),
                LmtPrice = Convert.ToDouble(entryPrice),
                Tif = TwsTimeInForce.GoodTillClose,
                Transmit = false
            };

            var takeProfit = new Order()
            {
                Action = TwsOrderActions.Reverse(entryAction),
                OrderType = TwsOrderType.Limit,
                TotalQuantity = Convert.ToDouble(quantity),
                LmtPrice = Convert.ToDouble(targetReward),
                ParentId = entryOrderId,
                Tif = TwsTimeInForce.GoodTillClose,
                Transmit = false
            };

            var stopLoss = new Order()
            {
                Action = TwsOrderActions.Reverse(entryAction),
                OrderType = TwsOrderType.StopLoss,
                TotalQuantity = Convert.ToDouble(quantity),
                AuxPrice = Convert.ToDouble(stopPrice),
                ParentId = entryOrderId,
                Tif = TwsTimeInForce.GoodTillClose,
                Transmit = true
            };

            var entryOrderAckTask = twsController.PlaceOrderAsync(entryOrderId, contract, entryOrder);
            var takeProfitOrderAckTask = twsController.PlaceOrderAsync(takeProfitOrderId, contract, takeProfit);
            var stopOrderAckTask = twsController.PlaceOrderAsync(stopOrderId, contract, stopLoss);

            Task.WaitAll(entryOrderAckTask, takeProfitOrderAckTask, stopOrderAckTask);

            var result = entryOrderAckTask.Result && takeProfitOrderAckTask.Result && stopOrderAckTask.Result;

            if (result) return entryOrderId;
            else return 0;
        }

        public async Task<List<int>> GetOpenOrderIdsAsync(string symbol)
        {
            var result = new List<int>();
            var twsController = twsObjectFactory.TwsController;

            await twsController.EnsureConnectedAsync();

            var timeoutCancellationTokenSource = new CancellationTokenSource(60 * 1000);
            var openOrders = await twsController.RequestOpenOrders(timeoutCancellationTokenSource.Token);
            var openOrdersByAccountId = openOrders.Where(order => order.Order.Account == accountId).ToList();

            foreach (var openOrder in openOrdersByAccountId)
            {
                if (openOrder.Contract.Symbol == symbol)
                    result.Add(openOrder.OrderId);
            }

            return result.Distinct().ToList();
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var twsController = twsObjectFactory.TwsController;

            await twsController.EnsureConnectedAsync();

            var result = await twsController.CancelOrderAsync(orderId);

            return result;
        }

        public async Task<decimal> GetClosePriceAsync(string symbol)
        {
            var twsController = twsObjectFactory.TwsController;

            await twsController.EnsureConnectedAsync();

            var contractDetails = await twsController.GetContractAsync(new IBApi.Contract
            {
                SecType = TwsContractSecType.Stock,
                Symbol = symbol,
                Exchange = TwsExchange.Smart,
                PrimaryExch = TwsExchange.Island
            });

            if (contractDetails == null || contractDetails.Count == 0) return 0;

            var contract = contractDetails.First().Contract;
            var requestId = twsController.GetNextRequestId();
            var taskSource = new TaskCompletionSource<decimal>();

            EventHandler<TickPriceEventArgs> tickPriceEventHandler = null;

            tickPriceEventHandler = (sender, args) => 
            {
                if (args.TickerId == requestId)
                {
                    if (args.Field == TickType.CLOSE || args.Field == TickType.DELAYED_CLOSE)
                    {
                        twsObjectFactory.TwsCallbackHandler.TickPriceEvent -= tickPriceEventHandler;
                        taskSource.TrySetResult((decimal)args.Price);
                    }
                }
            };

            // Set the operation to cancel after 1 minute
            var tokenSource = new CancellationTokenSource(60 * 1000);
            tokenSource.Token.Register(() =>
            {
                twsObjectFactory.ClientSocket.CancelMarketData(requestId);

                twsObjectFactory.TwsCallbackHandler.TickPriceEvent -= tickPriceEventHandler;

                taskSource.TrySetCanceled();
            });


            twsObjectFactory.TwsCallbackHandler.TickPriceEvent += tickPriceEventHandler;

            twsObjectFactory.ClientSocket.RequestMarketDataType(3);
            twsObjectFactory.ClientSocket.RequestMarketData(requestId, contract, "", false, false, new List<TagValue>());

            var result = await taskSource.Task;

            return result;
        }

        public async Task<decimal> GetLastPriceAsync(string symbol)
        {
            var twsController = twsObjectFactory.TwsController;

            await twsController.EnsureConnectedAsync();

            var contractDetails = await twsController.GetContractAsync(new IBApi.Contract
            {
                SecType = TwsContractSecType.Stock,
                Symbol = symbol,
                Exchange = TwsExchange.Smart,
                PrimaryExch = TwsExchange.Island
            });

            if (contractDetails == null || contractDetails.Count == 0) return 0;

            var contract = contractDetails.First().Contract;
            var requestId = twsController.GetNextRequestId();
            var taskSource = new TaskCompletionSource<decimal>();

            EventHandler<TickPriceEventArgs> tickPriceEventHandler = null;

            tickPriceEventHandler = (sender, args) =>
            {
                if (args.TickerId == requestId)
                {
                    if (args.Field == TickType.LAST || args.Field == TickType.DELAYED_LAST)
                    {
                        twsObjectFactory.TwsCallbackHandler.TickPriceEvent -= tickPriceEventHandler;
                        taskSource.TrySetResult((decimal)args.Price);
                    }
                }
            };

            // Set the operation to cancel after 1 minute
            var tokenSource = new CancellationTokenSource(60 * 1000);
            tokenSource.Token.Register(() =>
            {
                twsObjectFactory.ClientSocket.CancelMarketData(requestId);

                twsObjectFactory.TwsCallbackHandler.TickPriceEvent -= tickPriceEventHandler;

                taskSource.TrySetCanceled();
            });


            twsObjectFactory.TwsCallbackHandler.TickPriceEvent += tickPriceEventHandler;

            twsObjectFactory.ClientSocket.RequestMarketDataType(3);
            twsObjectFactory.ClientSocket.RequestMarketData(requestId, contract, "", false, false, new List<TagValue>());

            var result = await taskSource.Task;

            return result;
        }
    }
}
