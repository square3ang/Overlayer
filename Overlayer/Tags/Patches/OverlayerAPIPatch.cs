using Overlayer.Core;
using Overlayer.Core.Patches;
using System.IO;
using System.Threading.Tasks;

namespace Overlayer.Tags.Patches
{
    public class OverlayerAPIPatch : PatchBase<OverlayerAPIPatch>
    {
        [LazyPatch("Tags.OverlayerAPI.PredictedDifficultyUpdater", "ADOFAI.LevelData", "LoadLevel", Triggers = new string[]
        {
            nameof(OverlayerAPI.PredictedDifficulty)
        })]
        public static class PredictedDifficultyUpdater
        {
            public static void Postfix(string levelPath)
            {
                if (!File.Exists(levelPath))
                {
                    OverlayerAPI.PredictedDifficulty = -404;
                    return;
                }
                new Task(async () =>
                {
                    try
                    {
                        OverlayerAPI.PredictedDifficulty = await OverlayerWebAPI.GetPredDifficulty(File.ReadAllBytes(levelPath));
                    }
                    catch
                    {
                        OverlayerAPI.PredictedDifficulty = -500;
                    }
                }).Start();
            }
        }
    }
}
