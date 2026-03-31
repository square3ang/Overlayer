using HarmonyLib;
using Overlayer.Core.Patches;
using System.Reflection;

namespace Overlayer.Patches;

public class FileAttemptLoadPatch : SafeConditionalPatch {
    public FileAttemptLoadPatch() : base(nameof(FileAttemptLoadPatch)) { }

    protected override bool ShouldApply() => Main.Settings.FileAttempt;

    protected override MethodBase GetTargetMethod() =>
        SafePatch.GetMethodSafe("scnGame", "LoadLevel");

    protected override HarmonyMethod Postfix() =>
        new(typeof(FileAttemptLoadPatch).GetMethod(nameof(PostfixImpl), BindingFlags.Static | BindingFlags.NonPublic));

    private static void PostfixImpl() {
        Main.FileAttempt?.Load();
    }
}

public class FileAttemptSavePatch : SafeConditionalPatch {
    public FileAttemptSavePatch() : base(nameof(FileAttemptSavePatch)) { }

    protected override bool ShouldApply() => Main.Settings.FileAttempt;

    protected override MethodBase GetTargetMethod() =>
        SafePatch.GetMethodSafe("scrController", "Fail2Action");

    protected override HarmonyMethod Postfix() =>
        new(typeof(FileAttemptSavePatch).GetMethod(nameof(PostfixImpl), BindingFlags.Static | BindingFlags.NonPublic));

    private static void PostfixImpl() {
        if(Main.FileAttempt == null) {
            return;
        }
        Main.FileAttempt.Attempts++;

        Main.FileAttempt.Save();
    }
}