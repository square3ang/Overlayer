using Overlayer.Tags.Attributes;

namespace Overlayer.Tags;

public static class Hex {
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.TrimString)]
    [TagDesc("TooEarly Hex Color")]
    public static string TEHex = "FF0000FF";
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.TrimString)]
    [TagDesc("VeryEarly Hex Color")]
    public static string VEHex = "FF6F4EFF";
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.TrimString)]
    [TagDesc("EarlyPerfect Hex Color")]
    public static string EPHex = "A0FF4EFF";
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.TrimString)]
    [TagDesc("Perfect Hex Color")]
    public static string PHex = "60FF4EFF";
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.TrimString)]
    [TagDesc("LatePerfect Hex Color")]
    public static string LPHex = "A0FF4EFF";
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.TrimString)]
    [TagDesc("VeryLate Hex Color")]
    public static string VLHex = "FF6F4EFF";
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.TrimString)]
    [TagDesc("TooLate Hex Color")]
    public static string TLHex = "FF0000FF";
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.TrimString)]
    [TagDesc("Multipress Hex Color")]
    public static string MPHex = "00FFEDFF";
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.TrimString)]
    [TagDesc("Miss Hex Color")]
    public static string FMHex = "D958FFFF";
    [Tag(NotPlaying = true, ProcessingFlags = ValueProcessing.TrimString)]
    [TagDesc("Overload Hex Color")]
    public static string FOHex = "D958FFFF";
    public static void Reset() { }
}
