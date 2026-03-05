using Overlayer.Core.Patches;
namespace Overlayer.Tags.Patches;

public class P_scrCamera : PatchBase<P_scrCamera> {
    [LazyPatch("Tags.P_scrCamera.FrameRate__Update", "scrCamera", "Update", Triggers = new string[] {
        nameof(FrameRate.Fps), nameof(FrameRate.FrameTime),
    })]
    public static class FrameRate__Update {
        public static void Postfix() {
            var deltaTime = UnityEngine.Time.deltaTime;
            FrameRate.LastDeltaTime += (deltaTime - FrameRate.LastDeltaTime) * 0.1f;
            if(FrameRate.FpsTimer > Main.Settings.FPSUpdateRate / 1000.0f) {
                FrameRate.Fps = 1.0f / FrameRate.LastDeltaTime;
                FrameRate.FpsTimer = 0;
            }
            FrameRate.FpsTimer += deltaTime;
            if(FrameRate.FpsTimeTimer > Main.Settings.FrameTimeUpdateRate / 1000.0f) {
                FrameRate.FrameTime = FrameRate.LastDeltaTime * 1000.0f;
                FrameRate.FpsTimeTimer = 0;
            }
            FrameRate.FpsTimeTimer += deltaTime;
        }
    }
}
