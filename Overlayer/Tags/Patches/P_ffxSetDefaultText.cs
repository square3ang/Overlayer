using Overlayer.Core.Patches;

namespace Overlayer.Tags.Patches;

public class P_ffxSetDefaultText : PatchBase<P_ffxSetDefaultText> {
    [LazyPatch("Tags.P_ffxSetDefaultText.Level_LevelNameText__StartEffect", "ffxSetDefaultText", "StartEffect", Triggers = new string[] {
        nameof(Level.LevelNameText)
    })]
    public static class Level_LevelNameText__StartEffect {
        public static void Postfix() => Level.UpdateLevelNameText();
    }

    [LazyPatch("Tags.P_ffxSetDefaultText.Level_LevelNameTextRaw__StartEffect", "ffxSetDefaultText", "StartEffect", Triggers = new string[] {
        nameof(Level.LevelNameTextRaw)
    })]
    public static class Level_LevelNameTextRaw__StartEffect {
        public static void Postfix() => Level.UpdateLevelNameTextRaw();
    }

    [LazyPatch("Tags.P_ffxSetDefaultText.Level_LevelNameTextColor__Decode", "ffxSetDefaultText", "Decode", Triggers = new string[] {
        nameof(Level.LevelNameTextColor)
    })]
    public static class Level_LevelNameText__Decode {
        public static void Postfix(ffxSetDefaultText __instance) => __instance.defaultTextColorUsed = true;
    }

    [LazyPatch("Tags.P_ffxSetDefaultText.Level_LevelNameTextShadowColor__Decode", "ffxSetDefaultText", "Decode", Triggers = new string[] {
        nameof(Level.LevelNameTextShadowColor)
    })]
    public static class Level_LevelNameTextRaw__Decode {
        public static void Postfix(ffxSetDefaultText __instance) => __instance.defaultTextShadowColorUsed = true;
    }
}
