using Overlayer.Tags.Attributes;
using UnityEngine;

namespace Overlayer.Tags;

public static class FrameRate {
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Current FPS of ADOFAI")]
    public static double Fps;
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Time difference between the previous frame and the current frame in ms")]
    public static double FrameTime;

    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Target FPS of ADOFAI")]
    public static double TargetFps => Application.targetFrameRate;

    public static float LastDeltaTime;
    public static float FpsTimer;
    public static float FpsTimeTimer;

    public static void Reset() => Fps = FrameTime = 0;
}
