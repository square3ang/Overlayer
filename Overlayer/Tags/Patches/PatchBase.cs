using Overlayer.Core.Patches;

namespace Overlayer.Tags.Patches
{
    public class PatchBase<T> where T : PatchBase<T>
    {
        public static void Patch() => LazyPatchManager.PatchNested(typeof(T), true);
        public static void Unpatch() => LazyPatchManager.UnpatchNested(typeof(T), true);
    }
}
