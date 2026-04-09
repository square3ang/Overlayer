using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using UnityEngine;

namespace Overlayer.Tags;

public static class Hit {
    [Tag("LHitRaw")]
    [TagDesc("Lenient Hit Raw")]
    public static HitMargin Lenient;
    [Tag("NHitRaw")]
    [TagDesc("Normal Hit Raw")]
    public static HitMargin Normal;
    [Tag("SHitRaw")]
    [TagDesc("Strict Hit Raw")]
    public static HitMargin Strict;
    [Tag("CHitRaw")]
    [TagDesc("Current Hit Raw")]
    public static HitMargin Current;
    [Tag]
    [TagDesc("Lenient Hit")]
    public static string LHit(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => RDString.Get("HitMargin." + Lenient).Trim(maxLength, afterTrimStr);
    [Tag]
    [TagDesc("Normal Hit")]
    public static string NHit(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => RDString.Get("HitMargin." + Normal).Trim(maxLength, afterTrimStr);
    [Tag]
    [TagDesc("Strict Hit")]
    public static string SHit(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => RDString.Get("HitMargin." + Strict).Trim(maxLength, afterTrimStr);
    [Tag]
    [TagDesc("Current Hit")]
    public static string CHit(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => RDString.Get("HitMargin." + Current).Trim(maxLength, afterTrimStr);
    [Tag]
    [TagDesc("Lenient Too Late")]
    public static int LTE, LVE, LEP, LP, LLP, LVL, LTL;
    [Tag]
    [TagDesc("Normal Too Late")]
    public static int NTE, NVE, NEP, NP, NLP, NVL, NTL;
    [Tag]
    [TagDesc("Strict Too Late")]
    public static int STE, SVE, SEP, SP, SLP, SVL, STL;
    [Tag]
    [TagDesc("Current Too Late")]
    public static int CTE, CVE, CEP, CP, CLP, CVL, CTL;
    [Tag]
    [TagDesc("Fast judgment in Lenient difficulty")]
    public static int LFast => LTE + LVE + LEP;
    [Tag]
    [TagDesc("Fast judgment in Normal difficulty")]
    public static int NFast => NTE + NVE + NEP;
    [Tag]
    [TagDesc("Fast judgment in Strict difficulty")]
    public static int SFast => STE + SVE + SEP;
    [Tag]
    [TagDesc("Fast judgment in Current difficulty")]
    public static int CFast => CTE + CVE + CEP;
    [Tag]
    [TagDesc("Slow judgment in Lenient difficulty")]
    public static int LSlow => LTL + LVL + LLP;
    [Tag]
    [TagDesc("Slow judgment in Normal difficulty")]
    public static int NSlow => NTL + NVL + NLP;
    [Tag]
    [TagDesc("Slow judgment in Strict difficulty")]
    public static int SSlow => STL + SVL + SLP;
    [Tag]
    [TagDesc("Slow judgment in Current difficulty")]
    public static int CSlow => CTL + CVL + CLP;
    [Tag]
    [TagDesc("Lenient Early Perfect + Lenient Late Perfect")]
    public static int LELP => LEP + LLP;
    [Tag]
    [TagDesc("Normal Early Perfect + Normal Late Perfect")]
    public static int NELP => NEP + NLP;
    [Tag]
    [TagDesc("Strict Early Perfect + Strict Late Perfect")]
    public static int SELP => SEP + SLP;
    [Tag]
    [TagDesc("Current Early Perfect + Late Perfect")]
    public static int CELP => CEP + CLP;
    [Tag]
    public static int LV => LVE + LVL;
    [Tag]
    [TagDesc("Normal Very Early + Normal Very Late")]
    public static int NV => NVE + NVL;
    [Tag]
    [TagDesc("Strict Very Early + Strict Very Late")]
    public static int SV => SVE + SVL;
    [Tag]
    [TagDesc("Current Very Early + Current Very Late")]
    public static int CV => CVE + CVL;
    [Tag]
    public static int LT => LTE + LTL;
    [Tag]
    [TagDesc("Normal Too Early + Normal Too Late")]
    public static int NT => NTE + NTL;
    [Tag]
    [TagDesc("Strict Too Early + Strict Too Late")]
    public static int ST => STE + STL;
    [Tag]
    [TagDesc("Current Too Early + Current Too Late")]
    public static int CT => CTE + CTL;
    [Tag]
    [TagDesc("Official Too Early")]
    public static int OTE => scrMistakesManager.hitMarginsCount[0];
    [Tag]
    [TagDesc("Official Very Early")]
    public static int OVE => scrMistakesManager.hitMarginsCount[1];
    [Tag]
    [TagDesc("Official Early Perfect")]
    public static int OEP => scrMistakesManager.hitMarginsCount[2];
    [Tag]
    [TagDesc("Official Perfect")]
    public static int OP => scrMistakesManager.hitMarginsCount[3] + scrMistakesManager.hitMarginsCount[10];
    [Tag]
    [TagDesc("Official Late Perfect")]
    public static int OLP => scrMistakesManager.hitMarginsCount[4];
    [Tag]
    [TagDesc("Normal Very Late")]
    public static int OVL => scrMistakesManager.hitMarginsCount[5];
    [Tag]
    [TagDesc("Official Too Late")]
    public static int OTL => scrMistakesManager.hitMarginsCount[6];
    [Tag]
    [TagDesc("Official Perfect (Only Auto)")]
    public static int OA => scrMistakesManager.hitMarginsCount[10];
    [Tag]
    [TagDesc("Official Perfect (Only Player)")]
    public static int OPP => scrMistakesManager.hitMarginsCount[3];
    [Tag]
    [TagDesc("Fast judgment in Official")]
    public static int OFast => OTE + OVE + OEP;
    [Tag]
    [TagDesc("Slow judgment in Official")]
    public static int OSlow => OTL + OVL + OLP;
    [Tag]
    [TagDesc("Official Early Perfect + Official Late Perfect")]
    public static int OELP => OEP + OLP;
    [Tag]
    [TagDesc("Official Very Early + Official Very Late")]
    public static int OV => OVE + OVL;
    [Tag]
    [TagDesc("Official Too Early + Official Too Late")]
    public static int OT => OTE + OTL;
    [Tag]
    [TagDesc("Number of Misses")]
    public static int MissCount => scrController.instance?.mistakesManager?.GetHits(HitMargin.FailMiss) ?? 0;
    [Tag]
    [TagDesc("Number of Overloads")]
    public static int Overloads => scrController.instance?.mistakesManager?.GetHits(HitMargin.FailOverload) ?? 0;
    [Tag]
    [TagDesc("MissCount + Overloads")]
    public static int Fail => MissCount + Overloads;
    [Tag]
    [TagDesc("Number of Multipresses")]
    public static int Multipress;
    [Tag]
    [TagDesc("Current Difficulty")]
    public static string Difficulty(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => RDString.Get("enum.Difficulty." + GCS.difficulty).Trim(maxLength, afterTrimStr);
    [Tag]
    [TagDesc("Fixed difficulty return value regardless of language setting")]
    public static string DifficultyRaw(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => GCS.difficulty.ToString().Trim(maxLength, afterTrimStr);

    public static bool ControllerIsSafe(scrController ctrl) => ctrl.currFloor?.isSafe ?? false;

    public static void FixMargin(scrController ctrl, ref HitMargin hitMargin) {
        if(ctrl.gameworld) {
            if(ctrl.noFailInfiniteMargin) {
                hitMargin = HitMargin.FailMiss;
            }
            if(ctrl.midspinInfiniteMargin || (RDC.auto && !RDC.useOldAuto)) {
                hitMargin = HitMargin.Perfect;
            }
        }
    }

    public static void IncreaseCount(Difficulty diff, HitMargin hit) {
        switch(hit) {
            case HitMargin.TooEarly:
                switch(diff) {
                    case global::Difficulty.Lenient:
                        LTE++;
                        break;
                    case global::Difficulty.Normal:
                        NTE++;
                        break;
                    case global::Difficulty.Strict:
                        STE++;
                        break;
                }
                break;
            case HitMargin.VeryEarly:
                switch(diff) {
                    case global::Difficulty.Lenient:
                        LVE++;
                        break;
                    case global::Difficulty.Normal:
                        NVE++;
                        break;
                    case global::Difficulty.Strict:
                        SVE++;
                        break;
                }
                break;
            case HitMargin.EarlyPerfect:
                switch(diff) {
                    case global::Difficulty.Lenient:
                        LEP++;
                        break;
                    case global::Difficulty.Normal:
                        NEP++;
                        break;
                    case global::Difficulty.Strict:
                        SEP++;
                        break;
                }
                break;
            case HitMargin.Perfect:
                switch(diff) {
                    case global::Difficulty.Lenient:
                        LP++;
                        break;
                    case global::Difficulty.Normal:
                        NP++;
                        break;
                    case global::Difficulty.Strict:
                        SP++;
                        break;
                }
                break;
            case HitMargin.LatePerfect:
                switch(diff) {
                    case global::Difficulty.Lenient:
                        LLP++;
                        break;
                    case global::Difficulty.Normal:
                        NLP++;
                        break;
                    case global::Difficulty.Strict:
                        SLP++;
                        break;
                }
                break;
            case HitMargin.VeryLate:
                switch(diff) {
                    case global::Difficulty.Lenient:
                        LVL++;
                        break;
                    case global::Difficulty.Normal:
                        NVL++;
                        break;
                    case global::Difficulty.Strict:
                        SVL++;
                        break;
                }
                break;
            case HitMargin.TooLate:
                switch(diff) {
                    case global::Difficulty.Lenient:
                        LTL++;
                        break;
                    case global::Difficulty.Normal:
                        NTL++;
                        break;
                    case global::Difficulty.Strict:
                        STL++;
                        break;
                }
                break;
        }
    }

    public static void IncreaseCCount(HitMargin hit) {
        switch(hit) {
            case HitMargin.TooEarly:
                CTE++;
                break;
            case HitMargin.VeryEarly:
                CVE++;
                break;
            case HitMargin.EarlyPerfect:
                CEP++;
                break;
            case HitMargin.Perfect:
                CP++;
                break;
            case HitMargin.LatePerfect:
                CLP++;
                break;
            case HitMargin.VeryLate:
                CVL++;
                break;
            case HitMargin.TooLate:
                CTL++;
                break;
        }
    }

    public static double GetAdjustedAngleBoundaryInDeg(Difficulty diff, HitMarginGeneral marginType, double bpmTimesSpeed, double conductorPitch, double marginMult = 1.0) {
        float num = 0.065f;
        switch(diff) {
            case global::Difficulty.Lenient:
                num = 0.091f;
                break;
            case global::Difficulty.Normal:
                num = 0.065f;
                break;
            case global::Difficulty.Strict:
                num = 0.04f;
                break;
        }
        bool isMobile = ADOBase.isMobile;
        num = isMobile ? 0.09f : (num / GCS.currentSpeedTrial);
        float num2 = isMobile ? 0.07f : (0.03f / GCS.currentSpeedTrial);
        float a = isMobile ? 0.05f : (0.02f / GCS.currentSpeedTrial);
        num = Mathf.Max(num, 0.025f);
        num2 = Mathf.Max(num2, 0.025f);
        double num3 = (double)Mathf.Max(a, 0.025f);
        double val = scrMisc.TimeToAngleInRad((double)num, bpmTimesSpeed, conductorPitch, false) * 57.295780181884766;
        double val2 = scrMisc.TimeToAngleInRad((double)num2, bpmTimesSpeed, conductorPitch, false) * 57.295780181884766;
        double val3 = scrMisc.TimeToAngleInRad(num3, bpmTimesSpeed, conductorPitch, false) * 57.295780181884766;
        double result = Math.Max(GCS.HITMARGIN_COUNTED * marginMult, val);
        double result2 = Math.Max(45.0 * marginMult, val2);
        double result3 = Math.Max(30.0 * marginMult, val3);
        return marginType switch {
            HitMarginGeneral.Counted => result,
            HitMarginGeneral.Perfect => result2,
            HitMarginGeneral.Pure => result3,
            _ => result,
        };
    }

    public static HitMargin GetHitMargin(Difficulty diff, float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch, double marginScale) {
        float angleDeg = 57.29578f * (hitangle - refangle) * (isCW ? 1 : -1);

        double countedDeg = GetAdjustedAngleBoundaryInDeg(diff, HitMarginGeneral.Counted, bpmTimesSpeed, conductorPitch, marginScale);
        double perfectDeg = GetAdjustedAngleBoundaryInDeg(diff, HitMarginGeneral.Perfect, bpmTimesSpeed, conductorPitch, marginScale);
        double pureDeg = GetAdjustedAngleBoundaryInDeg(diff, HitMarginGeneral.Pure, bpmTimesSpeed, conductorPitch, marginScale);

        return angleDeg < -countedDeg
            ? HitMargin.TooEarly
            : angleDeg < -perfectDeg
            ? HitMargin.VeryEarly
            : angleDeg < -pureDeg
            ? HitMargin.EarlyPerfect
            : angleDeg <= pureDeg
            ? HitMargin.Perfect
            : angleDeg <= perfectDeg ? HitMargin.LatePerfect : angleDeg <= countedDeg ? HitMargin.VeryLate : HitMargin.TooLate;
    }

    public static void Reset() {
        Lenient = Normal = Strict = Current = HitMargin.Perfect;
        LTE = LVE = LEP = LP = LLP = LVL = LTL = 0;
        NTE = NVE = NEP = NP = NLP = NVL = NTL = 0;
        STE = SVE = SEP = SP = SLP = SVL = STL = 0;
        CTE = CVE = CEP = CP = CLP = CVL = CTL = 0;
        Multipress = 0;
    }

    public static HitMargin GetCHit(Difficulty diff) {
        return diff switch {
            global::Difficulty.Lenient => Lenient,
            global::Difficulty.Normal => Normal,
            global::Difficulty.Strict => Strict,
            _ => Strict,
        };
    }

    public static int GetHitCount(Difficulty diff, HitMargin margin) {
        return diff switch {
            global::Difficulty.Lenient => margin switch {
                HitMargin.TooEarly => LTE,
                HitMargin.VeryEarly => LVE,
                HitMargin.EarlyPerfect => LEP,
                HitMargin.Perfect => LP,
                HitMargin.LatePerfect => LLP,
                HitMargin.VeryLate => LVL,
                HitMargin.TooLate => LTL,
                _ => 0,
            },
            global::Difficulty.Normal => margin switch {
                HitMargin.TooEarly => NTE,
                HitMargin.VeryEarly => NVE,
                HitMargin.EarlyPerfect => NEP,
                HitMargin.Perfect => NP,
                HitMargin.LatePerfect => NLP,
                HitMargin.VeryLate => NVL,
                HitMargin.TooLate => NTL,
                _ => 0,
            },
            global::Difficulty.Strict => margin switch {
                HitMargin.TooEarly => STE,
                HitMargin.VeryEarly => SVE,
                HitMargin.EarlyPerfect => SEP,
                HitMargin.Perfect => SP,
                HitMargin.LatePerfect => SLP,
                HitMargin.VeryLate => SVL,
                HitMargin.TooLate => STL,
                _ => 0,
            },
            _ => 0,
        };
    }
}
