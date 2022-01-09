﻿//using GeneticSharp.Domain.Chromosomes;
//using GeneticSharp.Domain.Randomizations;
//using MMBotGA.backtest;

//namespace MMBotGA.ga
//{
//    class StrategyChromosome : ChromosomeBase
//    {
//        public StrategyChromosome()
//            : base(14)
//        {
//            CreateGenes();
//        }

//        #region Strategy
//        public double Exponent => this.GetGene<double>(0);
//        public double Trend => this.GetGene<double>(1);

//        //Roztoč si Rebalance jak potřebuješ.
//        public int Rebalance => this.GetGene<int>(2);
//        //Pokaždé použij SmartRebalance.
//        //public int Rebalance => 4;

//        //readonly string[] functions = new[] { "halfhalf", "keepvalue", "exponencial", "invsqrtsinh", "gauss" };
//        //Vypnutý Cosecant, dělal prasečiny.

//        //Half-Half  se jeví jako velmi zajímavý.
//        readonly string[] functions = new[] { "halfhalf", "keepvalue", "exponencial", "gauss" };
//        public string Function => functions[this.GetGene<int>(10)];

//        #endregion

//        #region Spread

//        public double Stdev => this.GetGene<double>(3);
//        public double Sma => this.GetGene<double>(4);
//        public double Mult => this.GetGene<double>(5);
//        public double Raise => this.GetGene<double>(6);
//        public double Fall => this.GetGene<double>(7);
//        public double Cap => this.GetGene<double>(8);

//        readonly string[] modes = new[] { "disabled", "independent", "together", "alternate", "half_alternate" };
//        //public string Mode => modes[this.GetGene<int>(9)];
//        public string Mode => "independent"; //nastavený statický Independent, je odzkoušený. 

//        public bool DynMult => this.GetGene<int>(11) == 1;
//        public bool Freeze => this.GetGene<int>(12) == 1;

//        public int SecondaryOrder => this.GetGene<int>(13);
//        #endregion

//        public string ID { get; set; }

//        public int Generation { get; set; }

//        public string Metadata { get; set; }

//        public Statistics Statistics { get; set; }
//        public Statistics BacktestStats { get; set; }
//        public Statistics ControlStats { get; set; }

//        public override Gene GenerateGene(int geneIndex)
//        {
//            return geneIndex switch
//            { // max is exclusive
//                //Exponent
//                0 => new Gene(RandomizationProvider.Current.GetDouble(1, 20)),
//                //Trend. Pref (-100,0) <- Close positions faster, do not hold position over normalized profit.)
//                1 => new Gene(RandomizationProvider.Current.GetDouble(-100, 10)),
//                //Type of Rebalance to be used.
//                2 => new Gene(RandomizationProvider.Current.GetInt(0, 5)),
//                //stDeviation.
//                3 => new Gene(RandomizationProvider.Current.GetDouble(1, 60)),
//                //Smooth Moving Average (SMA).
//                4 => new Gene(RandomizationProvider.Current.GetDouble(1, 60)),
//                //Manual adjust (-30/+30) as current. 
//                5 => new Gene(RandomizationProvider.Current.GetDouble(0.70, 1.30)),
//                //Mult. Raise.
//                6 => new Gene(RandomizationProvider.Current.GetDouble(1, 1000)),
//                //Mult. Fall.
//                7 => new Gene(RandomizationProvider.Current.GetDouble(0.1, 10)),
//                //Mult. Cap.
//                8 => new Gene(RandomizationProvider.Current.GetDouble(0, 100)),
//                //Mult. Modes.
//                9 => new Gene(RandomizationProvider.Current.GetInt(0, 5)),
//                //Strategy - Underlying Gamma Functions.
//                10 => new Gene(RandomizationProvider.Current.GetInt(0, 4)),
//                //Dynmult (on/off).
//                11 => new Gene(RandomizationProvider.Current.GetInt(0, 2)),
//                //Freeze Spread (on/off).
//                12 => new Gene(RandomizationProvider.Current.GetInt(0, 2)),
//                //SecondaryOrder distance in %. (Zalimitovat na např. 30% ? Pozn. Nadsazuje to vysoko.)
//                13 => new Gene(RandomizationProvider.Current.GetInt(0, 0)),
//                _ => new Gene(),
//            };
//        }

//        public override IChromosome CreateNew()
//        {
//            return new StrategyChromosome();
//        }
//    }
//}


using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using MMBotGA.backtest;

namespace MMBotGA.ga
{
    class StrategyChromosome : ChromosomeBase
    {
        private readonly GeneFactory _factory;

        public StrategyChromosome()
            : base(2)
        {
            _factory = new GeneFactory(this);

            // max is exclusive
            Exponent = _factory.Create(() => RandomizationProvider.Current.GetDouble(1, 20));
            Trend = _factory.Create(() => RandomizationProvider.Current.GetDouble(-100, 100));
            Rebalance = _factory.Create(() => RandomizationProvider.Current.GetInt(3, 5)); // 0-5
            FunctionGene = _factory.Create(() => RandomizationProvider.Current.GetInt(0, 3));
            Stdev = _factory.Create(() => RandomizationProvider.Current.GetDouble(1, 60));
            Sma = _factory.Create(() => RandomizationProvider.Current.GetDouble(1, 60));
            Mult = _factory.Create(() => RandomizationProvider.Current.GetDouble(0.95, 1.05)); // 0.95 - 1.05
            Raise = _factory.Create(() => RandomizationProvider.Current.GetDouble(1, 1000));
            Fall = _factory.Create(() => RandomizationProvider.Current.GetDouble(0.1, 10));
            Cap = _factory.Create(() => RandomizationProvider.Current.GetDouble(0, 100));
            //ModeGene = _factory.Create(() => RandomizationProvider.Current.GetInt(0, 5));
            DynMultGene = _factory.Create(() => RandomizationProvider.Current.GetInt(0, 2));
            FreezeGene = _factory.Create(() => RandomizationProvider.Current.GetInt(0, 2));

            //Static gene example:
            ModeGene = _factory.Create(1); //Independent osvědčený (obvzvlášť když ulítnou API na Exchange)

            Resize(_factory.Length);
            CreateGenes();
        }

        #region Strategy

        public GeneWrapper<double> Exponent { get; }

        public GeneWrapper<double> Trend { get; }
        public GeneWrapper<int> Rebalance { get; }

        private readonly string[] _functions = { "halfhalf", "keepvalue", "gauss" }; //"exponencial", "invsqrtsinh"
        private GeneWrapper<int> FunctionGene { get; }
        public string Function => _functions[FunctionGene.Value];

        #endregion

        #region Spread

        public GeneWrapper<double> Stdev { get; }
        public GeneWrapper<double> Sma { get; }
        public GeneWrapper<double> Mult { get; }
        public GeneWrapper<double> Raise { get; }
        public GeneWrapper<double> Fall { get; }
        public GeneWrapper<double> Cap { get; }

        private readonly string[] _modes = { "disabled", "independent", "together", "alternate", "half_alternate" };
        private GeneWrapper<int> ModeGene { get; }
        public string Mode => _modes[ModeGene.Value];

        private GeneWrapper<int> DynMultGene { get; }
        public bool DynMult => DynMultGene.Value == 1;

        private GeneWrapper<int> FreezeGene { get; }
        public bool Freeze => FreezeGene.Value == 1;

        #endregion

        public string ID { get; set; }

        public int Generation { get; set; }

        public string Metadata { get; set; }

        public Statistics Statistics { get; set; }
        public Statistics BacktestStats { get; set; }
        public Statistics ControlStats { get; set; }

        public override Gene GenerateGene(int geneIndex) => _factory.Generate(geneIndex);

        public override IChromosome CreateNew() => new StrategyChromosome();
    }
}
