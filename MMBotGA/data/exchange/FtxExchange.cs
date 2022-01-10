namespace MMBotGA.data.exchange
{
    internal class FtxExchange : IExchange
    {
        public string Name => "FTX";

        public string GetSymbol(Pair pair)
        {
            if (pair.Currency.ToUpper() == "PERP")
            {
                return $"{pair.Asset}-{pair.Currency}".ToUpperInvariant();
            } else
            {
                return $"{pair.Asset}/{pair.Currency}".ToUpperInvariant();
            }
            
        }

        public string GetRobotSymbol(Pair pair)
        {
            return GetSymbol(pair);
        }
    }
}