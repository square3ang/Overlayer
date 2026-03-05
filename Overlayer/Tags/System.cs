using Overlayer.Tags.Attributes;
using System;
using System.Threading;
using UnityEngine.Profiling;

namespace Overlayer.Tags;

public static class System {
    [Tag("GCMemUsage", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemUsage;
    [Tag("GCMemUsageGB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemUsageGB;
    [Tag("GCMemUsageKB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemUsageKB;

    [Tag("GCMemAllocRate", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemAllocRate;
    [Tag("GCMemAllocRateGB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemAllocRateGB;
    [Tag("GCMemAllocRateKB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemAllocRateKB;

    [Tag("UnityMemUsage", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double UnityMemUsage;
    [Tag("UnityMemUsageGB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double UnityMemUsageGB;
    [Tag("UnityMemUsageKB", NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double UnityMemUsageKB;

    private static long lastGCAllocatedMemory = GC.GetTotalMemory(false);

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

    private static double GCMemoryAllocRateCheck(long currentMemory) {
        long rate = currentMemory - lastGCAllocatedMemory;
        lastGCAllocatedMemory = currentMemory;
        return rate;
    }

    public static void Free() {
        inited = false;
        try {
            Update?.Abort();
            Update = null;
        } catch { } finally { Update = null; }
    }
}
