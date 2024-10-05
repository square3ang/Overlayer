using Overlayer.Tags.Attributes;
using UnityEngine;

namespace Overlayer.Tags
{
    public static class FrameRate
    {
        [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double Fps;
        [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double FrameTime;

        [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double TargetFps => Application.targetFrameRate;
        
        
        public static void Reset()
        {
            Fps = FrameTime = 0;
        }
    }
}
