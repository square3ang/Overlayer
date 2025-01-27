using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Overlayer.Tags;
using UnityEngine;
using Time = Overlayer.Tags.Time;

namespace Overlayer
{
    [HarmonyPatch(typeof(scrController), "Awake_Rewind")]
    public static class TagResetter
    {
        public static void Postfix()
        {
            OverlayerAPI.Reset();
            Bpm.Reset();
            Tags.FrameRate.Reset();
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
