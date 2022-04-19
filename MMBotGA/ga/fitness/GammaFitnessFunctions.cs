using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MMBotGA.backtest;
using MMBotGA.dto;

namespace MMBotGA.ga.fitness
{
    internal static class GammaFitnessFunctions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GammaFitnessFunctions));

        public static double CheckForEvents(
            ICollection<RunResponse> results
        )
        {
            if (results.Where(x => x.Event != null).Where(x => x.Event == "margin_call").Count() > 0)
            {
                return 0;
            }
            else
            {
                return 1;
            };
        }

        public static double TightenNplRpnlSubmergedFunction(
            ICollection<RunResponse> results,
            int minimumTradeCountThreshold
        )
        {
            if (results.Count == 0) { return 0; }
            if (ensureMinimumTradeCount(results, minimumTradeCountThreshold)) { return 0; }

            double deviatedTrades = 0;
            double tradesCounted = results.Count();
            var trades = results.Where(x => x.Info != null);

            int index = 0;

            foreach (var trade in trades)
            {
                double pLast = trade.Info.PriceLast;
                double pNeutral = trade.Info.PriceNeutral;

                double np = trade.Np; //neutral price
                double tradeSize = trade.Sz;
                double pl = trade.Pl; //profit and loss
                double npl = trade.Npl; //normalized profit


                //double percDiffPlNpl = PercentageDifference(pl, npl);
                double percDiffpLastNp = PercentageDifference(pLast, np);

                double percDiffpNeutralpLastEvaluated;
                //double percDiffPlRpnlEvaluated;

                double opPrWeight = 5; // 1-10 < lower the weight, more aggressive.
                //double plRpnlWeight = 10; // same as above. Not used, have negative impact on profits.

                //Explanation :
                //If measuring diff in npl and pl in percentage, can heavily impact profit, because further down we go, the bigger 
                //penalization, but we need to aggresively buy in downtrend direction to overturn it in slight upward tick.

                if (tradeSize != 0)
                {
                    //f(y) = x/100 * x/(5-10);
                    percDiffpNeutralpLastEvaluated = (percDiffpLastNp / 100) * (percDiffpLastNp / opPrWeight); 
                    //percDiffPlRpnlEvaluated = (percDiffPlNpl / 100) * (percDiffPlNpl / plRpnlWeight); 
                    

                    if (pLast < pNeutral) { 
                        //Calc penalization for trade only if priceLast is lower then priceNeutral.
                        //(meaning, that MMbot is not catching up on downtrend quick enough)                        
                        deviatedTrades += percDiffpNeutralpLastEvaluated;
                        //deviatedTrades += percDiffPlRpnlEvaluated;
                    }

                }

                if (tradeSize == 0) { deviatedTrades += 2; }

                index++;
            }

            

            double deviatedTradesRatio = deviatedTrades / tradesCounted;
            double deviationThresholdActual = 1 - deviatedTradesRatio;
            return deviationThresholdActual;
        }

        public static bool GetEquityToFollow(
            RunResponse result,
            double tightenEquityFollow
        )
        {
            bool deviated = false;
            double pl = result.Pl;
            double rpnl = result.Rpnl;

            double percentageDiffCalculation = PercentageDifference(pl, rpnl);
            if (percentageDiffCalculation > tightenEquityFollow) { deviated = true; }

            return deviated;
        }
        private static double PnlProfitPerYear(
            BacktestRequest request,
            ICollection<RunResponse> results
        )
        {
            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var interval = last.Tm - first.Tm;
            var profit = (Math.Max(last.Pl * 31536000000 / (interval * request.RunRequest.Balance), 0)) * 100;

            if (profit <= 0)
            { return 0; }
            else 
            { return profit; }

        }
        private static bool ensureMinimumTradeCount(
            ICollection<RunResponse> results,
            int tradesPerDayThreshold
        )
        {
            if (results.Count < 2) return false;

            var last = results.Last();
            var first = results.First();

            var trades = results.Count(x => x.Sz != 0);
            var alerts = 1 - (results.Count - trades) / (double)results.Count;

            //if (trades == 0 || alerts / trades > 0.02) return false; //alerts / trades > 0.02

            var days = (last.Tm - first.Tm) / 86400000d;
            var tradesPerDay = trades / days;

            if (tradesPerDay > tradesPerDayThreshold) 
            { return false; } 
            else 
            { return true; } //if failed check, return true.
        }

        #region NotFoundUseFor
        public static double Rrr(
            ICollection<RunResponse> results
        )
        {
            if (results.Count < 1) return 0;

            double maxPl = 0, minPl = 0, maxDowndraw = 0;
            foreach (var trade in results)
            {
                if (trade.Pl > maxPl)
                {
                    minPl = maxPl = trade.Pl;
                }

                if (trade.Pl < minPl)
                {
                    minPl = trade.Pl;
                    var downdraw = maxPl - minPl;
                    if (downdraw > maxDowndraw)
                    {
                        maxDowndraw = downdraw;
                    }
                }
            }

            var result = Math.Max(maxPl / maxDowndraw, 0);
            return Normalize(result, 5, 10, null);
        }        
        private static double IncomePerDayRatio(
            ICollection<RunResponse> results
        )
        {
            if (results.Count < 2)
            {
                return 0;
            }

            var firstResult = results.First();
            var lastResult = results.Last();

            var totalDays = (lastResult.Tm - firstResult.Tm) / 86400000;

            if (totalDays <= 0)
            {
                return 0;
            }

            var backtestStartingPoint = firstResult.Tm;
            var goodDay = 0;

            for (var day = 0; day < totalDays; day++)
            {
                var firstChunkTrade = backtestStartingPoint + day * 86400000;
                var lastChunkTrade = backtestStartingPoint + (day + 1) * 86400000;

                var dayTrades = results
                    .Where(x => x.Tm >= firstChunkTrade && x.Tm < lastChunkTrade)
                    .ToList();

                if (!dayTrades.Any()) continue;

                var np = dayTrades.Last().Np - dayTrades.First().Np;
                var pl = dayTrades.Last().Pl - dayTrades.First().Pl;

                if (pl > 0 && np > 0)
                {
                    goodDay++;
                }
            }

            return (double)goodDay / totalDays;
        }
        #endregion

        public static FitnessComposition NpaRrr(
            BacktestRequest request,
            ICollection<RunResponse> results
        )
        {
            if (results == null || results.Count == 0) return new FitnessComposition();

            //const double rrrWeight = 0.4;
            const double tightenNplRpnlWeight = 1;
            //const double ipdrWeight = 0;

            const double tightenNplRpnlThreshold = 1.5; //Dynamic as of now.
            const double tightenEquityThreshold = 1.5; //Dynamic as of now.
            const int minimumTradesThreshold = 7; //minimum of x trades per day. Does not work, need to reinstate somehow more brutal.

            //var eventCheck = CheckForEvents(results); //0-1, nic jiného nevrací.
            var result = new FitnessComposition();

            result.RRR = Rrr(results);
            result.TightenNplRpnl = tightenNplRpnlWeight * TightenNplRpnlSubmergedFunction(results,
                minimumTradesThreshold);
            if (result.TightenNplRpnl < 0) result.TightenNplRpnl = 0;
            result.PnlProfitPerYear = PnlProfitPerYear(request, results);

            #region FitnessTriangleCalculation
            var first = results.First();
            var last = results.Last();

            var interval = last.Tm - first.Tm;
            var backtestDays = (interval / 86400000);
            var penalization = backtestDays * result.TightenNplRpnl;// + result.IncomePerDayRatio);

            if (penalization == 0) { backtestDays = 5 * backtestDays; } // Kickstart, like old LADA.

            double xDiff = backtestDays - (penalization); //if negative penalization, it turns into positive thus increasing the base
            // of triangle, hence introducing 
            double yDiff = result.PnlProfitPerYear;
            var fitnessAngle = Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;

            if (penalization < 0 || ensureMinimumTradeCount(results, minimumTradesThreshold)) 
            {
                fitnessAngle = 0;
            }

            //                                      /|
            //                                /      |
            //                         /             |
            //                  /                  profit
            //            /                          |
            //      / fitnessAngle                   |
            //      ---------------days---------------
            result.Fitness = fitnessAngle;
            #endregion

            return result;
        }

        public static double Normalize(
            double value,
            double target,
            double virtualMax,
            double? cap
        )
        {
            if (value <= 0) return 0;
            var capped = Math.Min(value, cap ?? value);
            var baseline = Math.Min(capped, target) / target;
            var aboveTarget = Math.Max(0, value - target);
            var vMaxAboveTarget = virtualMax - target;
            var extra = Math.Min(aboveTarget, vMaxAboveTarget) / vMaxAboveTarget;

            return 0.75 * baseline + 0.25 * extra;
        }

        public static double PercentageDifference(
            double firstValue,
            double secondValue
        )
        {
            double numerator = Math.Abs(firstValue - secondValue);
            double denominator = (firstValue + secondValue) / 2;

            if (numerator != 0)
            {
                double percentageDiff = (numerator / denominator) * 100;
                return Math.Abs(percentageDiff);
            }

            return 0;
        }
    }
}