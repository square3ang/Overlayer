using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class OverlayerAPI
    {
        [Tag(NotPlaying = false, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double PredictedGGDifficulty = -999;
        public static void Reset()
        {
            //PredictedGGDifficulty = -999;
        }
    }
}
