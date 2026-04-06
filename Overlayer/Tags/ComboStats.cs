using System;
using System.Collections.Generic;
using System.Linq;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;

namespace Overlayer.Tags;

public static class ComboStats {
    [Tag] public static int Combo;
    [Tag] public static int MaxCombo;

    #region MarginCombo

    [Tag]
    public static int LMarginCombo(HitMargin margin) {
        return Combos[(int)Difficulty.Lenient][(int)margin];
    }

    [Tag]
    public static int NMarginCombo(HitMargin margin) {
        return Combos[(int)Difficulty.Normal][(int)margin];
    }

    [Tag]
    public static int SMarginCombo(HitMargin margin) {
        return Combos[(int)Difficulty.Strict][(int)margin];
    }

    [Tag]
    public static int MarginCombo(HitMargin margin) {
        return Combos[(int)GCS.difficulty][(int)margin];
    }

    #endregion

    #region MarginCombos

    [Tag]
    public static int LMarginCombos(string margins) {
        return MarginCombos_Internal(Difficulty.Lenient, margins);
    }

    [Tag]
    public static int NMarginCombos(string margins) {
        return MarginCombos_Internal(Difficulty.Normal, margins);
    }

    [Tag]
    public static int SMarginCombos(string margins) {
        return MarginCombos_Internal(Difficulty.Strict, margins);
    }

    [Tag]
    public static int MarginCombos(string margins) {
        return MarginCombos_Internal(GCS.difficulty, margins);
    }

    [Tag]
    public static string SpecialPlayMark(int maxLength = -1, string afterTrimStr = Extensions.DefaultTrimStr) {
        var seqID = scrController.instance.currentSeqID;
        var ppCombo = MarginCombos_Internal(GCS.difficulty, "Perfect");
        var noMiss = MarginCombos_Internal(GCS.difficulty, "VeryEarly|EarlyPerfect|Perfect|LatePerfect|VeryLate");
        var result = "XX";
        if (ppCombo == seqID)
            result = "PP";
        else if (noMiss == seqID)
            result = "FC+";
        else if (Hit.MissCount + Hit.Overloads <= 0) result = "FC";
        return result.Trim(maxLength, afterTrimStr);
    }

    public static int MarginCombos_Internal(Difficulty diff, string margins) {
        var hms = margins.SplitParse<HitMargin>('|');
        var hash = ADOUtils.HashMargins(hms);
        if (!MMaxComboCache.TryGetValue(hash, out _))
            MMaxComboCache[hash] = new int[EnumHelper<Difficulty>.GetValues().Length];
        if (!MComboCache.TryGetValue(hash, out var combos))
            combos = MComboCache[hash] = new int[EnumHelper<Difficulty>.GetValues().Length];
        return combos[(int)diff];
    }

    #endregion

    #region MarginMaxCombo

    [Tag]
    public static int LMarginMaxCombo(HitMargin margin) {
        return MaxCombos[(int)Difficulty.Lenient][(int)margin];
    }

    [Tag]
    public static int NMarginMaxCombo(HitMargin margin) {
        return MaxCombos[(int)Difficulty.Normal][(int)margin];
    }

    [Tag]
    public static int SMarginMaxCombo(HitMargin margin) {
        return MaxCombos[(int)Difficulty.Strict][(int)margin];
    }

    [Tag]
    public static int MarginMaxCombo(HitMargin margin) {
        return MaxCombos[(int)GCS.difficulty][(int)margin];
    }

    #endregion

    public static int[][] Combos = new int[EnumHelper<Difficulty>.GetValues().Length][];
    public static int[][] MaxCombos = new int[EnumHelper<Difficulty>.GetValues().Length][];

    public static void Combos_Set(Difficulty diff, HitMargin hit) {
        var iHit = (int)hit;
        var combos = Combos[(int)diff];
        var maxCombos = MaxCombos[(int)diff];
        combos[iHit]++;
        for (var i = 0; i < combos.Length; i++)
            if (i != iHit)
                combos[i] = 0;

        for (var i = 0; i < maxCombos.Length; i++) maxCombos[i] = Math.Max(maxCombos[i], combos[i]);
    }

    #region MarginMaxCombos

    [Tag]
    public static int LMarginMaxCombos(string margins) {
        return MarginMaxCombos_Internal(Difficulty.Lenient, margins);
    }

    [Tag]
    public static int NMarginMaxCombos(string margins) {
        return MarginMaxCombos_Internal(Difficulty.Normal, margins);
    }

    [Tag]
    public static int SMarginMaxCombos(string margins) {
        return MarginMaxCombos_Internal(Difficulty.Strict, margins);
    }

    [Tag]
    public static int MarginMaxCombos(string margins) {
        return MarginMaxCombos_Internal(GCS.difficulty, margins);
    }

    public static int MarginMaxCombos_Internal(Difficulty diff, string margins) {
        var hms = margins.SplitParse<HitMargin>('|');
        var hash = ADOUtils.HashMargins(hms);
        if (!MComboCache.TryGetValue(hash, out _))
            MComboCache[hash] = new int[EnumHelper<Difficulty>.GetValues().Length];
        if (!MMaxComboCache.TryGetValue(hash, out var combos))
            combos = MMaxComboCache[hash] = new int[EnumHelper<Difficulty>.GetValues().Length];
        return combos[(int)diff];
    }

    #endregion

    public static Dictionary<int, int[]> MComboCache = [];
    public static Dictionary<int, int[]> MMaxComboCache = [];

    public static void SetMarginCombos() {
        foreach (var hash in MComboCache.Keys.ToList()) {
            var hms = ADOUtils.UnboxMarginHash(hash);
            var combos = MComboCache[hash];
            var maxCombos = MMaxComboCache[hash];
            foreach (var diff in EnumHelper<Difficulty>.GetValues()) {
                var difference = Hit.GetCHit(diff);
                if (Array.IndexOf(hms, difference) >= 0)
                    maxCombos[(int)diff] = Math.Max(maxCombos[(int)diff], ++combos[(int)diff]);
                else
                    combos[(int)diff] = 0;
            }
        }
    }

    public static void Reset() {
        Combo = MaxCombo = 0;

        MComboCache.Clear();
        MMaxComboCache.Clear();

        var margins = EnumHelper<HitMargin>.GetValues().Length;
        for (var i = 0; i < Combos.Length; i++) Combos[i] = new int[margins];
        for (var i = 0; i < MaxCombos.Length; i++) MaxCombos[i] = new int[margins];
    }
}