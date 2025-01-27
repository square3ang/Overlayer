namespace Overlayer.Tags
{
    public static class HitTiming
    {
        [Tag(Dummy=1)]
        public static double Timing;
        [Tag(Dummy=1)]
        public static double TimingAvg;
        public static void Reset()
        {
            Timing = TimingAvg = 0;
        }
    }
}
