using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overlayer.Tags;

public static class ComboStats {
    [Tag]
    [TagDesc("Combo count")]
    public static int Combo;
    [Tag]
    [TagDesc("Maximum Combo")]
    public static int MaxCombo;

    #region MarginCombo
    [Tag]
    [TagDesc("Lenient judgment of MarginCombo")]
    public static int LMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Lenient][(int)margin];
    [Tag]
    [TagDesc("Normal judgment of MarginCombo")]
    public static int NMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Normal][(int)margin];
    [Tag]
    [TagDesc("Strict judgment of MarginCombo")]
    public static int SMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Strict][(int)margin];
    [Tag]
    [TagDesc("Displays the combo for a single judgment\n(ex: {MarginCombo:VeryLate})")]
    public static int MarginCombo(HitMargin margin) => Combos[(int)GCS.difficulty][(int)margin];
    #endregion

    #region MarginCombos
    [Tag]
    [TagDesc("Lenient judgment of MarginCombos")]
    public static int LMarginCombos(string margins) => MarginCombos_Internal(Difficulty.Lenient, margins);
    [Tag]
    [TagDesc("Normal judgment of MarginMaxCombo")]
    public static int NMarginCombos(string margins) => MarginCombos_Internal(Difficulty.Normal, margins);
    [Tag]
    [TagDesc("Strict judgment of MarginCombos")]
    public static int SMarginCombos(string margins) => MarginCombos_Internal(Difficulty.Strict, margins);
    [Tag]
    [TagDesc("Displays all combos for multiple judgments\n(ex: {MarginCombos:VeryLate|VeryEarly})")]
    public static int MarginCombos(string margins) => MarginCombos_Internal(GCS.difficulty, margins);
    [Tag]
    [TagDesc("Displays a special mark based on your play performance:\nPP = perfect  FC+ = no miss\nFC = full combo  XX = no special mark")]
    public static string SpecialPlayMark(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) {
        var seqID = scrController.instance.currentSeqID;
        var ppCombo = MarginCombos_Internal(GCS.difficulty, "Perfect");
        var noMiss = MarginCombos_Internal(GCS.difficulty, "VeryEarly|EarlyPerfect|Perfect|LatePerfect|VeryLate");
        string result = "XX";
        if(ppCombo == seqID) {
            result = "PP";
        } else if(noMiss == seqID) {
            result = "FC+";
        } else if(Hit.MissCount + Hit.Overloads <= 0) {
            result = "FC";
        }
        return result.Trim(maxLength, afterTrimStr);
    }

    public static int MarginCombos_Internal(Difficulty diff, string margins) {
        var hms = margins.SplitParse<HitMargin>('|');
        int hash = ADOUtils.HashMargins(hms);
        if(!MMaxComboCache.TryGetValue(hash, out _)) {
            MMaxComboCache[hash] = new int[EnumHelper<Difficulty>.GetValues().Length];
        }
        if(!MComboCache.TryGetValue(hash, out int[] combos)) {
            combos = MComboCache[hash] = new int[EnumHelper<Difficulty>.GetValues().Length];
        }
        return combos[(int)diff];
    }
    #endregion

    #region MarginMaxCombo
    [Tag]
    [TagDesc("Lenient judgment of MarginMaxCombo")]
    public static int LMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Lenient][(int)margin];
    [Tag]
    [TagDesc("Normal judgment of MarginMaxCombos")]
    public static int NMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Normal][(int)margin];
    [Tag]
    [TagDesc("Strict judgment of MarginMaxCombo")]
    public static int SMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Strict][(int)margin];
    [Tag]
    [TagDesc("Displays the maximum combo for a single judgment\n(ex: {MarginMaxCombo:VeryLate})")]
    public static int MarginMaxCombo(HitMargin margin) => MaxCombos[(int)GCS.difficulty][(int)margin];
    #endregion

    public static int[][] Combos = new int[EnumHelper<Difficulty>.GetValues().Length][];
    public static int[][] MaxCombos = new int[EnumHelper<Difficulty>.GetValues().Length][];

    public static void Combos_Set(Difficulty diff, HitMargin hit) {
        int iHit = (int)hit;
        int[] combos = Combos[(int)diff];
        int[] maxCombos = MaxCombos[(int)diff];
        combos[iHit]++;
        for(int i = 0; i < combos.Length; i++) {
            if(i != iHit) {
                combos[i] = 0;
            }
        }
        for(int i = 0; i < maxCombos.Length; i++) {
            maxCombos[i] = Math.Max(maxCombos[i], combos[i]);
        }
    }

    #region MarginMaxCombos
    [Tag]
    [TagDesc("Lenient judgment of MarginMaxCombos")]
    public static int LMarginMaxCombos(string margins) => MarginMaxCombos_Internal(Difficulty.Lenient, margins);
    [Tag]
    [TagDesc("Normal judgment of MarginMaxCombos")]
    public static int NMarginMaxCombos(string margins) => MarginMaxCombos_Internal(Difficulty.Normal, margins);
    [Tag]
    [TagDesc("Strict judgment of MarginMaxCombos")]
    public static int SMarginMaxCombos(string margins) => MarginMaxCombos_Internal(Difficulty.Strict, margins);
    [Tag]
    [TagDesc("Displays all maximum combos for multiple judgments\n(ex: {MarginMaxCombos:VeryLate|VeryEarly})")]
    public static int MarginMaxCombos(string margins) => MarginMaxCombos_Internal(GCS.difficulty, margins);

    public static int MarginMaxCombos_Internal(Difficulty diff, string margins) {
        var hms = margins.SplitParse<HitMargin>('|');
        int hash = ADOUtils.HashMargins(hms);
        if(!MComboCache.TryGetValue(hash, out _)) {
            MComboCache[hash] = new int[EnumHelper<Difficulty>.GetValues().Length];
        }
        if(!MMaxComboCache.TryGetValue(hash, out int[] combos)) {
            combos = MMaxComboCache[hash] = new int[EnumHelper<Difficulty>.GetValues().Length];
        }
        return combos[(int)diff];
    }

    #endregion
    public static Dictionary<int, int[]> MComboCache = [];
    public static Dictionary<int, int[]> MMaxComboCache = [];
    public static void SetMarginCombos() {
        foreach(int hash in MComboCache.Keys.ToList()) {
            var hms = ADOUtils.UnboxMarginHash(hash);
            var combos = MComboCache[hash];
            var maxCombos = MMaxComboCache[hash];
            foreach(var diff in EnumHelper<Difficulty>.GetValues()) {
                var difference = Hit.GetCHit(diff);
                if(Array.IndexOf(hms, difference) >= 0) {
                    maxCombos[(int)diff] = Math.Max(maxCombos[(int)diff], ++combos[(int)diff]);
                } else {
                    combos[(int)diff] = 0;
                }
            }
        }
    }

    public static void Reset() {
        Combo = MaxCombo = 0;

        MComboCache.Clear();
        MMaxComboCache.Clear();

        int margins = EnumHelper<HitMargin>.GetValues().Length;
        for(int i = 0; i < Combos.Length; i++) {
            Combos[i] = new int[margins];
        }
        for(int i = 0; i < MaxCombos.Length; i++) {
            MaxCombos[i] = new int[margins];
        }
    }
}
