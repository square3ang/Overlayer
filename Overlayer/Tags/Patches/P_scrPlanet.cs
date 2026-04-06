using Overlayer.Core.Patches;
using System;
using System.Linq;

namespace Overlayer.Tags.Patches;

public class P_scrPlanet : PatchBase<P_scrPlanet> {
    [LazyPatch("Tags.P_scrPlanet.Bpm__MoveToNextFloor", "scrPlanet", "MoveToNextFloor", Triggers =
    [
        nameof(Bpm.TileBpm), nameof(Bpm.CurBpm), nameof(Bpm.RecKPS),
        nameof(Bpm.TileBpmWithoutPitch), nameof(Bpm.CurBpmWithoutPitch), nameof(Bpm.RecKPSWithoutPitch)
    ])]
    public static class Bpm__MoveToNextFloor {
        public static void Postfix(scrFloor floor) => Bpm.Update(floor);
    }

    [LazyPatch("Tags.P_scrPlanet.CheckPoint__MoveToNextFloor", "scrPlanet", "MoveToNextFloor", Triggers =
    [
        nameof(CheckPointStats.CurCheckPoint)
    ])]
    public static class CheckPoint__MoveToNextFloor {
        public static void Postfix(scrFloor floor) {
            if(CheckPointStats.AllCheckPoints != null) {
                CheckPointStats.CurCheckPoint = CheckPointStats.GetCheckPointIndex(floor);
            }
        }
    }

    [LazyPatch("Tags.P_scrPlanet.ProgressStats__MoveToNextFloor", "scrPlanet", "MoveToNextFloor", Triggers =
    [
        nameof(ProgressStats.BestProgress)
    ])]
    public static class ProgressStats__MoveToNextFloor {
        public static void Postfix() => ProgressStats.BestProgress_Update();
    }

    [LazyPatch("Tags.P_scrPlanet.Tile__MoveToNextFloor", "scrPlanet", "MoveToNextFloor", Triggers =
    [
        nameof(Tile.TileAngle), nameof(Tile.TileEntryAngle), nameof(Tile.TileExitAngle)
    ])]
    public static class Tile__MoveToNextFloor {
        public static void Postfix(scrFloor floor) => Tile.Angle_Update(floor);
    }

    [LazyPatch("Tags.P_scrPlanet.HitTiming__SwitchChosen", "scrPlanet", "SwitchChosen", Triggers =
    [
        nameof(HitTiming.Timing), nameof(HitTiming.TimingAvg)
    ])]
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

    [LazyPatch("Tags.P_scrPlanet.Tile__SwitchChosen", "scrPlanet", "SwitchChosen", Triggers =
    [
        nameof(Tile.CurTile), nameof(Tile.LeftTile), nameof(Tile.TotalTile),

        // Dependency
        nameof(AccuracyStats.MaxAccuracy), nameof(AccuracyStats.MaxXAccuracy), nameof(AccuracyStats.AbsMaxXAccuracy)
    ])]
    public static class Tile__SwitchChosen {
        public static void Postfix() {
            if(Main.IsPlaying) {
                Tile.CurTile = scrController.instance.currentSeqID;
                Tile.TotalTile = ADOBase.lm.listFloors.Count;
                Tile.LeftTile = Tile.TotalTile - Tile.CurTile;
            }
        }
    }
}
