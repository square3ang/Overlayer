namespace Overlayer.Tags
{
    public static class OverlayerAPI
    {
        [Tag(NotPlaying = false)]
        public static double PredictedGGDifficulty = -999;
        public static void Reset()
        {
            //PredictedGGDifficulty = -999;
        }
    }
}
