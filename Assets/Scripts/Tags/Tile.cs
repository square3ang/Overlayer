namespace Overlayer.Tags
{
    public static class Tile
    {
        [Tag(Dummy=1972)]
        public static int LeftTile;
        [Tag(Dummy=1972)]
        public static int CurTile;
        [Tag(Dummy=3944)]
        public static int TotalTile;
        [Tag(Dummy=0)]
        public static int StartTile;
        [Tag(Dummy=0)]
        public static double StartProgress;
        [Tag(Dummy=true)]
        public static bool IsStarted;
        [Tag(Dummy=1)]
        public static double MarginScale() => scrController.instance?.currFloor?.marginScale ?? 0;
        public static void Reset()
        {
            LeftTile = CurTile = TotalTile = StartTile = 0;
            StartProgress = 0;
            IsStarted = false;
        }
    }
}
