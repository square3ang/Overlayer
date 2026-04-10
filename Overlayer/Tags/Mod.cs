using Overlayer.Tags.Attributes;
using System;

namespace Overlayer.Tags;

public static class Mod {

    [Tag(NotPlaying = true)]
    [TagDesc("Current Overlayer Version")]
    public static Version ModVersion => Main.Mod.Version;
}
