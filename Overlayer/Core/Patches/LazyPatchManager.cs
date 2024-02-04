using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Overlayer.Core.Patches
{
    public static class LazyPatchManager
    {
        private static readonly Harmony Harmony = new Harmony("Overlayer.Core.Patches.LazyPatchManager");
        private static readonly Dictionary<Type, List<LazyPatch>> Patches = new Dictionary<Type, List<LazyPatch>>();
        private static readonly HashSet<string> PatchedTriggers = new HashSet<string>();
        public static void Load(Assembly ass)
        {
            foreach (var type in ass.GetTypes())
            {
                var lpas = type.GetCustomAttributes<LazyPatchAttribute>();
                if (lpas.Any())
                {
                    if (!Patches.TryGetValue(type, out var list))
                        list = Patches[type] = new List<LazyPatch>();
                    foreach (var patch in lpas)
                        list.Add(new LazyPatch(Harmony, type, patch));
                }
            }
        }
        public static void Unload(Assembly ass)
        {
            foreach (var type in ass.GetTypes())
            {
                if (Patches.TryGetValue(type, out var patches))
                {
                    Unpatch(type);
                    patches.ForEach(lp => LazyPatch.Patches.Remove(lp.attr.Id));
                    Patches.Remove(type);
                }
            }
        }
        public static void UnloadAll()
        {
            Harmony.UnpatchAll(Harmony.Id);
            Patches.Clear();
            PatchedTriggers.Clear();
            LazyPatch.Patches.Clear();
        }
        public static void PatchInternal() => PatchAll(LazyPatch.InternalTrigger);
        public static void PatchAll(string trigger = null)
        {
            if (trigger != null)
            {
                if (PatchedTriggers.Add(trigger))
                    foreach (var patch in Patches.Values.SelectMany(list => list).Where(p => p.attr.Triggers.Contains(trigger)))
                        patch.Patch();
            }
            else
            {
                foreach (var patchType in Patches.Keys)
                    Patch(patchType);
            }
        }
        public static void UnpatchAll(string trigger = null)
        {
            if (trigger != null)
            {
                if (PatchedTriggers.Remove(trigger))
                    foreach (var patch in Patches.Values.SelectMany(list => list).Where(p => p.attr.Triggers.All(t => !PatchedTriggers.Contains(t))))
                        patch.Unpatch();
            }
            else
            {
                foreach (var patchType in Patches.Keys)
                    Unpatch(patchType);
            }
        }
        public static void Patch(Type patchType)
        {
            if (Patches.TryGetValue(patchType, out var patches)) patches.ForEach(lp => lp.Patch());
        }
        public static void Unpatch(Type patchType)
        {
            if (Patches.TryGetValue(patchType, out var patches)) patches.ForEach(lp => lp.Unpatch());
        }
        public static void PatchNested(Type patchType)
        {
            foreach (var nType in patchType.GetNestedTypes((BindingFlags)15420))
                Patch(nType);
        }
        public static void UnpatchNested(Type patchType)
        {
            foreach (var nType in patchType.GetNestedTypes((BindingFlags)15420))
                Unpatch(nType);
        }
    }
}
