using System;
using HarmonyLib;
using UnityEngine;

namespace Overlayer.Tags.Patches
{
    public class SongPatch
    {
        [HarmonyPatch(typeof(scrConductor), "Update")]
        public static class SongTimePatch
        {
            public static void Postfix(scrConductor __instance)
            {
                if (scrController.instance.paused || !__instance.isGameWorld) return;
                AudioSource song = __instance.song;
                if (!song.clip) return;
                TimeSpan nowt = TimeSpan.FromSeconds(song.time);
                TimeSpan tott = TimeSpan.FromSeconds(song.clip.length);
                Song.CurMinute = nowt.Minutes;
                Song.CurSecond = nowt.Seconds;
                Song.CurMilliSecond = nowt.Milliseconds;
                Song.TotalMinute = tott.Minutes;
                Song.TotalSecond = tott.Seconds;
                Song.TotalMilliSecond = tott.Milliseconds;
            }
        }
    }
}