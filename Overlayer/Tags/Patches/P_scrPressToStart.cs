using Overlayer.Core.Patches;

namespace Overlayer.Tags.Patches;

public class P_scrPressToStart : PatchBase<P_scrPressToStart> {
    [LazyPatch("Tags.P_scrPressToStart.Bpm__ShowText", "scrPressToStart", "ShowText", Triggers = new string[] {
        nameof(Bpm.TileBpm), nameof(Bpm.CurBpm), nameof(Bpm.RecKPS),
        nameof(Bpm.TileBpmWithoutPitch), nameof(Bpm.CurBpmWithoutPitch), nameof(Bpm.RecKPSWithoutPitch),
    })]
    public static class Bpm__ShowText {
        public static void Postfix(scrController __instance) => Bpm.Init(__instance);
    }

    [LazyPatch("Tags.P_scrPressToStart.Level__ShowText", "scrPressToStart", "ShowText", Triggers = new string[] {
        nameof(Level.Title), nameof(Level.Author), nameof(Level.Artist),
        nameof(Level.TitleRaw), nameof(Level.AuthorRaw), nameof(Level.ArtistRaw),
        nameof(Level.DefaultTextColor), nameof(Level.DefaultTextShadowColor),
        nameof(Level.LevelNameTextColor), nameof(Level.LevelNameTextShadowColor)
    })]
    public static class Level__ShowText {
        public static void Postfix() => Level.Init();
    }

    [LazyPatch("Tags.P_scrPressToStart.Status__ShowText", "scrPressToStart", "ShowText", Triggers = new string[] {
        nameof(CheckPointStats.TotalCheckPoints), nameof(CheckPointStats.CurCheckPoint),
    })]
    public static class Status__ShowText {
        public static void Postfix() => CheckPointStats.InterCheckPoints_Update();
    }
}
