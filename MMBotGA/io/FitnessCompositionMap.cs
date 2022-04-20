using CsvHelper.Configuration;
using MMBotGA.ga.fitness;

namespace MMBotGA.io
{
    internal class FitnessCompositionMap : ClassMap<FitnessComposition>
    {
        public FitnessCompositionMap()
        {
            Map(x => x.PnlProfitPerYear).Index(203);
            Map(x => x.RRR).Index(204);
            Map(x => x.TightenNplRpnl).Index(205);
            Map(x => x.rrrTightenCombined).Index(206);
        }
    }
}