using Overlayer.Tags.Attributes;

namespace Overlayer.Tags;

public static class Status {
    [Tag]
    [TagDesc("Displays true if auto is enabled, false otherwise")]
    public static bool IsAutoEnabled => ADOFAI.RDC?.auto ?? false;
    [Tag]
    [TagDesc("Displays true if the current tile is an autotile, false otherwise")]
    public static bool IsAutoTile => scrLevelMaker.instance?.listFloors[Tile.CurTile]?.auto ?? false;
    [Tag]
    [TagDesc("Displays true if Old Auto is enabled, false if disabled")]
    public static bool IsOldAutoEnabled => ADOFAI.RDC?.useOldAuto ?? false;
    [Tag]
    [TagDesc("Displays true if practice mode is enabled, false otherwise")]
    public static bool IsPracticeModeEnabled => ADOFAI.RDC?.practice ?? false;
    [Tag]
    [TagDesc("Displays true if No-fail Mode is enabled, false if disabled")]
    public static bool IsNoFailEnabled => ADOFAI.Controller?.noFail ?? GCS.useNoFail;
    [Tag]
    [TagDesc("Displays true if Speed Trial Mode is enabled, false otherwise.\nOnly works on CLS and official levels.")]
    public static bool IsSpeedTrialEnabled => GCS.speedTrialMode;
    [Tag(NotPlaying = true)]
    [TagDesc("The total number of planet explosions on the current map")]
    public static int Deaths => scrController.deaths;
    [Tag]
    [TagDesc("Shows Attempts. Only effective at CLS and official levels.")]
    public static int Attempts;

    public static void Attempts_Update() {
        if(scnGame.instance == null) {
            Attempts = scrController.instance != null && scrConductor.instance != null
                ? ADOBase.sceneName.Contains("-") && !scrController.instance.noFail && scrConductor.instance.isGameWorld
                    ? Persistence.GetWorldAttempts(scrController.currentWorld)
                    : 0
                : 0;
        } else {
            if(scnEditor.instance == null) {
                var level = ADOFAI.LevelData;
                Attempts = level != null
                    ? Persistence.GetCustomWorldAttempts(
                        MD5Hash.GetHash(level.author + level.artist + level.song)
                    )
                    : 0;
            } else {
                Attempts = 0;
            }
        }
    }

    public static void Attempts_UpdateGame() {
        var level = ADOFAI.LevelData;
        Attempts = Persistence.GetCustomWorldAttempts(
            MD5Hash.GetHash(level.author + level.artist + level.song)
        );
    }

    public static void Attempts_UpdateOfficial() {
        Attempts = ADOBase.sceneName.Contains("-") && !scrController.instance.noFail && scrConductor.instance.isGameWorld
            ? Persistence.GetWorldAttempts(scrController.currentWorld)
            : 0;
    }

    [Tag]
    [TagDesc("Current attempt count.\nRequires File Attempt setting to be enabled.")]
    public static int FileAttempts() => Main.FileAttempt?.GetAttempts() ?? -1;
    [Tag]
    [TagDesc("Current attempt count for the start tile.\nRequires File Attempt setting to be enabled.")]
    public static int FileTileAttempts(int tile = -1) {
        if(tile < 0) {
            tile = Tile.StartTile;
        }
        return Main.FileAttempt?.GetTileAttempts(tile) ?? -1;
    }

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Speed set in CLS (displayed as 1 when at 1x speed)")]
    public static double Pitch => GCS.currentSpeedTrial;
    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Pitch set in the LevelEditor (displayed as 1 when 100%)")]
    public static double EditorPitch => (ADOFAI.LevelData?.pitch ?? 0) / 100.0;
}
