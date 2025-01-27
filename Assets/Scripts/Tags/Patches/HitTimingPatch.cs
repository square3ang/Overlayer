using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Overlayer.Tags.Patches
{
    public class HitTimingPatch 
    {
        [HarmonyPatch(typeof(scrPlanet), "SwitchChosen")]
        public static class TimingUpdater
        {
            public static List<double> Timings = new List<double>();
            public static void Prefix(scrPlanet __instance)
            {
                if (Main.IsPlaying)
                {
                    HitTiming.Timing = (__instance.angle - __instance.targetExitAngle) * (scrController.instance.isCW ? 1.0 : -1.0) * 60000.0 / (Math.PI * __instance.conductor.bpm * scrController.instance.speed * __instance.conductor.song.pitch);
                    Timings.Add(HitTiming.Timing);
                    HitTiming.TimingAvg = Timings.Average();
                }
                else HitTiming.Timing = 0;
            }
        }
        [HarmonyPatch(typeof(scrController), "Awake_Rewind")]
        public static class TimingResetter
        {
            public static void Postfix()
            {
                HitTiming.Timing = 0;
                TimingUpdater.Timings = new List<double>();
            }
        }
    }
}
