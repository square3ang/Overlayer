using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class Tile
    {
        [Tag]
        public static int LeftTile;
        [Tag]
        public static int CurTile;
        [Tag]
        public static int TotalTile;
        [Tag]
        public static int StartTile;
        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double StartProgress;
        [Tag]
        public static bool IsStarted;

        public static void Started_Reset(scrController controller) {
            IsStarted = false;
        }

        public static void Start_Init(scrController controller) {
            if(!IsStarted) {
                IsStarted = true;
                if(controller.gameworld) {
                    StartProgress = controller.percentComplete * 100;
                    StartTile = controller.currentSeqID + 1;
                }
            }
        }

        [Tag(ProcessingFlags = ValueProcessing.RoundNumber)]
        public static double MarginScale() => scrController.instance?.currFloor?.marginScale ?? 0;
        public static void Reset() {
            LeftTile = CurTile = TotalTile = StartTile = 0;
            StartProgress = 0;
            IsStarted = false;
        }
    }
}
