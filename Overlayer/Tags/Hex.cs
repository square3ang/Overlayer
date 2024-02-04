using Overlayer.Tags.Attributes;

namespace Overlayer.Tags
{
    public static class Hex
    {
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.String)]
        public static string TEHex = "FF0000FF";
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.String)]
        public static string VEHex = "FF6F4EFF";
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.String)]
        public static string EPHex = "A0FF4EFF";
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.String)]
        public static string PHex = "60FF4EFF";
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.String)]
        public static string LPHex = "A0FF4EFF";
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.String)]
        public static string VLHex = "FF6F4EFF";
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.String)]
        public static string TLHex = "FF0000FF";
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.String)]
        public static string MPHex = "00FFEDFF";
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.String)]
        public static string FMHex = "D958FFFF";
        [Tag(NotPlaying = true, Hint = ReturnTypeHint.String)]
        public static string FOHex = "D958FFFF";
        public static void Reset() { }
    }
}
