using Overlayer.Core.Patches;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overlayer.Tags.Patches {
    public class P_scrPlanet : PatchBase<P_scrPlanet> {
        [LazyPatch("Tags.P_scrPlanet.Bpm__MoveToNextFloor", "scrPlanet", "MoveToNextFloor", Triggers = new string[] {
            nameof(Bpm.TileBpm), nameof(Bpm.CurBpm), nameof(Bpm.RecKPS),
            nameof(Bpm.TileBpmWithoutPitch), nameof(Bpm.CurBpmWithoutPitch), nameof(Bpm.RecKPSWithoutPitch),
        })]
        public static class Bpm__MoveToNextFloor {
            public static void Postfix(scrFloor floor) {
                Bpm.Update(floor);
            }
        }

        [LazyPatch("Tags.P_scrPlanet.Status__MoveToNextFloor", "scrPlanet", "MoveToNextFloor", Triggers = new string[] {
            nameof(Status.CurCheckPoint), nameof(Status.BestProgress)
        })]
        public static class Status__MoveToNextFloor {
            public static void Postfix(scrFloor floor) {
                if(Status.AllCheckPoints != null) {
                    Status.CurCheckPoint = Status.GetCheckPointIndex(floor);
                }
                Status.BestProgress_Update();
            }
        }

        [LazyPatch("Tags.P_scrPlanet.HitTiming__SwitchChosen", "scrPlanet", "SwitchChosen", Triggers = new string[] {
            nameof(HitTiming.Timing), nameof(HitTiming.TimingAvg),
        })]
        public static class HitTiming__SwitchChosen {
            public static void Prefix(scrPlanet __instance) {
                if(Main.IsPlaying) {
                    HitTiming.Timing =
                        (__instance.angle - __instance.targetExitAngle)
                        * (scrController.instance.isCW ? 1.0 : -1.0)
                        * 60000.0
                        / (Math.PI * __instance.conductor.bpm * scrController.instance.speed * __instance.conductor.song.pitch);
                    HitTiming.Timings.Add(HitTiming.Timing);
                    HitTiming.TimingAvg = HitTiming.Timings.Average();
                } else {
                    HitTiming.Timing = 0;
                }
            }
        }

        [LazyPatch("Tags.P_scrPlanet.Tile__SwitchChosen", "scrPlanet", "SwitchChosen", Triggers = new string[] {
            nameof(Tile.CurTile), nameof(Tile.LeftTile), nameof(Tile.TotalTile),

            // Dependency
            nameof(Status.MaxAccuracy), nameof(Status.MaxXAccuracy), nameof(Status.AbsMaxXAccuracy),
        })]
        public static class Tile__SwitchChosen {
            public static void Postfix() {
                if(Main.IsPlaying) {
                    Tile.CurTile = scrController.instance.currentSeqID + 1;
                    Tile.TotalTile = ADOBase.lm.listFloors.Count;
                    Tile.LeftTile = Tile.TotalTile - Tile.CurTile;
                }
            }
        }
    }
}
