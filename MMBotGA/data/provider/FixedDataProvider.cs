//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Downloader.Core.Core;
//using MMBotGA.data.exchange;
//using MMBotGA.downloader;
//using MMBotGA.ga.abstraction;

//namespace MMBotGA.data.provider
//{

//    internal class FixedDataProvider : IDataProvider
//    {
//        private const string DataFolder = "data";

//        protected virtual DataProviderSettings Settings => new()
//        {
//            Allocations = AllocationDefinitions.Select(x => x.ToAllocation()).ToArray(),
//            DateSettings = new DataProviderDateSettings
//            {
//                Automatic = true
//            }
//        };

//        private static IEnumerable<AllocationDefinition> AllocationDefinitions => new AllocationDefinition[]
//        {
//            //TODO : Dynamické runy, allocationDefinition Ilist ? 
//            //new()
//            //{
//            //    Exchange = Exchange.Binance,
//            //    Pair = new Pair("ADAUP", "USDT"),
//            //    Balance = 1000
//            //},
//            //new()
//            //{
//            //    Exchange = Exchange.Ftx,
//            //    Pair = new Pair("ATOM", "PERP"),
//            //    Balance = 1000
//            //},
//            //new()
//            //{
//            //    Exchange = Exchange.Ftx,
//            //    Pair = new Pair("DOT", "PERP"),
//            //    Balance = 1000
//            //},
//            //new()
//            //{
//            //    Exchange = Exchange.Ftx,
//            //    Pair = new Pair("LUNA", "PERP"),
//            //    Balance = 1000
//            //},
//            //new()
//            //{
//            //    Exchange = Exchange.Ftx,
//            //    Pair = new Pair("SOL", "PERP"),
//            //    Balance = 1000
//            //},
//            //new()
//            //{
//            //    Exchange = Exchange.Ftx,
//            //    Pair = new Pair("CAKE", "PERP"),
//            //    Balance = 1000
//            //},
//            //new()
//            //{
//            //    Exchange = Exchange.Ftx,
//            //    Pair = new Pair("UNI", "PERP"),
//            //    Balance = 1000
//            //},
//            //new()
//            //{
//            //    Exchange = Exchange.Ftx,
//            //    Pair = new Pair("MATIC", "PERP"),
//            //    Balance = 1000
//            //},
//            //new()
//            //{
//            //    Exchange = Exchange.Ftx,
//            //    Pair = new Pair("AVAX", "PERP"),
//            //    Balance = 1000
//            //},
//            new()
//            {
//                Exchange = Exchange.Ftx,
//                Pair = new Pair("SOL", "PERP"),
//                Balance = 1000
//            }

//        };


//        public Batch[] GetBacktestData(IProgress progressCallback)
//        {
//            //File.WriteAllText("allocations.json", JsonConvert.SerializeObject(Settings, Formatting.Indented)); 

//            var downloader = new DefaultDownloader(progressCallback);

//            //divide to 3x graphs, more fluent, without spikes. 
//            var backtestRange = Settings.DateSettings.Automatic
//                ? DateTimeRange.FromDiff(DateTime.UtcNow.Date, TimeSpan.FromDays(-120))
//                : Settings.DateSettings.Backtest;
//            var backtestRangeTwo = Settings.DateSettings.Automatic
//                ? DateTimeRange.FromDiff(DateTime.UtcNow.Date.AddDays(-120), TimeSpan.FromDays(-120))
//                : Settings.DateSettings.Backtest;
//            var backtestRangeThree = Settings.DateSettings.Automatic
//                ? DateTimeRange.FromDiff(DateTime.UtcNow.Date.AddDays(-240), TimeSpan.FromDays(-120))
//                : Settings.DateSettings.Backtest;

//            List<Batch> settingsAllocations = new List<Batch>();

//            //Batch backtestRangeDyn = new Batch(settingsAllocations.Select(x => x)));
//            //settingsAllocations.Add();

//            return Settings.Allocations
//                .Select(x => new Batch(x.ToBatchName(),
//                    new[]
//                    {
//                        //downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRange), false, x.Balance),
//                        //downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRange), true, x.Balance),
//                        //downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRangeTwo), true, x.Balance),
//                        //downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRangeThree), true, x.Balance),
//                        downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRange), false, x.Balance),
//                        downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRangeTwo), false, x.Balance),
//                        downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRangeThree), false, x.Balance)
//                    }))
//                .ToArray();
//        }

//        public Batch[] GetControlData(IProgress progressCallback)
//        {
//            var downloader = new DefaultDownloader(progressCallback);
//            var backtestRange = Settings.DateSettings.Automatic
//                ? DateTimeRange.FromUtcToday(TimeSpan.FromDays(-60))
//                : Settings.DateSettings.Control;

//            return Settings.Allocations
//                .Select(x => new Batch(x.ToBatchName(),
//                    new[]
//                    {
//                        downloader.GetBacktestData(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRange), false, x.Balance)
//                    }))
//                .ToArray();
//        }
//    }
//}

using System;
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
                Automatic = true,
                Backtest = DateTimeRange.FromDiff(DateTime.UtcNow.Date.AddDays(-60), TimeSpan.FromDays(-365)),
                Control = DateTimeRange.FromDiff(new DateTime(2022, 1, 4, 0, 0, 0, DateTimeKind.Utc), TimeSpan.FromDays(-60))
            }
        };

        private static IEnumerable<AllocationDefinition> AllocationDefinitions => new AllocationDefinition[]
        {
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
                Pair = new Pair("ETH", "PERP"),
                Balance = 1000
            }
        };

        public Batch[] GetBacktestData(IProgress progressCallback)
        {
            //File.WriteAllText("allocations.json", JsonConvert.SerializeObject(Settings, Formatting.Indented)); 

            var downloader = new DefaultDownloader(progressCallback);
            var backtestRange = Settings.DateSettings.Automatic
                ? DateTimeRange.FromDiff(DateTime.UtcNow.Date.AddDays(-60), TimeSpan.FromDays(-365))
                : Settings.DateSettings.Backtest;

            const int splits = 3;
            var diff = backtestRange.End - backtestRange.Start;
            var partMinutes = (int)diff.TotalMinutes / splits;
            var halfPartMinutes = partMinutes / 2;

            var offsets = Enumerable
                .Repeat(partMinutes, splits)
                .Select((p, i) => p * i)
                .Concat(Enumerable
                    .Repeat(partMinutes, splits - 1)
                    .Select((p, i) => halfPartMinutes + p * i)
                );

            return Settings.Allocations
                .Select(x => new Batch(x.ToBatchName(),
                    offsets
                        .Select(o => downloader.GetBacktestData(x, DataFolder, backtestRange, false, limit: partMinutes, offset: o))
                        .ToArray()
                ))
                .ToArray();

            //var partDays = (int)diff.TotalDays / splits;
            //var overlapStart = backtestRange.Start.AddDays(partDays / 2);
            //var parts = Enumerable
            //    .Repeat(partDays, splits)
            //    .Select((p, i) => DateTimeRange.FromDiff(backtestRange.Start.AddDays(p * i), TimeSpan.FromDays(p)))
            //    .Concat(Enumerable
            //        .Repeat(partDays, splits - 1)
            //        .Select((p, i) => DateTimeRange.FromDiff(overlapStart.AddDays(p * i), TimeSpan.FromDays(p)))
            //    );

            //return Settings.Allocations
            //    .Select(x => new Batch(x.ToBatchName(),
            //        new[]
            //        {
            //            downloader.GetBacktestData(x, DataFolder, backtestRange, false),
            //            downloader.GetBacktestData(x, DataFolder, backtestRange, true)
            //        }))
            //    .ToArray();
        }

        public Batch[] GetControlData(IProgress progressCallback)
        {
            var downloader = new DefaultDownloader(progressCallback);
            var controlRange = Settings.DateSettings.Automatic
                ? DateTimeRange.FromUtcToday(TimeSpan.FromDays(-60))
                : Settings.DateSettings.Control;

            return Settings.Allocations
                .Select(x => new Batch(x.ToBatchName(),
                    new[]
                    {
                        downloader.GetBacktestData(x, DataFolder, controlRange, false)
                    }))
                .ToArray();
        }
    }
}