using System.Reflection;
using HarmonyLib;
using Overlayer.Core.Patches;

namespace Overlayer.Patches;

public class FileAttemptLoadPatch : SafeConditionalPatch {
    public FileAttemptLoadPatch() : base(nameof(FileAttemptLoadPatch)) { }

    protected override bool ShouldApply() {
        return Main.Settings.FileAttempt;
    }

    protected override MethodBase GetTargetMethod() {
        return SafePatch.GetMethodSafe("scnGame", "LoadLevel");
    }

    protected override HarmonyMethod Postfix() {
        return new HarmonyMethod(typeof(FileAttemptLoadPatch).GetMethod(nameof(PostfixImpl),
            BindingFlags.Static | BindingFlags.NonPublic));
    }

    private static void PostfixImpl() {
        Main.FileAttempt?.Load();
    }
}

public class FileAttemptSavePatch : SafeConditionalPatch {
    public FileAttemptSavePatch() : base(nameof(FileAttemptSavePatch)) { }

    protected override bool ShouldApply() {
        return Main.Settings.FileAttempt;
    }

    protected override MethodBase GetTargetMethod() {
        return SafePatch.GetMethodSafe("scnGame", "Play");
    }

    protected override HarmonyMethod Postfix() {
        return new HarmonyMethod(typeof(FileAttemptSavePatch).GetMethod(nameof(PostfixImpl),
            BindingFlags.Static | BindingFlags.NonPublic));
    }

    private static void PostfixImpl(scnGame __instance, int seqID = 0) {
        if (Main.FileAttempt == null || string.IsNullOrEmpty(__instance.levelPath)) return;
        Main.FileAttempt.IncreaseAttempts();
        Main.FileAttempt.IncreaseTileAttempts(seqID);
        Main.FileAttempt.Save();
    }
}