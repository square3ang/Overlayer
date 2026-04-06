using Overlayer.Tags.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Overlayer.Tags;

public static class CheckPointStats {
    [Tag]
    public static int CheckPointUsed => scrController.checkpointsUsed;
    [Tag]
    public static int CurCheckPoint;
    [Tag]
    public static int TotalCheckPoints;

    public static void TotalCheckPoients_Update() => TotalCheckPoints = scrLevelMaker.instance.listFloors.Count(f => f.GetComponent<ffxCheckpoint>() != null);

    public static List<scrFloor> AllCheckPoints;

    public static void AllCheckPoints_Set() => AllCheckPoints = scrLevelMaker.instance.listFloors.FindAll(f => f.GetComponent<ffxCheckpoint>() != null);

    public static void InterCheckPoints_Update() {
        AllCheckPoints = scrLevelMaker.instance.listFloors.FindAll(f => f.GetComponent<ffxCheckpoint>());
        TotalCheckPoints = AllCheckPoints.Count;
    }

    public static int GetCheckPointIndex(scrFloor floor) {
        if(!floor) {
            return 0;
        }
        int i = 0;
        foreach(var chkPt in AllCheckPoints) {
            if(floor.seqID + 1 <= chkPt.seqID) {
                return i;
            }
            i++;
        }
        return i;
    }

    public static void Reset() => CurCheckPoint = TotalCheckPoints = 0;
}
