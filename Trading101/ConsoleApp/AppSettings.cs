using System.Collections.Generic;

namespace ConsoleApp
{
    public class AppSettings
    {
        public decimal Capital { get; set; }
        public List<string> StockPicks { get; set; }
        public TwsSettings TwsSettings { get; set; }
        public string AvApiKey { get; set; }
    }

    public class TwsSettings
    {
        public string AccountId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public int ClientId { get; set; }
    }
}
