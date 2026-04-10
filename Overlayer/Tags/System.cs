using LibreHardwareMonitor.Hardware;
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
    [TagDesc("Shows the total memory used by the Garbage Collector in MB")]
    public static float GCMemUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total memory used by the Garbage Collector in GB")]
    public static float GCMemUsageGB;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total memory used by the Garbage Collector in KB")]
    public static float GCMemUsageKB;

    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the allocation rate of memory by the Garbage Collector in MB per tick")]
    public static float GCMemAllocRate;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the allocation rate of memory by the Garbage Collector in GB per tick")]
    public static float GCMemAllocRateGB;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the allocation rate of memory by the Garbage Collector in KB per tick")]
    public static float GCMemAllocRateKB;

    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total memory used by Unity, not including the entire process memory.\nOnly Windows Available")]
    public static float UnityMemUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total memory used by Unity in GB, not including the entire process memory.\nOnly Windows Available")]
    public static float UnityMemUsageGB;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total memory used by Unity in KB, not including the entire process memory.\nOnly Windows Available")]
    public static float UnityMemUsageKB;

    [Tag(NotPlaying = true)]
    [TagDesc("Shows the number of CPU processors available on the system")]
    public static int ProcessorCount;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Average CPU usage per core of the process.\nOnly Windows Available")]
    public static float CpuUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total CPU usage percentage of the entire system.\nOnly Windows Available")]
    public static float TotalCpuUsage;

    [Tag(NotPlaying = true)]
    [TagDesc("Shows the total physical memory (RAM) available on the system in MB.\nOnly Windows Available")]
    public static float Memory;
    [Tag(NotPlaying = true)]
    [TagDesc("Shows the total physical memory (RAM) available on the system in GB.\nOnly Windows Available")]
    public static float MemoryGBytes;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the current memory usage of the ADOFAI process in MB.\nOnly Windows Available")]
    public static float MemoryUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the current memory usage of the ADOFAI process in GB\nOnly Windows Available")]
    public static float MemoryUsageGBytes;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total memory usage of the system in MB.\nOnly Windows Available")]
    public static float TotalMemoryUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total memory usage of the system in GB.\nOnly Windows Available")]
    public static float TotalMemoryUsageGBytes;


    [Tag(NotPlaying = true)]
    [TagDesc("Shows the total GPU memory available on the system in MB.\nOnly Windows Available")]
    public static float GpuMemory;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total GPU memory available on the system in GB.\nOnly Windows Available")]
    public static float GpuMemoryGBytes;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total GPU memory usage on the system in MB.\nOnly Windows Available")]
    public static float GpuMemoryUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total GPU memory usage on the system in GB.\nOnly Windows Available")]
    public static float GpuMemoryUsageGBytes;
    [Tag(NotPlaying = true)]
    [TagDesc("Shows the current GPU usage as a percentage of total GPU capacity.\nOnly Windows Available")]
    public static int GpuUsage;

    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total shared GPU memory available on the system in MB.\nOnly Windows Available")]
    public static float GpuSharedMemory;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total shared GPU memory available on the system in GB.\nOnly Windows Available")]
    public static float GpuSharedMemoryGBytes;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total shared GPU memory usage on the system in MB.\nOnly Windows Available")]
    public static float GpuSharedMemoryUsage;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Shows the total shared GPU memory usage on the system in GB.\nOnly Windows Available")]
    public static float GpuSharedMemoryUsageGBytes;

    const int GPU_HISTORY = 16;
    static int[] gpuHistory = new int[GPU_HISTORY];
    static int gpuIndex = 0;
    static int gpuCount = 0;

    private static long lastGCAllocatedMemory;
    private static Thread updateThread;
    private static Computer computer;
    private static volatile bool running;
    private static bool inited;

    public static void Init() {
        if(inited) { return; }

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
            Memory = totalMemMB;
            MemoryGBytes = totalMemMB / 1024f;

            cpu = PerformanceCounterFactory.Default.CreateCounter("Process", "% Processor Time", proc.ProcessName);
            mem = PerformanceCounterFactory.Default.CreateCounter("Process", "Working Set", proc.ProcessName);
            totCpu = PerformanceCounterFactory.Default.CreateCounter("Processor", "% Processor Time", "_Total");
            totMem = PerformanceCounterFactory.Default.CreateCounter("Memory", "Available MBytes");

            computer = new Computer {
                IsGpuEnabled = true
            };
            computer.Open();
        }

        running = true;
        updateThread = new Thread(() => {
            while(running) {
                long gc = GC.GetTotalMemory(false);
                GCMemUsage = gc / 1024f / 1024f;
                GCMemUsageGB = gc / 1024f / 1024f / 1024f;
                GCMemUsageKB = gc / 1024f;

                long delta = gc - lastGCAllocatedMemory;
                lastGCAllocatedMemory = gc;
                GCMemAllocRate = delta / 1024f / 1024f;
                GCMemAllocRateGB = delta / 1024f / 1024f / 1024f;
                GCMemAllocRateKB = delta / 1024f;

                float unity = Profiler.GetTotalAllocatedMemoryLong();
                UnityMemUsage = unity / 1024f / 1024f;
                UnityMemUsageGB = unity / 1024f / 1024f / 1024f;
                UnityMemUsageKB = unity / 1024f;

                if(cpu != null) {
                    CpuUsage = (float)cpu.Observe() / ProcessorCount;
                    TotalCpuUsage = (float)totCpu.Observe();
                }

                if(mem != null) {
                    float memUsageMB = (float)mem.Observe() / 1048576f;
                    float usedTotalMB = totalMemMB - (float)totMem.Observe();

                    MemoryUsage = memUsageMB;
                    TotalMemoryUsage = usedTotalMB;

                    MemoryUsageGBytes = memUsageMB / 1024f;
                    TotalMemoryUsageGBytes = usedTotalMB / 1024f;
                }

                if(computer != null) {
                    int rawMax = 0;

                    float dUsed = 0, dTotal = 0;
                    float sUsed = 0, sTotal = 0;

                    foreach(var hw in computer.Hardware) {
                        hw.Update();

                        int localUsage = 0;
                        float ldUsed = 0, ldTotal = 0;
                        float lsUsed = 0, lsTotal = 0;

                        foreach(var s in hw.Sensors) {
                            if(s.Value == null) { continue; }

                            if(s.SensorType == SensorType.Load && s.Name.Contains("Core")) {
                                localUsage = Math.Max(localUsage, (int)s.Value.Value);
                            }

                            if(s.SensorType == SensorType.SmallData) {
                                if(s.Name.Contains("Dedicated Memory Used")) { ldUsed = s.Value.Value; }
                                if(s.Name.Contains("Dedicated Memory Total")) { ldTotal = s.Value.Value; }
                                if(s.Name.Contains("Shared Memory Used")) { lsUsed = s.Value.Value; }
                                if(s.Name.Contains("Shared Memory Total")) { lsTotal = s.Value.Value; }
                            }
                        }

                        if(localUsage > rawMax) {
                            rawMax = localUsage;
                            dUsed = ldUsed;
                            dTotal = ldTotal;
                            sUsed = lsUsed;
                            sTotal = lsTotal;
                        }
                    }

                    gpuHistory[gpuIndex] = rawMax;
                    gpuIndex = (gpuIndex + 1) % GPU_HISTORY;

                    if(gpuCount < GPU_HISTORY) {
                        gpuCount++;
                    }

                    int sum = 0;
                    for(int i = 0; i < gpuCount; i++) {
                        sum += gpuHistory[i];
                    }

                    int avg = sum / gpuCount;

                    GpuUsage = avg;

                    if(dTotal > 0) {
                        GpuMemoryUsage = dUsed;
                    }

                    if(sTotal > 0) {
                        GpuSharedMemoryUsage = sUsed;
                    }

                    GpuMemory = dTotal;
                    GpuMemoryGBytes = dTotal / 1024f;
                    GpuMemoryUsageGBytes = dUsed / 1024f;

                    GpuSharedMemory = sTotal;
                    GpuSharedMemoryGBytes = sTotal / 1024f;
                    GpuSharedMemoryUsageGBytes = sUsed / 1024f;
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