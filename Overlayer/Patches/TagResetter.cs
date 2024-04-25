﻿using Overlayer.Core.Patches;
using Overlayer.Tags;

namespace Overlayer.Patches
{
    [LazyPatch("Patches.TagResetter", "scrController", "Awake_Rewind")]
    public static class TagResetter
    {
        public static void Postfix()
        {
            OverlayerAPI.Reset();
            if (TagManager.HasReference(typeof(Adofaigg)))
                Adofaigg.Reset();
            if (TagManager.HasReference(typeof(TUF)))
                TUF.Reset();
            Bpm.Reset();
            Tags.FrameRate.Reset();
            Hex.Reset();
            Hit.Reset();
            HitTiming.Reset();
            Tags.Level.Reset();
            Song.Reset();
            Status.Reset();
            Tile.Reset();
            Time.Reset();
        }
    }
}
