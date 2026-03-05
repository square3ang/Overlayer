using Overlayer.Core.Patches;
using System;

namespace Overlayer.Tags.Patches {
    public class P_scrMistakeManager : PatchBase<P_scrMistakeManager> {
        [LazyPatch("Tags.P_scrMistakeManager.AccuracyStats__CalculatePercentAcc", "scrMistakesManager", "CalculatePercentAcc", Triggers = new string[] {
            nameof(AccuracyStats.Accuracy), nameof(AccuracyStats.MaxAccuracy),
            nameof(AccuracyStats.XAccuracy), nameof(AccuracyStats.MaxXAccuracy),
            nameof(AccuracyStats.AbsXAccuracy), nameof(AccuracyStats.AbsMaxXAccuracy),
        })]
        public static class AccuracyStats__CalculatePercentAcc {
            public static void Postfix(scrMistakesManager __instance) {
                int perfect = __instance.GetHits(HitMargin.Perfect);
                int auto = __instance.GetHits(HitMargin.Auto);
                int earlyPerfect = __instance.GetHits(HitMargin.EarlyPerfect);
                int latePerfect = __instance.GetHits(HitMargin.LatePerfect);
                int veryEarly = __instance.GetHits(HitMargin.VeryEarly);
                int veryLate = __instance.GetHits(HitMargin.VeryLate);
                int tooEarly = __instance.GetHits(HitMargin.TooEarly);
                int tooLate = __instance.GetHits(HitMargin.TooLate);
                int failMiss = __instance.GetHits(HitMargin.FailMiss);
                int failOverload = __instance.GetHits(HitMargin.FailOverload);

                int success = perfect + earlyPerfect + latePerfect + auto;
                int total = scrMistakesManager.hitMargins.Count + failMiss + failOverload;
                double ratio = (success == total) ? 1.0 : ((double)success / total);
                double bonus = (perfect + auto) * 0.0001;

                AccuracyStats.Accuracy = 100.0 * (ratio + bonus);

                double totalHits = scrMistakesManager.hitMargins.Count;
                double weightedHits =
                    perfect + auto +
                    0.75 * (earlyPerfect + latePerfect) +
                    0.4 * (veryEarly + veryLate) +
                    0.2 * (tooEarly + tooLate);

                double checkpointminus = Math.Pow(0.9875, scrController.checkpointsUsed);
                AccuracyStats.AbsXAccuracy = 100.0 * (weightedHits / totalHits);
                AccuracyStats.XAccuracy = AccuracyStats.AbsXAccuracy * checkpointminus;

                if(ADOBase.lm != null && ADOBase.lm.listFloors != null &&
                    Tile.CurTile >= 0 && Tile.CurTile < ADOBase.lm.listFloors.Count &&
                    ADOBase.lm.listFloors[Tile.CurTile] != null) {

                    int lefttile = Tile.LeftTile - 1 - (ADOBase.lm.listFloors[Tile.CurTile].midSpin ? 1 : 0);

                    int mxsucess = lefttile + perfect + auto + earlyPerfect + latePerfect;
                    int mxtotal = scrMistakesManager.hitMargins.Count + lefttile + failMiss + failOverload;
                    double mxratio = (mxsucess == mxtotal) ? 1.0 : ((double)mxsucess / mxtotal);
                    double mxbonus = (lefttile + perfect + auto) * 0.0001;

                    AccuracyStats.MaxAccuracy = 100.0 * (mxratio + mxbonus);

                    double possibleHitsX =
                        lefttile + perfect + auto + Tile.StartTile - 1 +
                        0.75 * (earlyPerfect + latePerfect) +
                        0.4 * (veryEarly + veryLate) +
                        0.2 * (tooEarly + tooLate);

                    double denomX = Tile.TotalTile - 1 + tooEarly + tooLate;
                    AccuracyStats.AbsMaxXAccuracy = 100.0 * (possibleHitsX / denomX);
                    AccuracyStats.MaxXAccuracy = AccuracyStats.AbsMaxXAccuracy * checkpointminus;
                }
            }
        }
    }
}
