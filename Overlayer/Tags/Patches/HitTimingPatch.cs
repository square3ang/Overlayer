using Overlayer.Core.Patches;
using System.Collections.Generic;
using static Overlayer.Tags.Patches.MixPatch;

namespace Overlayer.Tags.Patches
{
    public class HitTimingPatch : PatchBase<HitTimingPatch>
    {
        [LazyPatch("Tags.HitTiming.TimingResetter", "scrController", "Awake_Rewind", Triggers = new string[] { nameof(HitTiming.Timing), nameof(HitTiming.TimingAvg) })]
        public static class TimingResetter
        {
            public static void Postfix()
            {
                HitTiming.Timing = 0;
                PatchSwitchChosen.Timings = new List<double>();
            }
        }
    }
}
