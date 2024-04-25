using Overlayer.Core;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using System.Threading.Tasks;

namespace Overlayer.Tags
{
    public static class TUF
    {
        [Tag]
        public static bool TUFRequestCompleted = false;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double TUFDifficulty = -999;
        public static void Reset()
        {
            TUFRequestCompleted = false;
            var levelData = ADOFAI.LevelData;
            if (levelData == null) return;
            new Task(async () =>
            {
                string artist = levelData.artist.BreakRichTag(), author = levelData.author.BreakRichTag(), title = levelData.song.BreakRichTag();
                TUFDifficulty = await OverlayerWebAPI.GetTUFDifficulty(artist, title, author, string.IsNullOrWhiteSpace(levelData.pathData) ? levelData.angleData.Count : levelData.pathData.Length, (int)Math.Round(levelData.bpm));
                //if (TUFDifficulty == -404 && TagManager.HasReference(typeof(OverlayerAPI))) TUFDifficulty = OverlayerAPI.PredictedDifficulty;
                TUFRequestCompleted = true;
            }).Start();
        }
    }
}
