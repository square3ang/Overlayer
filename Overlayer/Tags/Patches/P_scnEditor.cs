using Overlayer.Core.Patches;

namespace Overlayer.Tags.Patches {
    public class P_scnEditor : PatchBase<P_scnEditor> {
        [LazyPatch("Tags.P_scnEditor.Status__OpenLevelCo", "scnEditor", "OpenLevelCo", Triggers = new string[] {
            nameof(Status.BestProgress)
        })]
        public static class Status__OpenLevelCo {
            public static void Postfix() {
                Status.BestProgress_Reset();
            }
        }

        [LazyPatch("Tags.P_scnEditor.Status__Play", "scnEditor", "Play", Triggers = new string[] {
            nameof(Status.CurCheckPoint),
        })]
        public static class Status__Play {
            public static void Postfix() {
                Status.TotalCheckPoients_Update();
            }
        }

        [LazyPatch("Tags.P_scnEditor.Tile__ResetScene", "scnEditor", "ResetScene", Triggers = new string[] {
            nameof(Tile.StartTile), nameof(Tile.StartProgress)
        })]
        public static class Tile__ResetScene {
            public static void Postfix(scrController __instance) {
                Tile.Started_Reset(__instance);
            }
        }
    }
}
