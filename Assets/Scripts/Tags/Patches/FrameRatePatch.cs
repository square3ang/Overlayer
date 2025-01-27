using HarmonyLib;

namespace Overlayer.Tags.Patches
{
    public class FrameRatePatch {
    [HarmonyPatch(typeof(scrCamera), "Update")]
        public static class FrameRateGetter
        {
            static float lastDeltaTime;
            static float fpsTimer;
            static float fpsTimeTimer;
            public static void Postfix()
            {
                var deltaTime = UnityEngine.Time.deltaTime;
                lastDeltaTime += (deltaTime - lastDeltaTime) * 0.1f;
                if (fpsTimer > 0.5f)
                {
                    FrameRate.Fps = 1.0f / lastDeltaTime;
                    fpsTimer = 0;
                }
                fpsTimer += deltaTime;
                if (fpsTimeTimer > 0.5f)
                {
                    FrameRate.FrameTime = lastDeltaTime * 1000.0f;
                    fpsTimeTimer = 0;
                }
                fpsTimeTimer += deltaTime;
            }
        }
    }
}
