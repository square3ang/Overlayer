using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class Tile
    {
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int LeftTile;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int CurTile;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int TotalTile;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int StartTile;
        [Tag(Flags = AdvancedFlags.Round, Hint = ReturnTypeHint.Double)]
        public static double StartProg;
        [Tag(Hint = ReturnTypeHint.Boolean)]
        public static bool IsStarted;
        [Tag(Hint = ReturnTypeHint.Double)]
        public static double MarginScale() => scrController.instance?.currFloor?.marginScale ?? 0;
        public static void Reset()
        {
            LeftTile = CurTile = TotalTile = StartTile = 0;
            StartProg = 0;
            IsStarted = true;
        }
    }
}
