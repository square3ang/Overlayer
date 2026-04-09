using Overlayer.Tags.Attributes;

namespace Overlayer.Tags;

public static class Song {
    [Tag]
    [TagDesc("Current day of song")]
    public static int CurDay;
    [Tag]
    [TagDesc("Current hour of song")]
    public static int CurHour;
    [Tag]
    [TagDesc("Current minute of song")]
    public static int CurMinute;
    [Tag]
    [TagDesc("Current second of song")]
    public static int CurSecond;
    [Tag]
    [TagDesc("Current millisecond of song")]
    public static int CurMilliSecond;

    [Tag]
    [TagDesc("Total day of song")]
    public static int TotalDay;
    [Tag]
    [TagDesc("Total hour of song")]
    public static int TotalHour;
    [Tag]
    [TagDesc("Total minute of song")]
    public static int TotalMinute;
    [Tag]
    [TagDesc("Total second of song")]
    public static int TotalSecond;
    [Tag]
    [TagDesc("Total millisecond of song")]
    public static int TotalMilliSecond;

    public static void Reset() {
        CurDay = CurHour = CurMinute = CurSecond = CurMilliSecond = 0;
        TotalDay = TotalHour = TotalMinute = TotalSecond = TotalMilliSecond = 0;
    }
}
