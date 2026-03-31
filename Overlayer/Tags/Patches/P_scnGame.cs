using Overlayer.Core.Patches;

namespace Overlayer.Tags.Patches;

public class P_scnGame : PatchBase<P_scnGame> {
    [LazyPatch("Tags.P_scnGame.ProgressStats__LoadLevel", "scnGame", "LoadLevel", Triggers = new string[] {
        nameof(ProgressStats.BestProgress)
    })]
    public static class ProgressStats__LoadLevel {
        public static void Postfix() => ProgressStats.BestProgress_Reset();
    }

    [LazyPatch("Tags.P_scnGame.Bpm__Play", "scnGame", "Play", Triggers = new string[] {
        nameof(Bpm.TileBpm), nameof(Bpm.CurBpm), nameof(Bpm.RecKPS),
        nameof(Bpm.TileBpmWithoutPitch), nameof(Bpm.CurBpmWithoutPitch), nameof(Bpm.RecKPSWithoutPitch),
    })]
    public static class Bpm__Play {
        public static void Postfix(scrController __instance) => Bpm.Init(__instance);
    }

    [LazyPatch("Tags.P_scnGame.Level__Play", "scnGame", "Play", Triggers = new string[] {
        nameof(Level.Title), nameof(Level.Author), nameof(Level.Artist),
        nameof(Level.TitleRaw), nameof(Level.AuthorRaw), nameof(Level.ArtistRaw),
        nameof(Level.DefaultTextColor), nameof(Level.DefaultTextShadowColor),
        nameof(Level.LevelNameTextColor), nameof(Level.LevelNameTextShadowColor)
    })]
    public static class Level__Play {
        public static void Postfix() => Level.Init();
    }

    [LazyPatch("Tags.P_scnGame.Tile__Play", "scnGame", "Play", Triggers = new string[] {
        nameof(Tile.TileAngle), nameof(Tile.TileEntryAngle), nameof(Tile.TileExitAngle)
    })]
    public static class Tile__Play {
        public static void Postfix() {
            scrFloor floor = scrController.instance?.currFloor;
            if(floor != null) {
                Tile.Angle_Update(floor);
            }
        }
    }

    [LazyPatch("Tags.P_scnGame.Status__Play", "scnGame", "Play", Triggers = new string[] {
        nameof(CheckPointStats.TotalCheckPoints), nameof(CheckPointStats.CurCheckPoint),
    })]
    public static class Status__Play {
        public static void Postfix() => CheckPointStats.InterCheckPoints_Update();
    }

    [LazyPatch("Tags.P_scnGame.Tile__ResetScene", "scnGame", "ResetScene", Triggers = new string[] {
         nameof(Tile.IsStarted),
    })]
    public static class Tile__ResetScene {
        public static void Postfix(scrController __instance) => Tile.IsStarted = false;
    }
}
