using Overlayer.Tags.Attributes;

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
        [Tag()]
        public static string LHit() => RDString.Get("HitMargin." + Lenient);
        [Tag()]
        public static string NHit() => RDString.Get("HitMargin." + Normal);
        [Tag()]
        public static string SHit() => RDString.Get("HitMargin." + Strict);
        [Tag()]
        public static string CHit() => RDString.Get("HitMargin." + Current);
        [Tag()]
        public static double LTE, LVE, LEP, LP, LLP, LVL, LTL;
        [Tag()]
        public static double NTE, NVE, NEP, NP, NLP, NVL, NTL;
        [Tag()]
        public static double STE, SVE, SEP, SP, SLP, SVL, STL;
        [Tag()]
        public static double CTE, CVE, CEP, CP, CLP, CVL, CTL;
        [Tag()]
        public static double LFast() => LTE + LVE + LEP;
        [Tag()]
        public static double NFast() => NTE + NVE + NEP;
        [Tag()]
        public static double SFast() => STE + SVE + SEP;
        [Tag()]
        public static double CFast() => CTE + CVE + CEP;
        [Tag()]
        public static double LSlow() => LTL + LVL + LLP;
        [Tag()]
        public static double NSlow() => NTL + NVL + NLP;
        [Tag()]
        public static double SSlow() => STL + SVL + SLP;
        [Tag()]
        public static double CSlow() => CTL + CVL + CLP;
        [Tag()]
        public static double MissCount() => scrController.instance?.mistakesManager?.GetHits(HitMargin.FailMiss) ?? 0;
        [Tag()]
        public static double Overloads() => scrController.instance?.mistakesManager?.GetHits(HitMargin.FailOverload) ?? 0;
        [Tag()]
        public static double Multipress;
        [Tag()]
        public static string Difficulty() => RDString.Get("enum.Difficulty." + GCS.difficulty);
        [Tag()]
        public static string DifficultyStr() => GCS.difficulty.ToString();
        public static void Reset()
        {
            Lenient = Normal = Strict = Current = HitMargin.Perfect;
            LTE = LVE = LEP = LP = LLP = LVL = LTL = 0;
            NTE = NVE = NEP = NP = NLP = NVL = NTL = 0;
            STE = SVE = SEP = SP = SLP = SVL = STL = 0;
            CTE = CVE = CEP = CP = CLP = CVL = CTL = 0;
            Multipress = 0;
        }
        public static HitMargin GetCHit(Difficulty diff)
        {
            switch (diff)
            {
                case global::Difficulty.Lenient: return Lenient;
                case global::Difficulty.Normal: return Normal;
                case global::Difficulty.Strict: return Strict;
                default: return Strict;
            }
        }
    }
}
