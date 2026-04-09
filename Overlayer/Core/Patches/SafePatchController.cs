using Overlayer.Patches;

namespace Overlayer.Core.Patches;

public static class SafePatchController {
    private static readonly SafeConditionalPatch[] patches = [
        new HitFixPatch(),
        new FileAttemptLoadPatch(),
        new FileAttemptSavePatch(),
    ];

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

    public static void ApplyPatch<T>() where T : SafeConditionalPatch {
        foreach(var patch in patches) {
            if(patch is T) {
                patch.Apply();
            }
        }
    }

    public static void RemovePatch<T>() where T : SafeConditionalPatch {
        foreach(var patch in patches) {
            if(patch is T) {
                patch.Remove();
            }
        }
    }
}