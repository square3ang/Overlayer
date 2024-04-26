using Overlayer.Core;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using System.Threading.Tasks;
using System.Net;

namespace Overlayer.Tags
{
    public static class TUF
    {
        [Tag]
        public static bool TUFRequestCompleted = false;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double TUFDifficulty => TUFDifficulties.pguDiffNum;
        [Tag(ProcessingFlags = ValueProcessing.AccessMember)]
        public static OverlayerWebAPI.TUFDifficulties TUFDifficulties = new OverlayerWebAPI.TUFDifficulties();
        public static void Reset()
        {
            TUFRequestCompleted = false;
            var levelData = ADOFAI.LevelData;
            if (levelData == null) return;
            new Task(async () =>
            {
                string artist = levelData.artist.BreakRichTag(), author = levelData.author.BreakRichTag(), title = levelData.song.BreakRichTag();
                TUFDifficulties = await OverlayerWebAPI.GetTUFDifficulties(artist, title, author, string.IsNullOrWhiteSpace(levelData.pathData) ? levelData.angleData.Count : levelData.pathData.Length, (int)Math.Round(levelData.bpm));
                if (TUFDifficulties.status == HttpStatusCode.NotFound)
                    TUFDifficulties.pguDiffNum = -404;
                else if (TUFDifficulties.status == HttpStatusCode.InternalServerError)
                    TUFDifficulties.pguDiffNum = -500;
                //if (TUFDifficulty == -404 && TagManager.HasReference(typeof(OverlayerAPI))) TUFDifficulty = OverlayerAPI.PredictedDifficulty;
                TUFRequestCompleted = true;
            }).Start();
        }
    }
}
