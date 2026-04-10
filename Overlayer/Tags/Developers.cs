using Overlayer.Tags.Attributes;

namespace Overlayer.Tags;

public static class Developers {
    // US
    [Tag(NotPlaying = true)]
    [TagDesc("modlist.org")]
    public static string Developer => Main.Lang.Get("MISC_DEVELOPER", "modlist.org. Display everything as you wish. Thank you for being with Overlayer.");
    [Tag(NotPlaying = true)]
    [TagDesc("Us")]
    public static string Modlist => "We Still Alive!";

    // MAIN LEADER
    [Tag(NotPlaying = true)]
    [TagDesc("Main Leader")]
    public static string Kkitut => "Thank you all.";
    [Tag(NotPlaying = true)]
    [TagDesc("Old Leader")]
    public static string Square3ang => "triangle <b>square</b> pentagon....";

    // OTHER
    [Tag(NotPlaying = true)]
    [TagDesc("Goodbye, Forever.")]
    public static string MipaNyang => "MipaNyang is God";
    [Tag("imBBBT", NotPlaying = true)]
    //[TagDesc("")]
    public static string ImBBBT => "imBBBT is not beepbit futures";
}
