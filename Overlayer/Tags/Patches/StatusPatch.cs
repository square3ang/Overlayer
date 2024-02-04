using Overlayer.Core.Patches;
using System.Linq;

namespace Overlayer.Tags.Patches
{
    public class StatusPatch : PatchBase<StatusPatch>
    {
        [LazyPatch("Tags.Status.TotalCheckPointsPatch_scnGame", "scnGame", "Play", Triggers = new string[] { nameof(Status.TotalCheckPoints) })]
        [LazyPatch("Tags.Status.TotalCheckPointsPatch_scrPressToStart", "scrPressToStart", "ShowText", Triggers = new string[] { nameof(Status.TotalCheckPoints) })]
        public static class TotalCheckPointsPatch
        {
            public static void Postfix()
            {
                Status.TotalCheckPoints = scrLevelMaker.instance.listFloors.Count(f => f.GetComponent<ffxCheckpoint>() != null);
            }
        }
        [LazyPatch("Tags.Status.Combo&ScoreCalculator", "scrMisc", "GetHitMargin", Triggers = new string[]
        {
            nameof(Status.Combo), nameof(Status.LScore), nameof(Status.NScore), nameof(Status.SScore), nameof(Status.Score)
        })]
        public static class ComboAndScoresPatch
        {
            public static void Postfix(float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch, double marginScale, ref HitMargin __result)
            {
                var controller = scrController.instance;
                if (controller && controller.currFloor.freeroam) return;
                if (__result != HitMargin.Perfect) Status.Combo++;
                else Status.Combo = 0;
                var l = HitPatch.JudgementTagPatch.GetHitMargin(Difficulty.Lenient, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                var n = HitPatch.JudgementTagPatch.GetHitMargin(Difficulty.Normal, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                var s = HitPatch.JudgementTagPatch.GetHitMargin(Difficulty.Strict, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                SetScores(l, n, s, __result);
            }
        }
        public static void SetScores(HitMargin l, HitMargin n, HitMargin s, HitMargin cur)
        {
            switch (cur)
            {
                case HitMargin.VeryEarly:
                case HitMargin.VeryLate:
                    Status.Score += 91;
                    break;
                case HitMargin.EarlyPerfect:
                case HitMargin.LatePerfect:
                    Status.Score += 150;
                    break;
                case HitMargin.Perfect:
                    Status.Score += 300;
                    break;
            }
            switch (l)
            {
                case HitMargin.VeryEarly:
                case HitMargin.VeryLate:
                    Status.LScore += 91;
                    break;
                case HitMargin.EarlyPerfect:
                case HitMargin.LatePerfect:
                    Status.LScore += 150;
                    break;
                case HitMargin.Perfect:
                    Status.LScore += 300;
                    break;
            }
            switch (n)
            {
                case HitMargin.VeryEarly:
                case HitMargin.VeryLate:
                    Status.NScore += 91;
                    break;
                case HitMargin.EarlyPerfect:
                case HitMargin.LatePerfect:
                    Status.NScore += 150;
                    break;
                case HitMargin.Perfect:
                    Status.NScore += 300;
                    break;
            }
            switch (s)
            {
                case HitMargin.VeryEarly:
                case HitMargin.VeryLate:
                    Status.SScore += 91;
                    break;
                case HitMargin.EarlyPerfect:
                case HitMargin.LatePerfect:
                    Status.SScore += 150;
                    break;
                case HitMargin.Perfect:
                    Status.SScore += 300;
                    break;
            }
        }
    }
}
