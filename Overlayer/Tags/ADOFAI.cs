using ADOFAI;

namespace Overlayer.Tags;

public static class ADOFAI {
    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static LevelData LevelData => scnGame.instance?.levelData ?? scnEditor.instance?.levelData;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static RDConstants RDC => RDConstants.data;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static scrController Controller => scrController.instance;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static scrConductor Conductor => scrConductor.instance;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static scrLevelMaker LevelMaker => scrLevelMaker.instance;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static scrPlanet chosenPlanet => Controller?.chosenPlanet;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static scrPlanet planetRed => Controller?.planetRed;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static scrPlanet planetBlue => Controller?.planetBlue;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static scnCLS CLS => scnCLS.instance;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static scnEditor Editor => scnEditor.instance;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static scnGame CustomLevel => scnGame.instance;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static scrFloor CurrentFloor => Controller?.currFloor;

    //[Tag(ProcessingFlags = ValueProcessing.AccessMember, NotPlaying = true)]
    public static scrMistakesManager JudgementManager => Controller?.mistakesManager;
}