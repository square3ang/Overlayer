using Overlayer.Tags.Attributes;

namespace Overlayer.Tags;

public static class Bpm {
    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double TileBpm;

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double MaxTileBpm => scnGame.instance.highestBPM;

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double CurBpm;

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double RecKPS;

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double TileBpmWithoutPitch;

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double CurBpmWithoutPitch;

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    public static double RecKPSWithoutPitch;

    public static float bpm, pitch, bpmwithoutpitch, playbackSpeed = 1;

    public static void Init(scrController __instance) {
        if (scnGame.instance == null && scnEditor.instance == null &&
            !(scrController.instance?.gameworld ?? false)) return;

        if (scnGame.instance != null) {
            pitch = (float)scnGame.instance.levelData.pitch / 100;
            if (ADOBase.isOfficialLevel) pitch *= scrConductor.instance.song.pitch;
            if (ADOBase.isCLSLevel) pitch *= GCS.currentSpeedTrial;
            if (scnEditor.instance != null) pitch *= scnEditor.instance.playbackSpeed;
            bpm = scnGame.instance.levelData.bpm * pitch;
            bpmwithoutpitch = scnGame.instance.levelData.bpm;
        }
        else {
            pitch = scrConductor.instance.song.pitch;
            bpm = scrConductor.instance.bpm * pitch;
            bpmwithoutpitch = scrConductor.instance.bpm;
        }

        var cur = bpm;
        if (__instance.currentSeqID != 0) {
            var speed = scrController.instance.speed;
            cur = (float)(bpm * speed);
        }

        TileBpm = cur;
        CurBpm = cur;
        RecKPS = cur / 60;
    }

    public static double GetRealBpm(scrFloor floor, float bpm) {
        return floor == null
            ? bpm
            : floor.nextfloor == null
                ? scrController.instance.speed * bpm
                : 60.0 / (floor.nextfloor.entryTime - floor.entryTime);
    }

    public static void Update(scrFloor floor) {
        if (floor.nextfloor is null) return;

        var curBPM = GetRealBpm(floor, bpm) * pitch;

        TileBpm = bpm * scrController.instance.speed;
        CurBpm = curBPM;
        RecKPS = curBPM / 60;

        var curBPMWithoutPitch = GetRealBpm(floor, bpmwithoutpitch);
        TileBpmWithoutPitch = bpmwithoutpitch * scrController.instance.speed;
        CurBpmWithoutPitch = curBPMWithoutPitch;
        RecKPSWithoutPitch = curBPMWithoutPitch / 60;
    }

    public static void Reset() {
        TileBpm = CurBpm = RecKPS = 0;
        TileBpmWithoutPitch = CurBpmWithoutPitch = RecKPSWithoutPitch = 0;
    }
}