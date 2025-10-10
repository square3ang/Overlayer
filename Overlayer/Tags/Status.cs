using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using UnityEngine;

namespace Overlayer.Tags
{
    public static class Status
    {
        [Tag]
        public static bool IsAutoEnabled => ADOFAI.RDC?.auto ?? false;
        [Tag]
        public static bool IsPracticeModeEnabled => ADOFAI.RDC?.practice ?? false;
        [Tag]
        public static bool IsOldAutoEnabled => ADOFAI.RDC?.useOldAuto ?? false;
        [Tag]
        public static bool IsNoFailEnabled => ADOFAI.Controller?.noFail ?? GCS.useNoFail;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double Progress() => scrController.instance?.percentComplete * 100 ?? 0;
        //[Tag]
        //public static int Deaths() => scrController.deaths;
        //[Tag]
        //public static int Attempts() => 0;

        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double ActualProgress()
        {
            var listFloors = scrLevelMaker.instance?.listFloors;
            if (listFloors == null || listFloors.
                    Count == 0)
                return 0;
            var firstFloorTime = listFloors[1].entryTime;
            var lastFloorTime = listFloors[listFloors.Count - 1].entryTime;
            var actualProgress = (scrController.instance?.currFloor.entryTime - firstFloorTime) / (lastFloorTime - firstFloorTime) * 100;
            if (actualProgress == null) return 0;
            return Mathf.Clamp((float)actualProgress, 0, 100);
        }
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double Accuracy() => (scrController.instance?.mistakesManager?.percentAcc * 100 ?? 0);
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double XAccuracy() => (scrController.instance?.mistakesManager?.percentXAcc * 100 ?? 0);
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double Pitch() => GCS.currentSpeedTrial;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double EditorPitch() => ((ADOFAI.LevelData?.pitch ?? 0) / 100.0);
        [Tag]
        public static int CheckPointUsed() => scrController.checkpointsUsed;
        [Tag]
        public static int CurCheckPoint;
        [Tag]
        public static int TotalCheckPoints;
        [Tag]
        public static int Combo;
        [Tag]
        public static int MaxCombo;
        [Tag]
        public static int LScore;
        [Tag]
        public static int NScore;
        [Tag]
        public static int SScore;
        [Tag]
        public static int Score;
        [Tag]
        public static double BestProgress;
        #region MarginCombo
        [Tag]
        public static int LMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Lenient][(int)margin];
        [Tag]
        public static int NMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Normal][(int)margin];
        [Tag]
        public static int SMarginCombo(HitMargin margin) => Combos[(int)Difficulty.Strict][(int)margin];
        [Tag]
        public static int MarginCombo(HitMargin margin) => Combos[(int)GCS.difficulty][(int)margin];
        #endregion
        #region MarginMaxCombo
        [Tag]
        public static int LMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Lenient][(int)margin];
        [Tag]
        public static int NMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Normal][(int)margin];
        [Tag]
        public static int SMarginMaxCombo(HitMargin margin) => MaxCombos[(int)Difficulty.Strict][(int)margin];
        [Tag]
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
