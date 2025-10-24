using Overlayer.Core.Patches;
using System;

namespace Overlayer.Tags.Patches {
    public class P_scrMisc : PatchBase<P_scrMisc> {
        [LazyPatch("Tags.P_scrMisc.Hit__GetHitMargin", "scrMisc", "GetHitMargin", Triggers = new string[] {
            nameof(Hit.LHit), nameof(Hit.LTE), nameof(Hit.LVE), nameof(Hit.LEP), nameof(Hit.LP), nameof(Hit.LLP), nameof(Hit.LVL), nameof(Hit.LTL),
            nameof(Hit.NHit), nameof(Hit.NTE), nameof(Hit.NVE), nameof(Hit.NEP), nameof(Hit.NP), nameof(Hit.NLP), nameof(Hit.NVL), nameof(Hit.NTL),
            nameof(Hit.SHit), nameof(Hit.STE), nameof(Hit.SVE), nameof(Hit.SEP), nameof(Hit.SP), nameof(Hit.SLP), nameof(Hit.SVL), nameof(Hit.STL),
            nameof(Hit.CHit), nameof(Hit.CTE), nameof(Hit.CVE), nameof(Hit.CEP), nameof(Hit.CP), nameof(Hit.CLP), nameof(Hit.CVL), nameof(Hit.CTL),
            nameof(Hit.LT),   nameof(Hit.LV),  nameof(Hit.LELP),
            nameof(Hit.NT),   nameof(Hit.NV),  nameof(Hit.NELP),
            nameof(Hit.ST),   nameof(Hit.SV),  nameof(Hit.SELP),
            nameof(Hit.CT),   nameof(Hit.CV),  nameof(Hit.CELP),
            "LHitRaw", "NHitRaw", "SHitRaw", "CHitRaw",
            nameof(Hit.LFast), nameof(Hit.NFast), nameof(Hit.SFast), nameof(Hit.CFast),
            nameof(Hit.LSlow), nameof(Hit.NSlow), nameof(Hit.SSlow), nameof(Hit.CSlow),
            nameof(Hit.LMarginCombos), nameof(Hit.NMarginCombos), nameof(Hit.SMarginCombos), nameof(Hit.MarginCombos),
            nameof(Hit.LMarginMaxCombos), nameof(Hit.NMarginMaxCombos), nameof(Hit.SMarginMaxCombos), nameof(Hit.MarginMaxCombos),
            nameof(Hit.SpecialPlayMark),
        })]
        public static class Hit__GetHitMargin {
            public static bool Prefix(float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch, double marginScale, ref HitMargin __result) {
                var controller = scrController.instance;
                if(controller && controller.currFloor.freeroam) {
                    return true;
                }
                Hit.Lenient = Hit.GetHitMargin(Difficulty.Lenient, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                Hit.Normal = Hit.GetHitMargin(Difficulty.Normal, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                Hit.Strict = Hit.GetHitMargin(Difficulty.Strict, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                Hit.FixMargin(controller, ref Hit.Lenient);
                Hit.FixMargin(controller, ref Hit.Normal);
                Hit.FixMargin(controller, ref Hit.Strict);
                Hit.Current = __result = Hit.GetCHit(GCS.difficulty);
                if(!Hit.ControllerIsSafe(controller)) {
                    Hit.IncreaseCount(Difficulty.Lenient, Hit.Lenient);
                    Hit.IncreaseCount(Difficulty.Normal, Hit.Normal);
                    Hit.IncreaseCount(Difficulty.Strict, Hit.Strict);
                    Hit.IncreaseCCount(Hit.Current);
                    Hit.SetMarginCombos();
                }
                return false;
            }
        }

        [LazyPatch("Tags.P_scrMisc.Status__GetHitMargin", "scrMisc", "GetHitMargin", Triggers = new string[] {
            nameof(Status.Combo), nameof(Status.MaxCombo), nameof(Status.LScore), nameof(Status.NScore), nameof(Status.SScore), nameof(Status.Score),
            nameof(Status.LMarginCombo), nameof(Status.NMarginCombo), nameof(Status.SMarginCombo), nameof(Status.MarginCombo),
            nameof(Status.LMarginMaxCombo), nameof(Status.NMarginMaxCombo), nameof(Status.SMarginMaxCombo), nameof(Status.MarginMaxCombo),
        })]
        public static class Status__GetHitMargin {
            public static void Postfix(float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch, double marginScale, ref HitMargin __result) {
                var controller = scrController.instance;
                if(controller && controller.currFloor.freeroam)
                    return;
                if(!Hit.ControllerIsSafe(controller)) {
                    if(__result == HitMargin.Perfect)
                        Status.MaxCombo = Math.Max(Status.MaxCombo, ++Status.Combo);
                    else
                        Status.Combo = 0;
                    var l = Hit.GetHitMargin(Difficulty.Lenient, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                    var n = Hit.GetHitMargin(Difficulty.Normal, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                    var s = Hit.GetHitMargin(Difficulty.Strict, hitangle, refangle, isCW, bpmTimesSpeed, conductorPitch, marginScale);
                    Hit.FixMargin(controller, ref l);
                    Hit.FixMargin(controller, ref n);
                    Hit.FixMargin(controller, ref s);
                    Hit.SetScores(l, n, s, __result);
                    Hit.SetCombos(Difficulty.Lenient, l);
                    Hit.SetCombos(Difficulty.Normal, n);
                    Hit.SetCombos(Difficulty.Strict, s);
                }
            }
        }
    }
}
