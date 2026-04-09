using Overlayer.Tags.Attributes;

namespace Overlayer.Tags;

public static class Tile {
    [Tag]
    [TagDesc("Number of tiles left")]
    public static int LeftTile;
    [Tag]
    [TagDesc("Current tile number")]
    public static int CurTile;
    [Tag]
    [TagDesc("Total number of tiles")]
    public static int TotalTile;
    [Tag]
    [TagDesc("Start tile number")]
    public static int StartTile;
    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Progress of start tile")]
    public static double StartProgress;
    [Tag]
    [TagDesc("Displays false if not started, or true if started by pressing any key")]
    public static bool IsStarted;

    public static void SetStartValues(scrController controller, int tile) {
        if(!controller ||
           !ADOBase.controller ||
           !ADOBase.lm ||
           ADOBase.lm.listFloors == null ||
           ADOBase.lm.listFloors.Count == 0) {
            return;
        }

        StartProgress = controller.percentComplete * 100;
        StartTile = tile;
    }

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Level's judgment range")]
    public static double MarginScale => (scrController.instance?.currFloor?.marginScale ?? 0) * 100d;

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("The angle of the current tile")]
    public static double TileAngle;

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Indicates the entry angle from the entry tile to the current tile.")]
    public static double TileEntryAngle;

    [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
    [TagDesc("Indicates the exit angle from the current tile to the next tile.")]
    public static double TileExitAngle;

    const double RAD_TO_DEG = 57.29577951308232;
    public static void Angle_Update(scrFloor floor) {
        TileAngle = floor.angleLength * RAD_TO_DEG;
        TileEntryAngle = floor.entryangle * RAD_TO_DEG;
        TileExitAngle = floor.exitangle * RAD_TO_DEG;
    }

    public static void Reset() {
        LeftTile = CurTile = TotalTile = StartTile = 0;
        StartProgress = 0;
        IsStarted = false;
        TileAngle = TileEntryAngle = TileExitAngle = double.NaN;
    }
}
