﻿using System;
using System.Collections.Generic;
using System.Linq;
using Downloader.Core.Core;
using MMBotGA.data.exchange;
using MMBotGA.downloader;
using MMBotGA.ga.abstraction;

namespace MMBotGA.data.provider
{

    internal class FixedDataProvider : IDataProvider
    {
        private const string DataFolder = "data";

        protected virtual DataProviderSettings Settings => new()
        {
            Allocations = AllocationDefinitions.Select(x => x.ToAllocation()).ToArray(),
            DateSettings = new DataProviderDateSettings
            {
                Automatic = true
            }
        };

        private static IEnumerable<AllocationDefinition> AllocationDefinitions => new AllocationDefinition[]
        {
            //TODO : Dynamické runy, allocationDefinition Ilist ? 
            //new()
            //{
            //    Exchange = Exchange.Binance,
            //    Pair = new Pair("ADAUP", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Ftx,
            //    Pair = new Pair("ATOM", "PERP"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Ftx,
            //    Pair = new Pair("DOT", "PERP"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Ftx,
            //    Pair = new Pair("LUNA", "PERP"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Ftx,
            //    Pair = new Pair("SOL", "PERP"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Ftx,
            //    Pair = new Pair("CAKE", "PERP"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Ftx,
            //    Pair = new Pair("UNI", "PERP"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Ftx,
            //    Pair = new Pair("MATIC", "PERP"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Ftx,
            //    Pair = new Pair("AVAX", "PERP"),
            //    Balance = 1000
            //},
            new()
            {
                Exchange = Exchange.Ftx,
                Pair = new Pair("SOL", "PERP"),
                Balance = 1000
            }

        };


        public Batch[] GetBacktestData(IProgress progressCallback)
        {
            //File.WriteAllText("allocations.json", JsonConvert.SerializeObject(Settings, Formatting.Indented)); 

            var downloader = new DefaultDownloader(progressCallback);

            //divide to 3x graphs, more fluent, without spikes. 
            var backtestRange = Settings.DateSettings.Automatic
                ? DateTimeRange.FromDiff(DateTime.UtcNow.Date, TimeSpan.FromDays(-120))
                : Settings.DateSettings.Backtest;
            var backtestRangeTwo = Settings.DateSettings.Automatic
                ? DateTimeRange.FromDiff(DateTime.UtcNow.Date.AddDays(-120), TimeSpan.FromDays(-120))
                : Settings.DateSettings.Backtest;
            var backtestRangeThree = Settings.DateSettings.Automatic
                ? DateTimeRange.FromDiff(DateTime.UtcNow.Date.AddDays(-240), TimeSpan.FromDays(-120))
                : Settings.DateSettings.Backtest;

            List<Batch> settingsAllocations = new List<Batch>();

            //Batch backtestRangeDyn = new Batch(settingsAllocations.Select(x => x)));
            //settingsAllocations.Add();

            return Settings.Allocations
                .Select(x => new Batch(x.ToBatchName(),
                    new[]
                    {
                        //downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRange), false, x.Balance),
                        //downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRange), true, x.Balance),
                        //downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRangeTwo), true, x.Balance),
                        //downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRangeThree), true, x.Balance),
                        downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRange), false, x.Balance),
                        downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRangeTwo), false, x.Balance),
                        downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRangeThree), false, x.Balance)
                    }))
                .ToArray();
        }

        public Batch[] GetControlData(IProgress progressCallback)
        {
            var downloader = new DefaultDownloader(progressCallback);
            var backtestRange = Settings.DateSettings.Automatic
                ? DateTimeRange.FromUtcToday(TimeSpan.FromDays(-60))
                : Settings.DateSettings.Control;

            return Settings.Allocations
                .Select(x => new Batch(x.ToBatchName(),
                    new[]
                    {
                        downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRange), false, x.Balance)
                    }))
                .ToArray();
        }
    }
}