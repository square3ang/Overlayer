using Overlayer.Core.Patches;

namespace Overlayer.Patches;

public static class BlockUMMClosing
{
    public static bool Block;
    [LazyPatch("Patches.BlockUMMClosing", "UnityModManager.UI", "ToggleWindow")]
    public static class BlockUMMClosingPatch
    {
        public static bool Prefix()
        {
            return !Block;
        }
    }
}