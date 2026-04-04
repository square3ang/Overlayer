using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Overlayer.Core.Patches;

public static class SafePatchManager {
    internal static readonly Harmony Harmony = new($"Overlayer.Core.Patches.{nameof(SafePatchManager)}");
    private static readonly HashSet<string> appliedPatches = [];

    public static void ApplyPatch(Type type) {
        if(type == null) {
            Main.Logger.Log($"[{nameof(SafePatch)}] Type is null");
            return;
        }

        var attr = type.GetCustomAttribute<SafePatchAttribute>();
        if(attr == null) {
            Main.Logger.Log($"[{nameof(SafePatch)}] {type.Name} Has No SafePatchAttribute");
            return;
        }

        if(appliedPatches.Contains(attr.Id)) {
            Main.Logger.Log($"[{nameof(SafePatch)}] {attr.Id} Already Applied");
            return;
        }

        try {
            var method = SafePatch.GetMethodSafe(attr.TargetType, attr.TargetMethod);
            var transpiler = type.GetMethod("Transpiler", BindingFlags.Public | BindingFlags.Static);
            Harmony.Patch(method, transpiler: new HarmonyMethod(transpiler));
            appliedPatches.Add(attr.Id);
            Main.Logger.Log($"[{nameof(SafePatch)}] {attr.Id} Patched");
        } catch(Exception e) {
            Main.Logger.Error($"[{nameof(SafePatch)}] {attr.Id} Patch Failed: {e.Message}");
        }
    }

    public static void RemovePatch(Type type) {
        if(type == null) {
            Main.Logger.Log($"[{nameof(SafePatch)}] Type is null, cannot unpatch");
            return;
        }

        var attr = type.GetCustomAttribute<SafePatchAttribute>();
        if(attr == null) {
            Main.Logger.Log($"[{nameof(SafePatch)}] {type.Name} Has No SafePatchAttribute, cannot unpatch");
            return;
        }

        if(!appliedPatches.Contains(attr.Id)) {
            Main.Logger.Log($"[{nameof(SafePatch)}] {attr.Id} Is Not Applied, nothing to unpatch");
            return;
        }

        try {
            var method = SafePatch.GetMethodSafe(attr.TargetType, attr.TargetMethod);
            Harmony.Unpatch(method, HarmonyPatchType.All, Harmony.Id);
            appliedPatches.Remove(attr.Id);
            Main.Logger.Log($"[{nameof(SafePatch)}] {attr.Id} Unpatched");
        } catch(Exception e) {
            Main.Logger.Error($"[{nameof(SafePatch)}] {attr.Id} Unpatch Failed: {e.Message}");
        }
    }
}