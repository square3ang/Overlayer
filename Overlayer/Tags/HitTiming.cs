using Overlayer.Tags.Attributes;
using System.Collections.Generic;

namespace Overlayer.Tags;

public static class HitTiming {
    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double Timing;
    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double TimingAvg;

    public static List<double> Timings = new();

    public static void Reset() => Timing = TimingAvg = 0;
}
