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
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Int32)]
        public static int Hour() => FastDateTime.Now.Hour;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Int32)]
        public static int Minute() => FastDateTime.Now.Minute;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Int32)]
        public static int Second() => FastDateTime.Now.Second;
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.Int32)]
        public static int MilliSecond() => FastDateTime.Now.Millisecond;
        public static void Reset() { }
    }
}
