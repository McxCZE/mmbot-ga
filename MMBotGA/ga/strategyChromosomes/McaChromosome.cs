using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using MMBotGA.backtest;
using MMBotGA.io;
using System;

namespace MMBotGA.ga
{
    class McaChromosome : SpreadChromosome
    {
        public McaChromosome() : base(null, false)
        {
            // max is exclusives
            BuyStrength = Factory.Create(() => RandomizationProvider.Current.GetDouble(0.01, 1));
            SellStrength = Factory.Create(() => RandomizationProvider.Current.GetDouble(0.01, 1));
            InitBet = Factory.Create(() => RandomizationProvider.Current.GetDouble(0.1, 100));

            FinalizeGenes();
        }

        #region Strategy

        public GeneWrapper<double> BuyStrength { get; }
        public GeneWrapper<double> SellStrength { get; }
        public GeneWrapper<double> InitBet { get; }

        #endregion

        public override Type CsvAggregatedMapType => typeof(AggregatedMcaChromosomeCsvMap);

        public override Type CsvSingleMapType => typeof(SingleMcaChromosomeCsvMap);

        public override Type CsvRecordType => typeof(McaChromosome);

        public override Gene GenerateGene(int geneIndex) => Factory.Generate(geneIndex);

        public override IChromosome CreateNew() => new McaChromosome();

        public override BacktestRequest ToBacktestRequest(bool export)
        {
            return ChromosomeExtensions.ToBacktestRequest(this, export);
        }
    }
}
