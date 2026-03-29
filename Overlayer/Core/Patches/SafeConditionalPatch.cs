using HarmonyLib;
using System;
using System.Reflection;

namespace Overlayer.Core.Patches;

public abstract class SafeConditionalPatch(string id) {
    public string Id { get; } = id;
    public bool IsApplied { get; private set; }

    public void Apply() {
        if(IsApplied || !ShouldApply()) {
            return;
        }

        try {
            var method = GetTargetMethod();
            SafePatchManager.Harmony.Patch(method, Prefix(), Postfix(), Transpiler());
            IsApplied = true;
            Main.Logger.Log($"[SafePatch] {Id} Applied");
        } catch(Exception e) {
            Main.Logger.Error($"[SafePatch] {Id} Apply Failed: {e.Message}");
        }
    }

    public void Remove() {
        if(!IsApplied) {
            return;
        }

        try {
            var method = GetTargetMethod();
            SafePatchManager.Harmony.Unpatch(method, HarmonyPatchType.All, SafePatchManager.Harmony.Id);
            IsApplied = false;
            Main.Logger.Log($"[SafePatch] {Id} Removed");
        } catch(Exception e) {
            Main.Logger.Error($"[SafePatch] {Id} Remove Failed: {e.Message}");
        }
    }

    protected abstract bool ShouldApply();
    protected abstract MethodBase GetTargetMethod();
    protected virtual HarmonyMethod Prefix() => null;
    protected virtual HarmonyMethod Postfix() => null;
    protected virtual HarmonyMethod Transpiler() => null;
}