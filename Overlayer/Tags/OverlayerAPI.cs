using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class OverlayerAPI
    {
        [Tag(NotPlaying = false, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double PredictedDifficulty = -999;
        public static void Reset()
        {
            //PredictedDifficulty = -999;
        }
    }
}
