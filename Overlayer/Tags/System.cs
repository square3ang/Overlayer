using Overlayer.Tags.Attributes;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine.Profiling;
using Vostok.Sys.Metrics.PerfCounters;

namespace Overlayer.Tags;

public static class System {
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemUsageGB;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemUsageKB;

    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemAllocRate;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemAllocRateGB;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double GCMemAllocRateKB;

    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double UnityMemUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double UnityMemUsageGB;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double UnityMemUsageKB;

    [Tag(NotPlaying = true)]
    public static int ProcessorCount;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double CpuUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double TotalCpuUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double MemoryUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double TotalMemoryUsage;

    private static long lastGCAllocatedMemory;
    private static Thread updateThread;
    private static volatile bool running;

    private static bool inited;

    public static void Init() {
        if(inited) {
            return;
        }

        ProcessorCount = Environment.ProcessorCount;
        lastGCAllocatedMemory = GC.GetTotalMemory(false);

        IPerformanceCounter<double> cpu = null;
        IPerformanceCounter<double> totCpu = null;
        IPerformanceCounter<double> mem = null;
        IPerformanceCounter<double> totMem = null;
        ulong totalMemMB = 0;

        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            var proc = Process.GetCurrentProcess();
            totalMemMB = MemoryStatus.GetMemoryStatus().TotalPhysicalMemorySize / 1048576;

            cpu = PerformanceCounterFactory.Default.CreateCounter("Process", "% Processor Time", proc.ProcessName);
            mem = PerformanceCounterFactory.Default.CreateCounter("Process", "Working Set", proc.ProcessName);
            totCpu = PerformanceCounterFactory.Default.CreateCounter("Processor", "% Processor Time", "_Total");
            totMem = PerformanceCounterFactory.Default.CreateCounter("Memory", "Available MBytes");
        }

        running = true;
        updateThread = new Thread(() => {
            while(running) {
                long gc = GC.GetTotalMemory(false);
                GCMemUsage = gc / 1024d / 1024d;
                GCMemUsageGB = gc / 1024d / 1024d / 1024d;
                GCMemUsageKB = gc / 1024d;

                long delta = gc - lastGCAllocatedMemory;
                lastGCAllocatedMemory = gc;
                GCMemAllocRate = delta / 1024d / 1024d;
                GCMemAllocRateGB = delta / 1024d / 1024d / 1024d;
                GCMemAllocRateKB = delta / 1024d;

                double unity = Profiler.GetTotalAllocatedMemoryLong();
                UnityMemUsage = unity / 1024d / 1024d;
                UnityMemUsageGB = unity / 1024d / 1024d / 1024d;
                UnityMemUsageKB = unity / 1024d;

                if(cpu != null) {
                    CpuUsage = cpu.Observe() / ProcessorCount;
                    TotalCpuUsage = totCpu.Observe();

                    var memUsage = mem.Observe() / 1048576;
                    var usedTotal = totalMemMB - totMem.Observe();

                    MemoryUsage = memUsage / totalMemMB * 100d;
                    TotalMemoryUsage = usedTotal / totalMemMB * 100d;
                }

                Thread.Sleep(Main.Settings.SystemTagUpdateRate);
            }
        });

        updateThread.Start();
        inited = true;
    }

    public static void Free() {
        if(!inited) {
            return;
        }

        running = false;
        updateThread?.Join();
        updateThread = null;
        inited = false;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class MemoryStatus {
        public uint Length = (uint)Marshal.SizeOf<MemoryStatus>();
        public uint MemoryLoad;
        public ulong TotalPhysicalMemorySize;
        public ulong AvailablePhysicalMemorySize;
        public ulong TotalPageFileSize;
        public ulong AvailablePageFileSize;
        public ulong TotalVirtualMemorySize;
        public ulong AvailableVirtualMemorySize;
        public ulong AvailableExtendedVirtualMemorySize;

        [DllImport("kernel32.dll")]
        static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatus lpBuffer);

        public static MemoryStatus GetMemoryStatus() {
            var s = new MemoryStatus();
            GlobalMemoryStatusEx(s);
            return s;
        }
    }
}