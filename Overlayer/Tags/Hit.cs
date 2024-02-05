using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class Hit
    {
        [Tag("LHitRaw", Hint = ReturnTypeHint.Enum)]
        public static HitMargin Lenient;
        [Tag("NHitRaw", Hint = ReturnTypeHint.Enum)]
        public static HitMargin Normal;
        [Tag("SHitRaw", Hint = ReturnTypeHint.Enum)]
        public static HitMargin Strict;
        [Tag("CHitRaw", Hint = ReturnTypeHint.Enum)]
        public static HitMargin Current;
        [Tag(Hint = ReturnTypeHint.String)]
        public static string LHit() => RDString.Get("HitMargin." + Lenient);
        [Tag(Hint = ReturnTypeHint.String)]
        public static string NHit() => RDString.Get("HitMargin." + Normal);
        [Tag(Hint = ReturnTypeHint.String)]
        public static string SHit() => RDString.Get("HitMargin." + Strict);
        [Tag(Hint = ReturnTypeHint.String)]
        public static string CHit() => RDString.Get("HitMargin." + Current);
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double LTE, LVE, LEP, LP, LLP, LVL, LTL;
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double NTE, NVE, NEP, NP, NLP, NVL, NTL;
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double STE, SVE, SEP, SP, SLP, SVL, STL;
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double CTE, CVE, CEP, CP, CLP, CVL, CTL;
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double MissCount() => scrController.instance?.mistakesManager?.GetHits(HitMargin.FailMiss) ?? 0;
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double Overloads() => scrController.instance?.mistakesManager?.GetHits(HitMargin.FailOverload) ?? 0;
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double Multipress;
        [Tag(Hint = ReturnTypeHint.String)]
        public static string Difficulty() => RDString.Get("enum.Difficulty." + GCS.difficulty);
        [Tag(Hint = ReturnTypeHint.String)]
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
