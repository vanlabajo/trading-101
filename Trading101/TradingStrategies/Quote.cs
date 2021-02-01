namespace TradingStrategies
{
    public class Quote
    {
        public decimal Open { get; private set; }
        public decimal Close { get; private set; }
        public decimal High { get; private set; }
        public decimal Low { get; private set; }

        public Quote(decimal open, decimal close, decimal high, decimal low)
        {
            Open = open;
            Close = close;
            High = high;
            Low = low;
        }
    }
}
