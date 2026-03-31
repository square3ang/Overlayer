using GDMiniJSON;
using Newtonsoft.Json.Linq;
using Overlayer.Tags.Attributes;
using System.IO;

namespace Overlayer.Tags;

public static class Status {
    [Tag]
    public static bool IsAutoEnabled => ADOFAI.RDC?.auto ?? false;
    [Tag]
    public static bool IsOldAutoEnabled => ADOFAI.RDC?.useOldAuto ?? false;
    [Tag]
    public static bool IsPracticeModeEnabled => ADOFAI.RDC?.practice ?? false;
    [Tag]
    public static bool IsNoFailEnabled => ADOFAI.Controller?.noFail ?? GCS.useNoFail;
    [Tag]
    public static bool IsSpeedTrialEnabled => GCS.speedTrialMode;
    [Tag(NotPlaying = true)]
    public static int Deaths => scrController.deaths;
    [Tag]
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
    public static int FileAttempts() => Main.FileAttempt?.GetAttempts() ?? -1;
    [Tag]
    public static int FileTileAttempts(int tile) => Main.FileAttempt?.GetTileAttempts(tile) ?? -1;

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double Pitch => GCS.currentSpeedTrial;
    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double EditorPitch => (ADOFAI.LevelData?.pitch ?? 0) / 100.0;
}
