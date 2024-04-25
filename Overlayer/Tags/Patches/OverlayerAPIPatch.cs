using Overlayer.Core;
using Overlayer.Core.Patches;
using System.IO;
using System.Threading.Tasks;
using static Newgrounds.Medal;

namespace Overlayer.Tags.Patches
{
    public class OverlayerAPIPatch : PatchBase<OverlayerAPIPatch>
    {
        [LazyPatch("Tags.OverlayerAPI.PredictedDifficultyUpdater", "ADOFAI.LevelData", "LoadLevel", Triggers = new string[]
        {
            nameof(OverlayerAPI.PredictedGGDifficulty)
        })]
        public static class PredictedDifficultyUpdater
        {
            public static void Postfix(string levelPath)
            {
                if (!File.Exists(levelPath))
                {
                    OverlayerAPI.PredictedGGDifficulty = -404;
                    return;
                }
                new Task(async () =>
                {
                    try
                    {
                        OverlayerAPI.PredictedGGDifficulty = await OverlayerWebAPI.GetPredDifficulty(File.ReadAllBytes(levelPath));
                        if (Adofaigg.GGDifficulty == -404) Adofaigg.GGDifficulty = OverlayerAPI.PredictedGGDifficulty;
                    }
                    catch
                    {
                        OverlayerAPI.PredictedGGDifficulty = -500;
                    }
                }).Start();
            }
        }
    }
}
