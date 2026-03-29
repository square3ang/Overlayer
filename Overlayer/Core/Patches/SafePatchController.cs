using Overlayer.Patches;

namespace Overlayer.Core.Patches;

public static class SafePatchController {
    private static readonly SafeConditionalPatch[] patches = {
        new HitFixPatch(),
    };

    public static void ApplyAll() {
        foreach(var patch in patches) {
            patch.Apply();
        }
    }

    public static void UnloadAll() {
        foreach(var patch in patches) {
            patch.Remove();
        }
    }
}