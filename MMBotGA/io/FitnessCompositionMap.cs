using CsvHelper.Configuration;
using MMBotGA.ga.fitness;

namespace MMBotGA.io
{
    internal class FitnessCompositionMap : ClassMap<FitnessComposition>
    {
        public FitnessCompositionMap()
        {
            Map(x => x.NpProfitPerYear).Index(40);
            Map(x => x.PnlProfitPerYear).Index(41);
            Map(x => x.IncomePerDayRatio).Index(42);
            Map(x => x.RRR).Index(43);
            Map(x => x.TradeCountFactor).Index(44);
            Map(x => x.LowerPositionFactor).Index(45);
            Map(x => x.MaxCostFactor).Index(46);
            Map(x => x.minMaxBalanceTheBalanceFactor).Index(47);
        }
    }
}