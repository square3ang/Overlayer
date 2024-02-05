using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class HitTiming
    {
        [Tag(Flags = AdvancedFlags.Round, Hint = ReturnTypeHint.Double)]
        public static double Timing;
        [Tag(Flags = AdvancedFlags.Round, Hint = ReturnTypeHint.Double)]
        public static double TimingAvg;
        public static void Reset()
        {
            Timing = TimingAvg = 0;
        }
    }
}
