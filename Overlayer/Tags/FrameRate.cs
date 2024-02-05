using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class FrameRate
    {
        [Tag(NotPlaying = true, Flags = AdvancedFlags.Round, Hint = ReturnTypeHint.Double)]
        public static double Fps;
        [Tag(NotPlaying = true, Flags = AdvancedFlags.Round, Hint = ReturnTypeHint.Double)]
        public static double FrameTime;
        public static void Reset()
        {
            Fps = FrameTime = 0;
        }
    }
}
