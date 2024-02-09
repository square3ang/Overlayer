using ADOFAI;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;

namespace Overlayer.Tags
{
    public static class Status
    {
        public static LevelData LevelData => scnGame.instance?.levelData ?? scnEditor.instance?.levelData;
        [Tag]
        public static double Progress(int digits = -1) => (scrController.instance?.percentComplete * 100 ?? 0).Round(digits);
        [Tag]
        public static double Accuracy(int digits = -1) => (scrController.instance?.mistakesManager?.percentAcc * 100 ?? 0).Round(digits);
        [Tag]
        public static double XAccuracy(int digits = -1) => (scrController.instance?.mistakesManager?.percentXAcc * 100 ?? 0).Round(digits);
        [Tag]
        public static double Pitch(int digits = -1) => GCS.currentSpeedTrial.Round(digits);
        [Tag]
        public static double EditorPitch(int digits = -1) => (LevelData.pitch / 100.0).Round(digits);
        [Tag]
        public static int CheckPointUsed() => scrController.checkpointsUsed;
        [Tag]
        public static int CurCheckPoint;
        [Tag]
        public static int TotalCheckPoints;
        [Tag]
        public static int Combo;
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
        public static void Reset()
        {
            CurCheckPoint = TotalCheckPoints = Combo = LScore = NScore = SScore = Score = 0;
            //BestProgress = 0;
        }
    }
}
