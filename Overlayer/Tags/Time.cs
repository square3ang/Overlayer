using Overlayer.Core;
using Overlayer.Tags.Attributes;
using System;

namespace Overlayer.Tags;

public static class Time {
    [Tag(NotPlaying = true)]
    [TagDesc("Year on current computer")]
    public static int Year => FastDateTime.Now.Year;
    [Tag(NotPlaying = true)]
    [TagDesc("Month on current computer")]
    public static int Month => FastDateTime.Now.Month;
    [Tag(NotPlaying = true)]
    [TagDesc("Day on current computer")]
    public static int Day => FastDateTime.Now.Day;
    [Tag(NotPlaying = true)]
    [TagDesc("All days so far")]
    public static double Days => TimeSpan.FromTicks(FastDateTime.Now.Ticks).TotalDays;
    [Tag(NotPlaying = true)]
    [TagDesc("Hour on current computer")]
    public static int Hour => FastDateTime.Now.Hour;
    [Tag(NotPlaying = true)]
    [TagDesc("Total hours so far")]
    public static double Hours => TimeSpan.FromTicks(FastDateTime.Now.Ticks).TotalHours;
    [Tag(NotPlaying = true)]
    [TagDesc("Minutes on current computer")]
    public static int Minute => FastDateTime.Now.Minute;
    [Tag(NotPlaying = true)]
    [TagDesc("Total minutes so far")]
    public static double Minutes => TimeSpan.FromTicks(FastDateTime.Now.Ticks).TotalMinutes;
    [Tag(NotPlaying = true)]
    [TagDesc("Seconds on current computer")]
    public static int Second => FastDateTime.Now.Second;
    [Tag(NotPlaying = true)]
    [TagDesc("Total seconds so far")]
    public static double Seconds => TimeSpan.FromTicks(FastDateTime.Now.Ticks).TotalSeconds;
    [Tag(NotPlaying = true)]
    [TagDesc("Milliseconds on current computer")]
    public static int MilliSecond => FastDateTime.Now.Millisecond;
    [Tag(NotPlaying = true)]
    [TagDesc("Total milliseconds so far")]
    public static double MilliSeconds => TimeSpan.FromTicks(FastDateTime.Now.Ticks).TotalMilliseconds;
    [Tag(NotPlaying = true)]
    [TagDesc("Current DateTime ticks (100ns units)")]
    public static long Ticks => FastDateTime.Now.Ticks;
    public static void Reset() { }
}
