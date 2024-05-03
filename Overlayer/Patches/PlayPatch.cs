using Overlayer.Core;
using Overlayer.Core.Patches;
using Overlayer.Utils;

namespace Overlayer.Patches
{
    [LazyPatch("Patches.PlayPatch", "scrController", "Start_Rewind")]
    public static class PlayPatch
    {
        public static void Postfix(scrController __instance, int _currentSeqID)
        {
            if (!__instance.gameworld && _currentSeqID < 0) return;
            OverlayerWebAPI.Play().Await();
        }
    }
}
