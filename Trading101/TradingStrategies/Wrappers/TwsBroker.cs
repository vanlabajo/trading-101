using AutoFinance.Broker.InteractiveBrokers.Constants;
using AutoFinance.Broker.InteractiveBrokers.Controllers;
using AutoFinance.Broker.InteractiveBrokers.EventArgs;
using AutoFinance.Broker.InteractiveBrokers.Exceptions;
using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradingStrategies.Extensions;

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

        public async Task<decimal> GetAvailableFundsAsync(string currency)
        {
            var twsController = twsObjectFactory.TwsController;

            await twsController.EnsureConnectedAsync();

            var accountDetails = await twsController.GetAccountDetailsAsync(accountId);
            if (accountDetails.Count == 0 || !accountDetails.ContainsKey("AvailableFunds")) return 0;

            return decimal.Parse(accountDetails["AvailableFunds"]);
        }

        //public async Task<List<Order>> GetOpenOrdersAsync(string symbol)
        //{
        //    var result = new List<Order>();
        //    var twsController = twsObjectFactory.TwsController;

        //    await twsController.EnsureConnectedAsync();

        //    var timeoutCancellationTokenSource = new CancellationTokenSource(60 * 1000);
        //    var openOrders = await twsController.RequestOpenOrders(timeoutCancellationTokenSource.Token);
        //    var openOrdersByAccountId = openOrders.Where(order => order.Order.Account == accountId).ToList();

        //    foreach (var openOrder in openOrdersByAccountId)
        //    {
        //        result.Add(new Order
        //        {
        //            OrderId = openOrder.OrderId,
        //            Symbol = openOrder.Contract.Symbol,
        //            Status = openOrder.OrderState.Status,
        //            Action = openOrder.Order.Action,
        //            OrderType = openOrder.Order.OrderType,
        //            LimitPrice = Convert.ToDecimal(openOrder.Order.LmtPrice),
        //            AuxPrice = Convert.ToDecimal(openOrder.Order.AuxPrice)
        //        });
        //    }

        //    return result;
        //}

        //public async Task<double> GetPositionsAsync(string symbol)
        //{
        //    var result = 0d;
        //    var twsController = twsObjectFactory.TwsController;

        //    await twsController.EnsureConnectedAsync();

        //    var positions = await twsController.RequestPositions();
        //    var positionsByAccountId = positions.Where(position => position.Account == accountId).ToList();
        //    var positionsBySymbol = positionsByAccountId.Where(position => position.Contract.Symbol == symbol).ToList();

        //    foreach (var position in positionsBySymbol)
        //    {
        //        result += position.Position;
        //    }

        //    return result;
        //}

        //public async Task<List<(double Position, string Symbol)>> GetPositionsAsync()
        //{
        //    var result = new List<(double position, string symbol)>();
        //    var twsController = twsObjectFactory.TwsController;

        //    await twsController.EnsureConnectedAsync();

        //    var positions = await twsController.RequestPositions();
        //    var positionsByAccountId = positions.Where(position => position.Account == accountId).ToList();

        //    foreach (var position in positionsByAccountId)
        //    {
        //        result.Add((position.Position, position.Contract.Symbol));
        //    }

        //    return result;
        //}

        //public async Task<bool> PlaceLimitOrderAsync(Order order)
        //{
        //    var twsController = twsObjectFactory.TwsController;

        //    await twsController.EnsureConnectedAsync();

        //    var contract = await GetContractAsync(order.Symbol, twsController);
        //    if (contract == null) return false;

        //    var twsOrder = new IBApi.Order { OrderType = TwsOrderType.Limit };
        //    twsOrder.Action = order.Action;
        //    twsOrder.TotalQuantity = order.Quantity;
        //    twsOrder.LmtPrice = Convert.ToDouble(order.LimitPrice);

        //    var result = await PlaceOrderAsync(contract, twsOrder, twsController);

        //    return result;
        //}

        //public async Task<bool> CancelOrderAsync(Order order)
        //{
        //    var twsController = twsObjectFactory.TwsController;

        //    await twsController.EnsureConnectedAsync();

        //    var result = await twsController.CancelOrderAsync(order.OrderId);

        //    return result;
        //}

        //private async Task<IBApi.Contract> GetContractAsync(string symbol, ITwsController twsController)
        //{
        //    var contractDetails = await twsController.GetContractAsync(new IBApi.Contract
        //    {
        //        SecType = TwsContractSecType.Stock,
        //        Symbol = symbol,
        //        Exchange = TwsExchange.Smart,
        //        PrimaryExch = TwsExchange.Island
        //    });

        //    if (contractDetails == null || contractDetails.Count == 0) return null;

        //    return contractDetails.First().Contract;
        //}

        //private async Task<bool> PlaceOrderAsync(IBApi.Contract contract, IBApi.Order order, ITwsController twsController)
        //{
        //    var orderId = await twsController.GetNextValidIdAsync();
        //    var timeoutCancellationTokenSource = new CancellationTokenSource(60 * 1000);
        //    var result = await twsController.PlaceOrderAsync(orderId, contract, order, timeoutCancellationTokenSource.Token);

        //    return result;
        //}


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
    }
}
