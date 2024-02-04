using ADOFAI;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;

namespace Overlayer.Tags
{
    public static class Status
    {
        public static LevelData LevelData => scnGame.instance?.levelData ?? scnEditor.instance?.levelData;
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double Progress(int digits = -1) => (scrController.instance?.percentComplete * 100 ?? 0).Round(digits);
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double Accuracy(int digits = -1) => (scrController.instance?.mistakesManager?.percentAcc * 100 ?? 0).Round(digits);
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double XAccuracy(int digits = -1) => (scrController.instance?.mistakesManager?.percentXAcc * 100 ?? 0).Round(digits);
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double Pitch(int digits = -1) => GCS.currentSpeedTrial.Round(digits);
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double EditorPitch(int digits = -1) => (LevelData.pitch / 100.0).Round(digits);
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int CheckPointUsed() => scrController.checkpointsUsed;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int TotalCheckPoints;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int Combo;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int LScore;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int NScore;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int SScore;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int Score;
        public static void Reset()
        {
            TotalCheckPoints = Combo = LScore = NScore = SScore = Score = 0;
        }
    }
}
