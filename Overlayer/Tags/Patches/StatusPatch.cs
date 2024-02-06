using Overlayer.Core.Patches;
using System;
using System.Collections.Generic;
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
                if (__result == HitMargin.Perfect) Status.Combo++;
                else Status.Combo = 0;
                var l = HitPatch.JudgementTagPatch.GetHitMargin(Difficulty.Lenient, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                var n = HitPatch.JudgementTagPatch.GetHitMargin(Difficulty.Normal, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                var s = HitPatch.JudgementTagPatch.GetHitMargin(Difficulty.Strict, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                HitPatch.JudgementTagPatch.FixMargin(controller, ref l);
                HitPatch.JudgementTagPatch.FixMargin(controller, ref n);
                HitPatch.JudgementTagPatch.FixMargin(controller, ref s);
                if (!HitPatch.JudgementTagPatch.IsSafe(controller)) SetScores(l, n, s, __result);
            }
            private static void SetScores(HitMargin l, HitMargin n, HitMargin s, HitMargin cur)
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
        [LazyPatch("Tags.Status.CurrentCheckPointPreparer_scnGame", "scnGame", "Play", Triggers = new string[]
        {
            nameof(Status.CurCheckPoint)
        })]
        [LazyPatch("Tags.Status.CurrentCheckPointPreparer_scnEditor", "scnEditor", "Play", Triggers = new string[]
        {
            nameof(Status.CurCheckPoint)
        })]
        [LazyPatch("Tags.Status.CurrentCheckPointPreparer_scrPressToStart", "scrPressToStart", "ShowText", Triggers = new string[]
        {
            nameof(Status.CurCheckPoint)
        })]
        public static class CurrentCheckPointPreparer
        {
            public static List<scrFloor> AllCheckPoints;
            public static void Postfix()
            {
                AllCheckPoints = scrLevelMaker.instance.listFloors.FindAll(f => f.GetComponent<ffxCheckpoint>() != null);
            }
        }
        [LazyPatch("Tag.Status.CurrentCheckPointGetter", "scrPlanet", "MoveToNextFloor", Triggers = new string[]
        {
            nameof(Status.CurCheckPoint)
        })]
        public static class CurrentCheckPointGetter
        {
            public static void Postfix(scrFloor floor)
            {
                if (CurrentCheckPointPreparer.AllCheckPoints == null) return;
                Status.CurCheckPoint = GetCheckPointIndex(floor);
            }
            public static int GetCheckPointIndex(scrFloor floor)
            {
                if (floor == null) return 0;
                int i = 0;
                foreach (var chkPt in CurrentCheckPointPreparer.AllCheckPoints)
                {
                    if (floor.seqID + 1 <= chkPt.seqID)
                        return i;
                    i++;
                }
                return i;
            }
        }
        [LazyPatch("Tag.Status.BestProgressResetter_Editor", "scnEditor", "OpenLevelCo", Triggers = new string[]
        {
            nameof(Status.BestProgress)
        })]
        [LazyPatch("Tag.Status.BestProgressResetter_CLS", "scnGame", "LoadLevel", Triggers = new string[]
        {
            nameof(Status.BestProgress)
        })]
        public static class BestProgressResetter
        {
            public static void Postfix()
            {
                Status.BestProgress = 0;
            }
        }
        [LazyPatch("Tag.Status.BestProgressUpdater_OnMove", "scrPlanet", "MoveToNextFloor", Triggers = new string[]
        {
            nameof(Status.BestProgress)
        })]
        [LazyPatch("Tag.Status.BestProgressUpdater_OnFail", "scrController", "FailAction", Triggers = new string[]
        {
            nameof(Status.BestProgress)
        })]
        public static class BestProgressUpdater
        {
            public static void Postfix()
            {
                if (scrLevelMaker.instance == null) return;
                Status.BestProgress = Math.Max(Status.BestProgress, scrController.instance.percentComplete * 100);
            }
        }
        [LazyPatch("Tag.Status.BestProgressFixer", "scrController", "OnLandOnPortal", Triggers = new string[]
        {
            nameof(Status.BestProgress)
        })]
        public static class BestProgressFixer
        {
            public static void Postfix(scrController __instance)
            {
                if (__instance.gameworld)
                    Status.BestProgress = 100;
            }
        }
    }
}
