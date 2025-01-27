using System;
using Overlayer.Utils;
using UnityEngine;

namespace Overlayer.Tags
{
    public static class Status
    {
        public static bool IsAutoEnabled
        {
            [Tag] get => ADOFAI.RDC?.auto ?? false;
        }

        public static bool IsPracticeModeEnabled
        {
            [Tag] get => ADOFAI.RDC?.practice ?? false;
        }

        public static bool IsOldAutoEnabled
        {
            [Tag] get => ADOFAI.RDC?.useOldAuto ?? false;
        }

        public static bool IsNoFailEnabled
        {
            [Tag] get => ADOFAI.Controller?.noFail ?? GCS.useNoFail;
        }

        [Tag(Dummy=100d)]
        public static double Progress() =>
            scrController.instance?.percentComplete * 100 ?? 0;

        [Tag(Dummy=100d)]
        public static double ActualProgress()
        {
            var listFloors = scrLevelMaker.instance?.listFloors;
            if (listFloors == null || listFloors.Count == 0)
                return 0;
            var firstFloorTime = listFloors[1].entryTime;
            var lastFloorTime = listFloors[listFloors.Count - 1].entryTime;
            var actualProgress = (scrController.instance?.currFloor.entryTime - firstFloorTime) /
                (lastFloorTime - firstFloorTime) * 100;
            if (actualProgress == null) return 0;
            return Mathf.Clamp((float)actualProgress, 0, 100);
        }

        [Tag(Dummy=100d)]
        public static double Accuracy() =>
            (scrController.instance?.mistakesManager?.percentAcc * 100 ?? 0);

        [Tag(Dummy = 100d)]
        public static double XAccuracy()
        {
            var a =scrController.instance?.mistakesManager?.percentXAcc * 100 ?? 0;
            if (double.IsNaN(a)) a = 0;
            return a;
        }

        [Tag(Dummy=100d)]
        public static double Pitch() => GCS.currentSpeedTrial;

        [Tag(Dummy=100d)]
        public static double EditorPitch() => (ADOFAI.LevelData?.pitch ?? 0) / 100.0;

        [Tag(Dummy=1972)]
        public static int CheckPointUsed() => scrController.checkpointsUsed;

        [Tag(Dummy=1)] public static int CurCheckPoint;
        [Tag(Dummy=1972)] public static int TotalCheckPoints;
        [Tag(Dummy=1234)] public static int Combo;
        [Tag(Dummy=1234)] public static int MaxCombo;
        [Tag(Dummy=10000)] public static int LScore;
        [Tag(Dummy=10000)] public static int NScore;
        [Tag(Dummy=10000)] public static int SScore;
        [Tag(Dummy=10000)] public static int Score;
        [Tag(Dummy=19.72)] public static double BestProgress;

        #region MarginCombo

        [Tag(Dummy=1234)]
        public static int LMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Lenient][(int)margin];

        [Tag(Dummy=1234)]
        public static int NMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Normal][(int)margin];

        [Tag(Dummy=1234)]
        public static int SMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Strict][(int)margin];

        [Tag(Dummy=1234)]
        public static int MarginCombo(HitMargin margin) => Combos[(int)GCS.difficulty][(int)margin];

        #endregion

        #region MarginMaxCombo

        [Tag(Dummy=1972)]
        public static int LMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Lenient][(int)margin];

        [Tag(Dummy=1972)]
        public static int NMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Normal][(int)margin];

        [Tag(Dummy=1972)]
        public static int SMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Strict][(int)margin];

        [Tag(Dummy=1972)]
        public static int MarginMaxCombo(HitMargin margin) => MaxCombos[(int)GCS.difficulty][(int)margin];

        #endregion

        public static int[][] Combos = new int[EnumHelper<Difficulty>.GetValues().Length][];
        public static int[][] MaxCombos = new int[EnumHelper<Difficulty>.GetValues().Length][];

        public static void Reset()
        {
            CurCheckPoint = TotalCheckPoints = Combo = MaxCombo = LScore = NScore = SScore = Score = 0;
            //BestProgress = 0;
            int margins = EnumHelper<HitMargin>.GetValues().Length;
            for (int i = 0; i < Combos.Length; i++)
                Combos[i] = new int[margins];
            for (int i = 0; i < MaxCombos.Length; i++)
                MaxCombos[i] = new int[margins];
        }
    }
}