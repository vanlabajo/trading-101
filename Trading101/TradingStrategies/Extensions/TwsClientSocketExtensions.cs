using AutoFinance.Broker.InteractiveBrokers.Wrappers;

namespace TradingStrategies.Extensions
{
    public static class TwsClientSocketExtensions
    {
        public static void RequestAccountSummary(this ITwsClientSocket twsClientSocket, int reqId, string group, string tags)
        {
            twsClientSocket.EClientSocket.reqAccountSummary(reqId, group, tags);
        }

        public static void CancelAccountSummary(this ITwsClientSocket twsClientSocket, int reqId)
        {
            twsClientSocket.EClientSocket.cancelAccountSummary(reqId);
        }
    }
}
