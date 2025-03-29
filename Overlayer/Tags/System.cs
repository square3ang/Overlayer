using Overlayer.Tags.Attributes;
using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;
using static Overlayer.Tags.Patches.BpmPatch;

namespace Overlayer.Tags {
    public static class System {
        public static double GCMemUsage;
        public static double GCMemUsageGB;
        public static double GCMemUsageKB;
        public static double UnityMemUsage;
        public static double UnityMemUsageGB;
        public static double UnityMemUsageKB;
        public static double GCMemAllocRate;
        public static double GCMemAllocRateGB;
        public static double GCMemAllocRateKB;

        private static long lastGCAllocatedMemory = GC.GetTotalMemory(false);
        private static Stopwatch gcStopwatch = Stopwatch.StartNew();

        private static Thread Update;
        public static bool inited { get; private set; }

        public static void Init() {
            if(inited) {
                return;
            }

            Update = new Thread(() => {
                while(true) {
                    long GCmem = GC.GetTotalMemory(false);
                    GCMemUsage = GCmem / 1024d / 1024d;
                    GCMemUsageGB = GCmem / 1024d / 1024d / 1024d;
                    GCMemUsageKB = GCmem / 1024d;
                    double gcmemalloc = GCMemoryAllocRateCheck(GCmem);
                    GCMemAllocRate = gcmemalloc / 1024d / 1024d;
                    GCMemAllocRateGB = gcmemalloc / 1024d / 1024d / 1024d;
                    GCMemAllocRateKB = gcmemalloc / 1024d;
                    double unitymem = Profiler.GetTotalAllocatedMemoryLong();
                    UnityMemUsage = unitymem / 1024d / 1024d;
                    UnityMemUsageGB = unitymem / 1024d / 1024d / 1024d;
                    UnityMemUsageKB = unitymem / 1024d;
                    Thread.Sleep(Main.Settings.SystemTagUpdateRate);
                }
            });
            Update.Start();
            inited = true;
        }

        [Tag("GCMemUsage", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double _GCMemUsage() => GCMemUsage;
        [Tag("GCMemUsageGB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double _GCMemUsageGB() => GCMemUsageGB;
        [Tag("GCMemUsageKB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double _GCMemUsageKB() => GCMemUsageKB;

        [Tag("GCMemAllocRate", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double _GCMemAllocRate() => GCMemAllocRate;
        [Tag("GCMemAllocRateGB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double _GCMemAllocRateGB() => GCMemAllocRateGB;
        [Tag("GCMemAllocRateKB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double _GCMemAllocRateKB() => GCMemAllocRateKB;

        [Tag("UnityMemUsage", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double _UnityMemUsage() => UnityMemUsage;
        [Tag("UnityMemUsageGB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double _UnityMemUsageGB() => UnityMemUsageGB;
        [Tag("UnityMemUsageKB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double _UnityMemUsageKB() => UnityMemUsageKB;

        private static double GCMemoryAllocRateCheck(long currentMemory) {
            double elapsedSeconds = gcStopwatch.Elapsed.TotalSeconds;
            gcStopwatch.Restart();
            long allocatedSinceLastCheck = currentMemory - lastGCAllocatedMemory;

            lastGCAllocatedMemory = currentMemory;

            return allocatedSinceLastCheck / elapsedSeconds;
        }

        public static void Free() {
            inited = false;
            try {
                Update?.Abort();
                Update = null;
            } catch { } finally { Update = null; }
        }
    }
}
