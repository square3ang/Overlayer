using Overlayer.Patches;

namespace Overlayer.Core.Patches;

public static class SafePatchController {
    private static readonly SafeConditionalPatch[] patches = [
        new HitFixPatch(),
        new FileAttemptLoadPatch(),
        new FileAttemptSavePatch()
    ];

    public static void ApplyAll() {
        foreach (var patch in patches) patch.Apply();
    }

    public static void UnloadAll() {
        foreach (var patch in patches) patch.Remove();
    }
}