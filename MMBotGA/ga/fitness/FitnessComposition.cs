﻿namespace MMBotGA.ga.fitness
{
    internal class FitnessComposition
    {
        public double Fitness { get; set; }
        public double NpProfitPerYear { get; set; }
        public double PnlProfitPerYear { get; set; }
        public double IncomePerDayRatio { get; set; }
        public double RRR { get; set; }
        public double TradeCountFactor { get; set; }
        public double LowerPositionFactor { get; set; }
        public double MaxCostFactor { get; set; }
        public double RpnlFactor { get; set; }
        public double MinMaxBalanceTheBalanceFactor { get; set; }
        public double TightenNplRpnlSubmergedFunction { get; set; }
    }
}