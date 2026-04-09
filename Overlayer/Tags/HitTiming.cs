using Overlayer.Tags.Attributes;
using System.Collections.Generic;

namespace Overlayer.Tags;

public static class HitTiming {
    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Error range showing the timing of keystrokes in ms from the center of the tile")]
    public static double Timing;
    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Average of Timing")]
    public static double TimingAvg;

    public static List<double> Timings = [];

    public static void Reset() => Timing = TimingAvg = 0;
}
