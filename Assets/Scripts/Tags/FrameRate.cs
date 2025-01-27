using UnityEngine;

namespace Overlayer.Tags
{
    public static class FrameRate
    {
        [Tag(NotPlaying = true)]
        public static double Fps;
        [Tag(NotPlaying = true)]
        public static double FrameTime;


        public static double TargetFps
        {
            [Tag(NotPlaying = true)]
            get => Application.targetFrameRate;
        }
        
        
        public static void Reset()
        {
            Fps = FrameTime = 0;
        }
    }
}
