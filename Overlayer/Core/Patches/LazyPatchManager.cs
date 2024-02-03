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
        private static readonly Dictionary<Type, LazyPatch> Patches = new Dictionary<Type, LazyPatch>();
        private static readonly HashSet<string> PatchedTriggers = new HashSet<string>();
        public static void Load(Assembly ass)
        {
            foreach (var type in ass.GetTypes())
            {
                var lpa = type.GetCustomAttribute<LazyPatchAttribute>();
                if (lpa != null) Patches.Add(type, new LazyPatch(Harmony, type, lpa));
            }
        }
        public static void Unload(Assembly ass)
        {
            foreach (var type in ass.GetTypes())
            {
                if (Patches.TryGetValue(type, out var patch))
                {
                    Unpatch(type);
                    LazyPatch.Patches.Remove(patch.attr.Id);
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
        public static void PatchAll(string trigger = null)
        {
            if (trigger != null)
            {
                if (PatchedTriggers.Add(trigger))
                    foreach (var patch in Patches.Values.Where(p => p.attr.Triggers.Contains(trigger)))
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
                    foreach (var patch in Patches.Values.Where(p => p.attr.Triggers.All(t => !PatchedTriggers.Contains(t))))
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
            if (Patches.TryGetValue(patchType, out var patch)) patch.Patch();
        }
        public static void Unpatch(Type patchType)
        {
            if (Patches.TryGetValue(patchType, out var patch)) patch.Unpatch();
        }
    }
}
