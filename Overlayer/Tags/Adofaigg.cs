using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;

namespace Overlayer.Tags
{
    public static class Adofaigg
    {
        [Tag]
        public static bool GGRequestCompleted = false;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double GGDifficulty = -999;
        [Tag]
        public static double GGPlayPoint(int digits = -1)
        {
            if (!GGRequestCompleted) return 0.0;
            double result;
            var edit = scnEditor.instance;
            if (edit)
                result = (double)CalculatePlayPoint(GGDifficulty, edit.levelData.pitch, Status.XAccuracy(), scrLevelMaker.instance.listFloors.Count, digits);
            else result = (double)CalculatePlayPoint(GGDifficulty, (int)Math.Round(Status.Pitch() * 100), Status.XAccuracy(), scrLevelMaker.instance.listFloors.Count, digits);
            return result;
        }
        [Tag(ProcessingFlags = ValueProcessing.AccessMember)]
        public static GGRatingHolder GGRating = new GGRatingHolder();
        [Tag]
        public static double CalculatePlayPoint(double difficulty = 0, int speed = 100, double accuracy = 100, int tile = 0, int digits = -1)
        {
            if (difficulty < 1) return 0.0;
            double difficultyRating = 1600.0 / (1.0 + Math.Exp(-0.3 * difficulty + 5.5));
            double xAccuracy = accuracy / 100.0;
            double accuracyRating = 0.03 / (-Math.Pow(Math.Min(1, xAccuracy), Math.Pow(tile, 0.05)) + 1.05) + 0.4;
            double pitch = speed / 100.0;
            double pitchRating = pitch >= 1 ? Math.Pow((2 + pitch) / 3.0, Math.Pow(0.1 + Math.Pow(tile, 0.5) / Math.Pow(2000, 0.5), 1.1)) : Math.Pow(pitch, 1.8);
            double tilesRating = tile < 2000 ? 0.9 + tile / 10000.0 : Math.Pow(tile / 2000.0, 0.05);
            return Math.Pow(difficultyRating * accuracyRating * pitchRating * tilesRating, 1.01).Round(digits);
        }
        public static void Reset()
        {
            GGRating = new GGRatingHolder();
        }
        [IgnoreCase]
        public class GGRatingHolder
        {
            public Rank rank = Rank.X;
            public double rating = 0;
            public Rank rank_Internal = Rank.SSS100;
        }
        public enum Rank : ushort
        {
            X,
            F = 799,
            D = 800,
            C = 900,
            B = 930,
            A = 960,
            S = 980,
            SS = 990,
            SSS = 995,
            SSS100 = 1000,
        }
    }
}
