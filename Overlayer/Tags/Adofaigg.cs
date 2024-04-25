using Overlayer.Core;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using System.Threading.Tasks;

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
                result = (double)CalculatePlayPoint(GGDifficulty, edit.levelData.pitch, Status.XAccuracy(), scrLevelMaker.instance.listFloors.Count);
            else result = (double)CalculatePlayPoint(GGDifficulty, (int)Math.Round(Status.Pitch() * 100), Status.XAccuracy(), scrLevelMaker.instance.listFloors.Count);
            return result.Round(digits);
        }
        [Tag(ProcessingFlags = ValueProcessing.AccessMember)]
        public static GGRatingHolder GGRating = new GGRatingHolder();
        public static double CalculatePlayPoint(double difficulty, int speed, double accuracy, int tile)
        {
            if (difficulty < 1) return 0.0;
            double difficultyRating = 1600.0 / (1.0 + Math.Exp(-0.3 * difficulty + 5.5));
            double xAccuracy = accuracy / 100.0;
            double accuracyRating = 0.03 / (-Math.Pow(Math.Min(1, xAccuracy), Math.Pow(tile, 0.05)) + 1.05) + 0.4;
            double pitch = speed / 100.0;
            double pitchRating = pitch >= 1 ? Math.Pow((2 + pitch) / 3.0, Math.Pow(0.1 + Math.Pow(tile, 0.5) / Math.Pow(2000, 0.5), 1.1)) : Math.Pow(pitch, 1.8);
            double tilesRating = tile < 2000 ? 0.9 + tile / 10000.0 : Math.Pow(tile / 2000.0, 0.05);
            return Math.Pow(difficultyRating * accuracyRating * pitchRating * tilesRating, 1.01);
        }
        public static void Reset()
        {
            GGRequestCompleted = false;
            GGRating = new GGRatingHolder();
            var levelData = ADOFAI.LevelData;
            if (levelData == null) return;
            new Task(async () =>
            {
                string artist = levelData.artist.BreakRichTag(), author = levelData.author.BreakRichTag(), title = levelData.song.BreakRichTag();
                GGDifficulty = await OverlayerWebAPI.GetGGDifficulty(artist, title, author, string.IsNullOrWhiteSpace(levelData.pathData) ? levelData.angleData.Count : levelData.pathData.Length, (int)Math.Round(levelData.bpm));
                GGRequestCompleted = true;
            }).Start();
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
