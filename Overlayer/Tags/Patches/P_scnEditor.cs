using Overlayer.Core.Patches;

namespace Overlayer.Tags.Patches;

public class P_scnEditor : PatchBase<P_scnEditor> {
    [LazyPatch("Tags.P_scnEditor.ProgressStats__OpenLevelCo", "scnEditor", "OpenLevelCo", Triggers = new string[] {
        nameof(ProgressStats.BestProgress)
    })]
    public static class ProgressStats__OpenLevelCo {
        public static void Postfix() => ProgressStats.BestProgress_Reset();
    }

    [LazyPatch("Tags.P_scnEditor.CheckPoint__Play", "scnEditor", "Play", Triggers = new string[] {
        nameof(CheckPointStats.CurCheckPoint),
    })]
    public static class CheckPoint__Play {
        public static void Postfix() => CheckPointStats.TotalCheckPoients_Update();
    }

    [LazyPatch("Tags.P_scnEditor.Tile__ResetScene", "scnEditor", "ResetScene", Triggers = new string[] {
        nameof(Tile.StartTile), nameof(Tile.StartProgress)
    })]
    public static class Tile__ResetScene {
        public static void Postfix(scrController __instance) => Tile.Started_Reset(__instance);
    }
}
