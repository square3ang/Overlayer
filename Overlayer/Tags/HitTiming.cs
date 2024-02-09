using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class HitTiming
    {
        [Tag(Flags = AdvancedFlags.Round)]
        public static double Timing;
        [Tag(Flags = AdvancedFlags.Round)]
        public static double TimingAvg;
        public static void Reset()
        {
            Timing = TimingAvg = 0;
        }
    }
}
