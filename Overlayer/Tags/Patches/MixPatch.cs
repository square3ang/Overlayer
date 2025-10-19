using Overlayer.Core.Patches;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overlayer.Tags.Patches {
    public class MixPatch {
        [LazyPatch("Tags.MixPatch.PatchSwitchChosen", "scrPlanet", "SwitchChosen", Triggers = new string[] { nameof(HitTiming.Timing), nameof(HitTiming.TimingAvg) })]
        public static class PatchSwitchChosen {
            public static List<double> Timings = new List<double>();
            public static void Prefix(scrPlanet __instance) {
                if(Main.IsPlaying) {
                    HitTiming.Timing = (__instance.angle - __instance.targetExitAngle) * (scrController.instance.isCW ? 1.0 : -1.0) * 60000.0 / (Math.PI * __instance.conductor.bpm * scrController.instance.speed * __instance.conductor.song.pitch);
                    Timings.Add(HitTiming.Timing);
                    HitTiming.TimingAvg = Timings.Average();
                } else {
                    HitTiming.Timing = 0;
                }
            }

            public static void Postfix() {
                if(!Main.IsPlaying) {
                    return;
                }
                Tile.CurTile = scrController.instance.currentSeqID + 1;
                Tile.TotalTile = ADOBase.lm.listFloors.Count;
                Tile.LeftTile = Tile.TotalTile - Tile.CurTile;
            }
        }
    }
}
