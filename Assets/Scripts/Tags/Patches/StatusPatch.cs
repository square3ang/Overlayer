using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Overlayer.Tags.Patches
{
    public class StatusPatch
    {
        [HarmonyPatch(typeof(scnGame), "Play")]
        public static class TotalCheckPointsPatch
        {
            public static void Postfix()
            {
                Status.TotalCheckPoints = scrLevelMaker.instance.listFloors.Count(f => f.GetComponent<ffxCheckpoint>() != null);
            }
        }
        [HarmonyPatch(typeof(scrPressToStart), "ShowText")]
        public static class TotalCheckPointsPatch2
        {
            public static void Postfix()
            {
                Status.TotalCheckPoints = scrLevelMaker.instance.listFloors.Count(f => f.GetComponent<ffxCheckpoint>() != null);
            }
        }
        [HarmonyPatch(typeof(scrMisc), "GetHitMargin")]
        public static class ComboAndScoresPatch
        {
            public static void Postfix(float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch, double marginScale, ref HitMargin __result)
            {
                var controller = scrController.instance;
                if (controller && controller.currFloor.freeroam) return;
                if (!HitPatch.JudgementTagPatch.IsSafe(controller))
                {
                    if (__result == HitMargin.Perfect)
                        Status.MaxCombo = Math.Max(Status.MaxCombo, ++Status.Combo);
                    else Status.Combo = 0;
                    var l = HitPatch.JudgementTagPatch.GetHitMargin(Difficulty.Lenient, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                    var n = HitPatch.JudgementTagPatch.GetHitMargin(Difficulty.Normal, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                    var s = HitPatch.JudgementTagPatch.GetHitMargin(Difficulty.Strict, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                    HitPatch.JudgementTagPatch.FixMargin(controller, ref l);
                    HitPatch.JudgementTagPatch.FixMargin(controller, ref n);
                    HitPatch.JudgementTagPatch.FixMargin(controller, ref s);
                    SetScores(l, n, s, __result);
                    SetCombos(Difficulty.Lenient, l);
                    SetCombos(Difficulty.Normal, n);
                    SetCombos(Difficulty.Strict, s);
                }
            }
            private static void SetScores(HitMargin l, HitMargin n, HitMargin s, HitMargin c)
            {
                switch (c)
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
            private static void SetCombos(Difficulty diff, HitMargin hit)
            {
                int iHit = (int)hit;
                int[] combos = Status.Combos[(int)diff];
                int[] maxCombos = Status.MaxCombos[(int)diff];
                combos[iHit]++;
                for (int i = 0; i < combos.Length; i++)
                    if (i != iHit) combos[i] = 0;
                for (int i = 0; i < maxCombos.Length; i++)
                    maxCombos[i] = Math.Max(maxCombos[i], combos[i]);
            }
        }
        [HarmonyPatch(typeof(scnGame), "Play")]
        public static class CurrentCheckPointPreparer
        {
            public static List<scrFloor> AllCheckPoints;
            public static void Postfix()
            {
                AllCheckPoints = scrLevelMaker.instance.listFloors.FindAll(f => f.GetComponent<ffxCheckpoint>() != null);
            }
        }
        [HarmonyPatch(typeof(scnEditor), "Play")]
        public static class CurrentCheckPointPreparer2
        {
            public static List<scrFloor> AllCheckPoints;
            public static void Postfix()
            {
                AllCheckPoints = scrLevelMaker.instance.listFloors.FindAll(f => f.GetComponent<ffxCheckpoint>() != null);
            }
        }
        [HarmonyPatch(typeof(scrPressToStart), "ShowText")]
        public static class CurrentCheckPointPreparer3
        {
            public static List<scrFloor> AllCheckPoints;
            public static void Postfix()
            {
                AllCheckPoints = scrLevelMaker.instance.listFloors.FindAll(f => f.GetComponent<ffxCheckpoint>() != null);
            }
        }
        [HarmonyPatch(typeof(scrPlanet), "MoveToNextFloor")]
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
        [HarmonyPatch(typeof(scnEditor), "OpenLevelCo")]
        public static class BestProgressResetter
        {
            public static void Postfix()
            {
                Status.BestProgress = 0;
            }
        }
        [HarmonyPatch(typeof(scnGame), "LoadLevel")]
        public static class BestProgressResetter2
        {
            public static void Postfix()
            {
                Status.BestProgress = 0;
            }
        }
        [HarmonyPatch(typeof(scrPlanet), "MoveToNextFloor")]
        public static class BestProgressUpdater
        {
            public static void Postfix()
            {
                if (scrLevelMaker.instance == null) return;
                Status.BestProgress = Math.Max(Status.BestProgress, scrController.instance.percentComplete * 100);
            }
        }
        [HarmonyPatch(typeof(scrController), "FailAction")]
        public static class BestProgressUpdater2
        {
            public static void Postfix()
            {
                if (scrLevelMaker.instance == null) return;
                Status.BestProgress = Math.Max(Status.BestProgress, scrController.instance.percentComplete * 100);
            }
        }
        [HarmonyPatch(typeof(scrController), "OnLandOnPortal")]
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
