using HarmonyLib;
using Overlayer.Core;
using Overlayer.Core.Patches;
using System;
using System.Linq;
using System.Reflection;

namespace Overlayer.Patches
{
    internal static class PatchGuard
    {
        private static bool IgnoreGuard = false;
        public static void Ignore(Action func)
        {
            IgnoreGuard = true;
            try { func?.Invoke(); }
            catch { throw; }
            finally { IgnoreGuard = false; }
        }
        public static void ForceIgnore() => IgnoreGuard = true;
        public static void Recover() => IgnoreGuard = false;
        public static bool Ignored => IgnoreGuard;
        [LazyPatch("Patches.PatchGuard_Harmony", "HarmonyLib.PatchTools", "GetOriginalMethod")]
        public static class PatchGuard_Harmony
        {
            public static void Postfix(ref MethodBase __result)
            {
                if (IgnoreGuard) return;
                if (__result.DeclaringType?.Assembly == Main.Ass)
                {
                    __result = null;
                    Main.Logger.Log($"Overlayer Patch Denied!");
                }
            }
        }
        [LazyPatch("Patches.PatchGuard_Harmony2", "HarmonyLib.PatchInfo", "RemovePatch")]
        public static class PatchGuard_Harmony2
        {
            public static bool Prefix(MethodBase patch)
            {
                if (IgnoreGuard) return true;
                if (patch.DeclaringType?.Assembly == Main.Ass)
                {
                    Main.Logger.Log($"Overlayer Unpatch Denied!");
                    return false;
                }
                return true;
            }
        }
        [LazyPatch("Patches.PatchGuard_MethodHandle", "System.Reflection.RuntimeMethodInfo", "get_MethodHandle")]
        [LazyPatch("Patches.PatchGuard_MethodHandle2", "System.Reflection.RuntimeMethodInfo", "GetMethodBody", new string[] { })]
        public static class PatchGuard_MethodHandle
        {
            public static void Prefix(MethodInfo __instance)
            {
                if (IgnoreGuard) return;
                if (__instance.DeclaringType?.Assembly == Main.Ass)
                    throw new InvalidOperationException("Cannot Get Method Handle From Declared On Overlayer!!");
            }
        }
        [LazyPatch("Patches.PatchGuard_Type", "System.RuntimeType", "GetMethodCandidates", new string[] { "System.String", "System.Reflection.BindingFlags", "System.Reflection.CallingConventions", "System.Type[]", "Int32", "Boolean" })]
        [LazyPatch("Patches.PatchGuard_Type2", "System.RuntimeType", "GetMethodCandidates", new string[] { "System.String", "Int32", "System.Reflection.BindingFlags", "System.Reflection.CallingConventions", "System.Type[]", "Boolean" })]
        [LazyPatch("Patches.PatchGuard_Type3", "System.RuntimeType", "GetPropertyCandidates")]
        [LazyPatch("Patches.PatchGuard_Type4", "System.RuntimeType", "GetFieldCandidates")]
        public static class PatchGuard_Type
        {
            public static void Postfix(object __result)
            {
                if (IgnoreGuard) return;
                var arr = __result.GetType().GetField("_items", (BindingFlags)15420).GetValue(__result) as MemberInfo[];
                if (arr == null) return;
                if (arr.Any(m => (m?.DeclaringType?.FullName?.StartsWithNotContains("System.Runtime", "Handle") ?? false) || (m?.DeclaringType?.FullName?.StartsWithNotContains("System.Reflection.Runtime", "Handle") ?? false)))
                    throw new InvalidOperationException("Cannot Access To Internal Type!");
                if (arr.Any(m =>
                    (
                        m?.DeclaringType != typeof(Tags.Adofaigg.GGRatingHolder) &&
                        m?.DeclaringType != typeof(Tags.Adofaigg.Rank) &&
                        m?.DeclaringType != typeof(OverlayerWebAPI.TUFDifficulties) &&
                        !(m?.DeclaringType?.FullName?.StartsWith("Overlayer.Scripting") ?? false)
                    )
                    &&
                    ((m?.DeclaringType?.Assembly?.GetName()?.Name?.Contains("Overlayer") ?? false))
                ))
                    throw new InvalidOperationException("Cannot Access To Overlayer!");
                if (arr.Any(m => (m?.DeclaringType.FullName.StartsWith("Patch") ?? false) && (m?.DeclaringType?.Assembly?.GetName()?.Name?.Contains("Harmony") ?? false)))
                    throw new InvalidOperationException("Cannot Access To HarmonyLib!");
            }
        }
        [LazyPatch("Patches.PatchGuard_Type5", "System.RuntimeType", "GetMethodImpl", new string[] { "System.String", "System.Reflection.BindingFlags", "System.Reflection.Binder", "System.Reflection.CallingConventions", "System.Type[]", "System.Reflection.ParameterModifier[]" }, ORFlags = BindingFlags.Default)]
        public static class PatchGuard_Type5
        {
            public static void Postfix(ref MethodBase __result)
            {
                if (IgnoreGuard) return;
                if (__result == null) return;
                // tlqkf 이렇게 안하면 얼불 DLC 터짐 ㅋㅋㅋㅋㅋㅋ (왜????)
                if (__result?.DeclaringType?.FullName?.Contains("Getter") ?? false) return;
                if (__result?.Name?.Contains("Getter") ?? false) return;
                var m = __result;
                if ((m?.DeclaringType?.FullName?.StartsWithNotContains("System.Runtime", "Handle") ?? false) || (m?.DeclaringType?.FullName?.StartsWithNotContains("System.Reflection.Runtime", "Handle") ?? false))
                {
                    __result = null;
                    Main.Logger.Log(m.FullDescription());
                    return;
                }
                if ((
                        m?.DeclaringType != typeof(Tags.Adofaigg.GGRatingHolder) &&
                        m?.DeclaringType != typeof(Tags.Adofaigg.Rank) &&
                        m?.DeclaringType != typeof(OverlayerWebAPI.TUFDifficulties) &&
                        !(m?.DeclaringType?.FullName?.StartsWith("Overlayer.Scripting") ?? false)
                    )
                    &&
                    ((m?.DeclaringType?.Assembly?.GetName()?.Name?.Contains("Overlayer") ?? false)))
                {
                    __result = null;
                    Main.Logger.Log(m.FullDescription());
                    return;
                }
                if ((m?.DeclaringType.FullName.StartsWith("Patch") ?? false) && (m?.DeclaringType?.Assembly?.GetName()?.Name?.Contains("Harmony") ?? false))
                {
                    __result = null;
                    Main.Logger.Log(m.FullDescription());
                    return;
                }
            }
        }
        private static bool StartsWithNotContains(this string str, string startsWith, string notContains)
        {
            return str.StartsWith(startsWith) && !str.Contains(notContains);
        }
    }
}
