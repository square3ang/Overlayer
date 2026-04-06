using System;
using Overlayer.Core.Patches;

namespace Overlayer.Tags.Patches;

public class P_scrConductor : PatchBase<P_scrConductor> {
    [LazyPatch("Tags.P_scrConductor.Song__Update", "scrConductor", "Update", Triggers = [
        nameof(Song.CurMinute), nameof(Song.CurSecond), nameof(Song.CurMilliSecond),
        nameof(Song.TotalMinute), nameof(Song.TotalSecond), nameof(Song.TotalMilliSecond)
    ])]
    public static class Song__Update {
        public static void Postfix(scrConductor __instance) {
            if (scrController.instance.paused || !__instance.isGameWorld) return;
            var song = __instance.song;
            if (!song.clip) return;
            var nowt = TimeSpan.FromSeconds(song.time);
            var tott = TimeSpan.FromSeconds(song.clip.length);

            Song.CurDay = nowt.Days;
            Song.CurHour = nowt.Hours;
            Song.CurMinute = nowt.Minutes;
            Song.CurSecond = nowt.Seconds;
            Song.CurMilliSecond = nowt.Milliseconds;

            Song.TotalDay = tott.Days;
            Song.TotalHour = tott.Hours;
            Song.TotalMinute = tott.Minutes;
            Song.TotalSecond = tott.Seconds;
            Song.TotalMilliSecond = tott.Milliseconds;
        }
    }
}