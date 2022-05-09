using MMBotGA.ga;

namespace MMBotGA.io
{
    internal class McaChromosomeCsvMapBase : SpreadChromosomeCsvMapBase<McaChromosome>
    {
        public McaChromosomeCsvMapBase(bool aggregated) : base(aggregated)
        {
            Map(x => x.BuyStrength).Index(3);
            Map(x => x.SellStrength).Index(4);
            Map(x => x.InitBet).Index(5);
        }
    }
}