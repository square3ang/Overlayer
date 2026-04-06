using System;
using Overlayer.Core.Patches;

namespace Overlayer.Tags.Patches;

public class P_scrMistakeManager : PatchBase<P_scrMistakeManager> {
    [LazyPatch("Tags.P_scrMistakeManager.AccuracyStats__CalculatePercentAcc", "scrMistakesManager",
        "CalculatePercentAcc", Triggers = [
            nameof(AccuracyStats.Accuracy), nameof(AccuracyStats.MaxAccuracy),
            nameof(AccuracyStats.XAccuracy), nameof(AccuracyStats.MaxXAccuracy),
            nameof(AccuracyStats.AbsXAccuracy), nameof(AccuracyStats.AbsMaxXAccuracy)
        ])]
    public static class AccuracyStats__CalculatePercentAcc {
        public static void Postfix(scrMistakesManager __instance) {
            var perfect = __instance.GetHits(HitMargin.Perfect);
            var auto = __instance.GetHits(HitMargin.Auto);
            var earlyPerfect = __instance.GetHits(HitMargin.EarlyPerfect);
            var latePerfect = __instance.GetHits(HitMargin.LatePerfect);
            var veryEarly = __instance.GetHits(HitMargin.VeryEarly);
            var veryLate = __instance.GetHits(HitMargin.VeryLate);
            var tooEarly = __instance.GetHits(HitMargin.TooEarly);
            var tooLate = __instance.GetHits(HitMargin.TooLate);
            var failMiss = __instance.GetHits(HitMargin.FailMiss);
            var failOverload = __instance.GetHits(HitMargin.FailOverload);

            var success = perfect + earlyPerfect + latePerfect + auto;
            var total = scrMistakesManager.hitMargins.Count + failMiss + failOverload;
            var ratio = success == total ? 1.0 : (double)success / total;
            var bonus = (perfect + auto) * 0.0001;

            AccuracyStats.Accuracy = 100.0 * (ratio + bonus);

            double totalHits = scrMistakesManager.hitMargins.Count;
            var weightedHits =
                perfect + auto +
                0.75 * (earlyPerfect + latePerfect) +
                0.4 * (veryEarly + veryLate) +
                0.2 * (tooEarly + tooLate);

            var checkpointminus = Math.Pow(0.9875, scrController.checkpointsUsed);
            AccuracyStats.AbsXAccuracy = 100.0 * (weightedHits / totalHits);
            AccuracyStats.XAccuracy = AccuracyStats.AbsXAccuracy * checkpointminus;

            if (ADOBase.lm is not null && ADOBase.lm.listFloors != null &&
                Tile.CurTile >= 0 && Tile.CurTile < ADOBase.lm.listFloors.Count &&
                ADOBase.lm.listFloors[Tile.CurTile] != null) {
                var lefttile = Tile.LeftTile - (ADOBase.lm.listFloors[Tile.CurTile].midSpin ? 1 : 0);

                var mxsucess = lefttile + perfect + auto + earlyPerfect + latePerfect;
                var mxtotal = scrMistakesManager.hitMargins.Count + lefttile + failMiss + failOverload;
                var mxratio = mxsucess == mxtotal ? 1.0 : (double)mxsucess / mxtotal;
                var mxbonus = (lefttile + perfect + auto) * 0.0001;

                AccuracyStats.MaxAccuracy = 100.0 * (mxratio + mxbonus);

                var possibleHitsX =
                    lefttile + perfect + auto +
                    0.75 * (earlyPerfect + latePerfect) +
                    0.4 * (veryEarly + veryLate) +
                    0.2 * (tooEarly + tooLate);

                var denomX = lefttile + totalHits;
                AccuracyStats.AbsMaxXAccuracy = 100.0 * (possibleHitsX / denomX);
                AccuracyStats.MaxXAccuracy = AccuracyStats.AbsMaxXAccuracy * checkpointminus;
            }
        }
    }
}