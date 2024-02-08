using System;
using Overlayer.Core;
using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class Time
    {
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Int32)]
        public static int Year() => FastDateTime.Now.Year;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Int32)]
        public static int Month() => FastDateTime.Now.Month;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Int32)]
        public static int Day() => FastDateTime.Now.Day;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Double)]
        public static double Days() => TimeSpan.FromTicks(FastDateTime.Now.Ticks).TotalDays;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Int32)]
        public static int Hour() => FastDateTime.Now.Hour;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Double)]
        public static double Hours() => TimeSpan.FromTicks(FastDateTime.Now.Ticks).TotalHours;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Int32)]
        public static int Minute() => FastDateTime.Now.Minute;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Double)]
        public static double Minutes() => TimeSpan.FromTicks(FastDateTime.Now.Ticks).TotalMinutes;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Int32)]
        public static int Second() => FastDateTime.Now.Second;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Double)]
        public static double Seconds() => TimeSpan.FromTicks(FastDateTime.Now.Ticks).TotalSeconds;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Int32)]
        public static int MilliSecond() => FastDateTime.Now.Millisecond;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Double)]
        public static double MilliSeconds() => TimeSpan.FromTicks(FastDateTime.Now.Ticks).TotalMilliseconds;
        public static void Reset() { }
    }
}
