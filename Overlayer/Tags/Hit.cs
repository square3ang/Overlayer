using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Overlayer.Tags
{
    public static class Hit
    {
        [Tag("LHitRaw")]
        public static HitMargin Lenient;
        [Tag("NHitRaw")]
        public static HitMargin Normal;
        [Tag("SHitRaw")]
        public static HitMargin Strict;
        [Tag("CHitRaw")]
        public static HitMargin Current;
        [Tag]
        public static string LHit(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => RDString.Get("HitMargin." + Lenient).Trim(maxLength, afterTrimStr);
        [Tag]
        public static string NHit(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => RDString.Get("HitMargin." + Normal).Trim(maxLength, afterTrimStr);
        [Tag]
        public static string SHit(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => RDString.Get("HitMargin." + Strict).Trim(maxLength, afterTrimStr);
        [Tag]
        public static string CHit(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => RDString.Get("HitMargin." + Current).Trim(maxLength, afterTrimStr);
        [Tag]
        public static int LTE, LVE, LEP, LP, LLP, LVL, LTL;
        [Tag]
        public static int NTE, NVE, NEP, NP, NLP, NVL, NTL;
        [Tag]
        public static int STE, SVE, SEP, SP, SLP, SVL, STL;
        [Tag]
        public static int CTE, CVE, CEP, CP, CLP, CVL, CTL;
        [Tag]
        public static int LFast() => LTE + LVE + LEP;
        [Tag]
        public static int NFast() => NTE + NVE + NEP;
        [Tag]
        public static int SFast() => STE + SVE + SEP;
        [Tag]
        public static int CFast() => CTE + CVE + CEP;
        [Tag]
        public static int LSlow() => LTL + LVL + LLP;
        [Tag]
        public static int NSlow() => NTL + NVL + NLP;
        [Tag]
        public static int SSlow() => STL + SVL + SLP;
        [Tag]
        public static int CSlow() => CTL + CVL + CLP;
        [Tag]
        public static int LELP() => LEP + LLP;
        [Tag]
        public static int NELP() => NEP + NLP;
        [Tag]
        public static int SELP() => SEP + SLP;
        [Tag]
        public static int CELP() => CEP + CLP;
        [Tag]
        public static int LV() => LVE + LVL;
        [Tag]
        public static int NV() => NVE + NVL;
        [Tag]
        public static int SV() => SVE + SVL;
        [Tag]
        public static int CV() => CVE + CVL;
        [Tag]
        public static int LT() => LTE + LTL;
        [Tag]
        public static int NT() => NTE + NTL;
        [Tag]
        public static int ST() => STE + STL;
        [Tag]
        public static int CT() => CTE + CTL;
        [Tag]
        public static int OTE() => scrMistakesManager.hitMarginsCount[0];
        [Tag]
        public static int OVE() => scrMistakesManager.hitMarginsCount[1];
        [Tag]
        public static int OEP() => scrMistakesManager.hitMarginsCount[2];
        [Tag]
        public static int OP() => scrMistakesManager.hitMarginsCount[3] + scrMistakesManager.hitMarginsCount[10];
        [Tag]
        public static int OLP() => scrMistakesManager.hitMarginsCount[4];
        [Tag]
        public static int OVL() => scrMistakesManager.hitMarginsCount[5];
        [Tag]
        public static int OTL() => scrMistakesManager.hitMarginsCount[6];
        [Tag]
        public static int OA() => scrMistakesManager.hitMarginsCount[10];
        [Tag]
        public static int OPP() => scrMistakesManager.hitMarginsCount[3];
        [Tag]
        public static int OFast() => OTE() + OVE() + OEP();
        [Tag]
        public static int OSlow() => OTL() + OVL() + OLP();
        [Tag]
        public static int OELP() => OEP() + OLP();
        [Tag]
        public static int OV() => OVE() + OVL();
        [Tag]
        public static int OT() => OTE() + OTL();
        [Tag]
        public static int MissCount() => scrController.instance?.mistakesManager?.GetHits(HitMargin.FailMiss) ?? 0;
        [Tag]
        public static int Overloads() => scrController.instance?.mistakesManager?.GetHits(HitMargin.FailOverload) ?? 0;
        [Tag]
        public static int Fail() => MissCount() + Overloads();
        [Tag]
        public static int Multipress;
        [Tag]
        public static string Difficulty(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) => RDString.Get("enum.Difficulty." + GCS.difficulty).Trim(maxLength, afterTrimStr);
        [Tag]
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
                    num = 0.091f; break;
                case global::Difficulty.Normal:
                    num = 0.065f; break;
                case global::Difficulty.Strict:
                    num = 0.04f; break;
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
            switch(marginType) {
                case HitMarginGeneral.Counted: return result;
                case HitMarginGeneral.Perfect: return result2;
                case HitMarginGeneral.Pure: return result3;
            }
            return result;
        }

        public static HitMargin GetHitMargin(Difficulty diff, float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch, double marginScale) {
            float angleDeg = 57.29578f * (hitangle - refangle) * (isCW ? 1 : -1);

            double countedDeg = GetAdjustedAngleBoundaryInDeg(diff, HitMarginGeneral.Counted, bpmTimesSpeed, conductorPitch, marginScale);
            double perfectDeg = GetAdjustedAngleBoundaryInDeg(diff, HitMarginGeneral.Perfect, bpmTimesSpeed, conductorPitch, marginScale);
            double pureDeg = GetAdjustedAngleBoundaryInDeg(diff, HitMarginGeneral.Pure, bpmTimesSpeed, conductorPitch, marginScale);

            if(angleDeg < -countedDeg) {
                return HitMargin.TooEarly;
            }
            if(angleDeg < -perfectDeg) {
                return HitMargin.VeryEarly;
            }
            if(angleDeg < -pureDeg) {
                return HitMargin.EarlyPerfect;
            }
            if(angleDeg <= pureDeg) {
                return HitMargin.Perfect;
            }
            if(angleDeg <= perfectDeg) {
                return HitMargin.LatePerfect;
            }
            if(angleDeg <= countedDeg) {
                return HitMargin.VeryLate;
            }
            return HitMargin.TooLate;
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
            switch (diff) {
                case global::Difficulty.Lenient: return Lenient;
                case global::Difficulty.Normal: return Normal;
                case global::Difficulty.Strict: return Strict;
                default: return Strict;
            }
        }

        public static int GetHitCount(Difficulty diff, HitMargin margin) {
            switch (diff) {
                case global::Difficulty.Lenient:
                    switch (margin) {
                        case HitMargin.TooEarly: return LTE;
                        case HitMargin.VeryEarly: return LVE;
                        case HitMargin.EarlyPerfect: return LEP;
                        case HitMargin.Perfect: return LP;
                        case HitMargin.LatePerfect: return LLP;
                        case HitMargin.VeryLate: return LVL;
                        case HitMargin.TooLate: return LTL;
                        default: return 0;
                    }
                case global::Difficulty.Normal:
                    switch (margin) {
                        case HitMargin.TooEarly: return NTE;
                        case HitMargin.VeryEarly: return NVE;
                        case HitMargin.EarlyPerfect: return NEP;
                        case HitMargin.Perfect: return NP;
                        case HitMargin.LatePerfect: return NLP;
                        case HitMargin.VeryLate: return NVL;
                        case HitMargin.TooLate: return NTL;
                        default: return 0;
                    }
                case global::Difficulty.Strict:
                    switch (margin) {
                        case HitMargin.TooEarly: return STE;
                        case HitMargin.VeryEarly: return SVE;
                        case HitMargin.EarlyPerfect: return SEP;
                        case HitMargin.Perfect: return SP;
                        case HitMargin.LatePerfect: return SLP;
                        case HitMargin.VeryLate: return SVL;
                        case HitMargin.TooLate: return STL;
                        default: return 0;
                    }
                default: return 0;
            }
        }
    }
}
