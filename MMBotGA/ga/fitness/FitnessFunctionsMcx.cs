﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using MMBotGA.backtest;
using MMBotGA.dto;

namespace MMBotGA.ga.fitness
{
    internal static class FitnessFunctionsMcx
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FitnessFunctionsMcx));

        #region Outdated
        public static double TradeCountFactor(ICollection<RunResponse> results)
        {
            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var trades = results.Count(x => x.Sz != 0);
            var alerts = 1 - (results.Count - trades) / (double)results.Count;

            var days = (last.Tm - first.Tm) / 86400000d;
            var tradesPerDay = trades / days;

            const int mean = 15;
            const int delta = 5; // target trade range is 7 - 22 trades per day

            var x = Math.Abs(tradesPerDay - mean); // 0 - inf, 0 is best
            var y = Math.Max(x - delta, 0) + 1; // 1 - inf, 1 is best ... 
            var r = 1 / y;

            var alertsRatio = alerts / trades;

            //Pokud mám více jak 3% alertů v celém spektru, špatný backtest.
            if (alertsRatio > 0.03) { return 0; } else { return r * alerts; }



            //return Normalize(trades, 1000, 3000, null) * alerts;
        }
        public static double IncomePerDayRatio(ICollection<RunResponse> results)
        {
            if (results.Count < 2)
            {
                return 0;
            }

            var firstResult = results.First();
            var lastResult = results.Last();

            double totalDays = (lastResult.Tm - firstResult.Tm) / 86400000;

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

                if (dayTrades.Any())
                {
                    var np = dayTrades.Last().Np - dayTrades.First().Np;
                    var pl = dayTrades.Last().Pl - dayTrades.First().Pl;

                    if (pl > 0 && np > 0)
                    {
                        goodDay++;
                    }
                }
            }

            var goodDayRatio = (goodDay / totalDays) + 0.5;
            // tie closer.
            return goodDayRatio;
        }
        public static double NormalizedProfitPerYear(BacktestRequest request, ICollection<RunResponse> results)
        {
            // npc: https://github.com/ondra-novak/mmbot/blob/141f74206f7b1938fa0903d20486f4962293ad1e/www/admin/code.js#L1872

            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var interval = last.Tm - first.Tm;
            var profit = Math.Max(last.Npl * 31536000000 / (interval * request.RunRequest.Balance), 0);

            if (profit == 0) return 0;

            return Normalize(profit, 0.3, 0.7, null);
        }
        public static double PnlProfitPerYear(BacktestRequest request, ICollection<RunResponse> results)
        {
            // pc: https://github.com/ondra-novak/mmbot/blob/141f74206f7b1938fa0903d20486f4962293ad1e/www/admin/code.js#L1873

            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var interval = last.Tm - first.Tm;
            var profit = Math.Max(last.Pl * 31536000000 / (interval * request.RunRequest.Balance), 0);

            if (profit == 0) return 0;

            return Normalize(profit, 0.3, 0.7, null);
        }
        public static double MaxCost(BacktestRequest request, ICollection<RunResponse> results)
        {
            double cost = 0;
            double maxCost = 0;

            foreach (var trade in results)
            {
                cost = cost + (trade.Sz * trade.Pr);
                if (cost > maxCost) { maxCost = cost; }
            }

            var balance = request.RunRequest.Balance;
            var budgetRatioInverse = 1 - (maxCost / balance);

            return budgetRatioInverse;
        }
        public static double LowerPositionOverall(BacktestRequest request, ICollection<RunResponse> results, double balancePercentage)
        {
            if (results.Count < 1) return 0;

            var balanceEval = balancePercentage * request.RunRequest.Balance;

            //All trades with position above x% of balance
            var tradesHighPosition = results.Count(x => x.Pr * x.Ps > balanceEval);

            var lowPosOverall = 1 - (double)tradesHighPosition / (double)results.Count();

            return lowPosOverall;
        }
        #endregion

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
            double tightenNplRpnlThreshold,
            double tightenEquityThreshold,
            double tightenNeutralPriceToLastThreshold,
            double howDeepToDive
        )
        {
            if (results.Count == 0) { return 0; }

            double deviatedTrades = 0;
            double resultsCounted = results.Count();

            int index = 0;

            foreach (var result in results)
            {
                double npl = result.Npl;
                double rpnl = result.Rpnl;
                double tradeSize = result.Sz;
                double percentageDiffCalculation = PercentageDifference(npl, rpnl);
                if (tradeSize != 0)
                {
                    if (percentageDiffCalculation > tightenNplRpnlThreshold) {

                        if ((index) > 0) { 
                            RunResponse whatHappenedBefore = results.ElementAt(index - 1);
                            if (whatHappenedBefore.Pr != 0)
                            {
                                double prBefore = whatHappenedBefore.Pr;
                                double prActual = result.Pr;
                                double percentageDiffPriceCalculation = PercentageDifference(prBefore, prActual);
                                if (percentageDiffPriceCalculation > 3.5) //Cenový rozdíl 3,5 procenta (dump/pump).
                                {
                                    //Nefunguje z nějakého důvodu, přestane obchodovat.
                                    //deviatedTrades += -1;
                                }
                            }
                        }

                        deviatedTrades += 1;
                        if (GetEquityToFollow(result, tightenEquityThreshold))
                        {
                            deviatedTrades += 1;
                            //if (TightenNeutralPriceToLast(result, tightenNeutralPriceToLastThreshold))
                            //{
                            //    deviatedTrades += 1;
                            //}
                        }
                    }

                    if (null != result.Info)
                    {
                        double budgetCurrent = result.Info.BudgetCurrent;
                        double budgetMax = result.Info.BudgetMax;
                        double percentageDiffBudgetCalc = PercentageDifference(budgetCurrent, budgetMax);

                        if (percentageDiffBudgetCalc > howDeepToDive) { deviatedTrades += 1; }
                    }
                }
                if (tradeSize == 0) { deviatedTrades += 1.5; }
                index++;
            }

            //slouží jako ratio. deviatedTrades může mít za jeden trade skóre až 3, přičemž se mu snižuje celková fitness na základě odpočtu
            //od celkového počtu obchodů.
            double deviatedTradesRatio = deviatedTrades / resultsCounted;
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

            //double deviatedTradesRatio = deviatedTrades / resultsCounted;
            //double deviationThresholdActual = 1 - deviatedTradesRatio;
            //return deviationThresholdActual;
        }

        public static bool TightenNeutralPriceToLast(
            RunResponse result,
            double tightenNeutralPriceToLast
        )
        {
            bool deviated = false;
            double priceLast = result.Info.PriceLast;
            double priceNeutral = result.Info.PriceNeutral;

            if((priceLast != 0) && (priceNeutral != 0))
            {
                double percentageDiffCalculation = PercentageDifference(priceLast, priceNeutral);
                if(percentageDiffCalculation > tightenNeutralPriceToLast)
                {
                deviated = true;
                }
                
            }
            return deviated;
        }


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
            return Normalize(result, 5, 30, null);
        }

        public static FitnessComposition NpaRRR(
            BacktestRequest request,
            ICollection<RunResponse> results
        )
        {
            if (results == null || results.Count == 0) return new FitnessComposition();

            #region Outdated
            //Jsem mnich a zisk mne nezajímá.
            //Ziskovost vychází z cenové oscilace daného páru. 
            //const double nppyWeight = 0.00;
            //const double pppyWeight = 0.00;
            //const double ipdrWeight = 0.00;
            //const double lpoWeight = 0.00;
            //const double maxCostWeight = 0.00;
            //const double tightenNeutralPriceWeight = 0.00; //Nefunguje moc dobře, nahrazeno NplRpnl. (bylo by zajímavé toto přezkoumat).
            //Balance nad 10% reportuj jako vysokou pozici.
            //const double balanceThreshold = 0.1;
            //Nad 2% deviace od neutrální ceny snižuj Fitness.
            //const double tightenNeutralPriceThreshold = 2; //Nefunguje při splitnutém grafu ! (Malém jsem se tady při ladění posral...)
            //const double tradeCountWeight = 0.00;
            #endregion

            const double rrrWeight = 0.00;
            const double tightenNplRpnlSubmergedWeight = 1.00;

            //tightenNplRpnl je proměnlivý údaj a velmi záleží na páru
            //chtělo by to matematickou rovnici která by určila optimální NplRpnl bez toho, aniž by overfitnul.
            //Napadla mne matice která by dle oscilace páru určila tento parametr. 
            //Je navázáno na Exponent u Gamma funkcí.
            const double tightenNplRpnlThreshold = 4; // 1.5% oscilace profit&loss kolem normalized profit.
            const double tightenEquityThreshold = 1; // 1% deviace od Equity.
            const double tightenNeutralPriceToLastThreshold = 0.5; // 0.5% deviace od neutrální ceny.
            const double howDeepToDive = 10; //DCAčkuj max. 10-ti % budgetu.

            //Debug
            //Debug.Assert(Math.Abs(nppyWeight + pppyWeight + ipdrWeight + lpoWeight + rrrWeight + tradeCountWeight + maxCostWeight + tightenNeutralPriceWeight + tightenNplRpnlWeight - 1) < 0.01);

            var eventCheck = CheckForEvents(results); //0-1, nic jiného nevrací.
            var result = new FitnessComposition();
            
            result.Fitness = (rrrWeight * (result.RRR = Rrr(results))
              //+ tradeCountWeight * (result.TradeCountFactor = TradeCountFactor(results))
              + tightenNplRpnlSubmergedWeight * (result.TightenNplRpnlSubmergedFunction = TightenNplRpnlSubmergedFunction(
                  results,
                  tightenNplRpnlThreshold,
                  tightenEquityThreshold,
                  tightenNeutralPriceToLastThreshold,
                  howDeepToDive)))
              * eventCheck;

            #region Outdated
            //result.Fitness = (nppyWeight * (result.NpProfitPerYear = NormalizedProfitPerYear(request, results))
            //  + pppyWeight * (result.PnlProfitPerYear = PnlProfitPerYear(request, results))
            //  + ipdrWeight * (result.IncomePerDayRatio = IncomePerDayRatio(results))
            //  + rrrWeight * (result.RRR = Rrr(results))
            //  + tradeCountWeight * (result.TradeCountFactor = TradeCountFactor(results))
            //  + lpoWeight * (result.LowerPositionFactor = LowerPositionOverall(request, results, balanceThreshold))
            //  + maxCostWeight * (result.MaxCostFactor = MaxCost(request, results))
            //  + tightenNeutralPriceWeight * (result.TightenNeutralPriceToLast = TightenNeutralPriceToLast(results, tightenNeutralPriceThreshold)))
            //  + tightenNplRpnlWeight * (result.TightenNplRpnl = TightenNplRpnl(results, tightenNplRpnlThreshold))
            //  * eventCheck;
            //var fitness = (nppyEval + pppyEval + ipdrEval + rrrEval + tradeCountEval + lowerPosEval + maxCostEval + minMaxBalanceTheBalanceEval) * eventCheck;
            //Formát výpisu zachovat, čárka a mezera se používají v LogAnalyzer.ps1 dle které se splitují hodnoty !
            //Log.Info($"Fitness : {fitness}, nppyEval : {nppyEval}, pppyEval : {pppyEval}, ipdrEval : {ipdrEval}, rrrEval : {rrrEval}, tradeCountEval : {tradeCountEval}, lowerPosEval : {lowerPosEval}, MaxCostEval : {maxCostEval}, EventCheck : {eventCheck}");
            #endregion

            return result;
        }

        public static double Normalize(
            double value, double target, double virtualMax, double? cap
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
                return percentageDiff;
            }

            return 0;
        }
    }
}