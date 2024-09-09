using Overlayer.Core.Patches;
using System;
using System.Reflection;

namespace Overlayer.Patches
{
    [LazyPatch("Patches.PatchGuard", "HarmonyLib.PatchTools", "GetOriginalMethod")]
    public static class PatchGuard
    {
        public static void Postfix(ref MethodBase __result)
        {
            if (__result.DeclaringType?.Assembly == Main.Ass)
            {
                __result = null;
                Main.Logger.Log($"Overlayer Patch Denied!");
            }
        }
    }
    [LazyPatch("Patches.PatchGuard2", "System.Reflection.RuntimeMethodInfo", "get_MethodHandle")]
    public static class PatchGuard2
    {
        public static void Prefix(MethodInfo __instance)
        {
            if (__instance.DeclaringType?.Assembly == Main.Ass)
                throw new InvalidOperationException("Cannot Get Method Handle From Declared On Overlayer!!");
        }
    }
}
