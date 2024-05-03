using ADOFAI;
using Overlayer.Core;
using Overlayer.Core.Patches;
using Overlayer.Utils;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Overlayer.Tags.Patches
{
    public class TUFPatch : PatchBase<TUFPatch>
    {
        [LazyPatch("Tags.TUF.TUFDifficultiesUpdater", "ADOFAI.LevelData", "LoadLevel", Triggers = new string[]
        {
            nameof(TUF.TUFDifficulty), nameof(TUF.TUFDifficulties),
        })]
        public static class TUFDifficultiesUpdater
        {
            public static void Postfix(LevelData __instance, bool __result)
            {
                if (!__result)
                {
                    TUF.TUFDifficulties.pguDiffNum = -999;
                    TUF.TUFRequestCompleted = true;
                    return;
                }
                new Task(async () =>
                {
                    string artist = __instance.artist.BreakRichTag(), author = __instance.author.BreakRichTag(), title = __instance.song.BreakRichTag();
                    TUF.TUFDifficulties = await OverlayerWebAPI.GetTUFDifficulties(artist, title, author, string.IsNullOrWhiteSpace(__instance.pathData) ? __instance.angleData.Count : __instance.pathData.Length, (int)Math.Round(__instance.bpm));
                    if (TUF.TUFDifficulties.status == HttpStatusCode.NotFound)
                        TUF.TUFDifficulties.pguDiffNum = -404;
                    else if (TUF.TUFDifficulties.status == HttpStatusCode.InternalServerError)
                        TUF.TUFDifficulties.pguDiffNum = -500;
                    //if (TUFDifficulty == -404 && TagManager.HasReference(typeof(OverlayerAPI))) TUFDifficulty = OverlayerAPI.PredictedDifficulty;
                    TUF.TUFRequestCompleted = true;
                }).Start();
            }
        }
    }
}
