using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class Song
    {
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int CurMinute;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int CurSecond;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int CurMilliSecond;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int TotalMinute;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int TotalSecond;
        [Tag(Hint = ReturnTypeHint.Int32)]
        public static int TotalMilliSecond;
        public static void Reset()
        {
            CurMinute = CurSecond = CurMilliSecond = 0;
            TotalMinute = TotalSecond = TotalMilliSecond = 0;
        }
    }
}
