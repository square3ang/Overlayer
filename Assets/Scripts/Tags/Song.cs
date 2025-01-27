namespace Overlayer.Tags
{
    public static class Song
    {
        [Tag(Dummy=1)]
        public static int CurMinute;
        [Tag(Dummy=23)]
        public static int CurSecond;
        [Tag(Dummy=456)]
        public static int CurMilliSecond;
        [Tag(Dummy=1)]
        public static int TotalMinute;
        [Tag(Dummy=83)]
        public static int TotalSecond;
        [Tag(Dummy=83456)]
        public static int TotalMilliSecond;
        public static void Reset()
        {
            CurMinute = CurSecond = CurMilliSecond = 0;
            TotalMinute = TotalSecond = TotalMilliSecond = 0;
        }
    }
}
