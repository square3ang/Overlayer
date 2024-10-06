using Overlayer.Core.Patches;

namespace Overlayer.Patches;

public static class BlockUMMClosing
{
    public static bool Block;

    [LazyPatch("Patches.BlockUMMClosing", "UnityModManagerNet.UnityModManager+UI", "ToggleWindow", new string[] { "System.Boolean" })]
    public static class BlockUMMClosingPatch
    {
        public static bool Prefix()
        {
            
            return !Block;
        }
    }
}