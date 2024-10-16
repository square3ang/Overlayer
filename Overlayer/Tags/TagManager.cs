﻿using Overlayer.Core.Patches;
using Overlayer.Patches;
using Overlayer.Tags.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Overlayer.Tags
{
    public static class TagManager
    {
        public static bool Initialized { get; private set; }
        public static int Count => tags.Count;
        internal static Dictionary<string, OverlayerTag> tags;
        public static IEnumerable<OverlayerTag> All => tags.Values;
        public static IEnumerable<OverlayerTag> NP => tags.Values.Where(ot => ot.NotPlaying);

        internal static string testerValue;

        private static OverlayerTag testerTag = new("INTERNAL_TESTER_TAG_1234512345", () => testerValue, true);
        public static void Load(Assembly ass)
        {
            foreach (var t in ass.GetExportedTypes())
                Load(t);
        }
        public static void Load(Type type)
        {
            PatchGuard.Ignore(() =>
            {
                PatchGuard.ForceIgnore();
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    var attr = method.GetCustomAttribute<TagAttribute>();
                    if (attr == null) continue;
                    SetTag(new OverlayerTag(method, attr));
                }
                PatchGuard.ForceIgnore();
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var attr = field.GetCustomAttribute<TagAttribute>();
                    if (attr == null) continue;
                    SetTag(new OverlayerTag(field, attr));
                }
                PatchGuard.ForceIgnore();
                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    var attr = prop.GetCustomAttribute<TagAttribute>();
                    if (attr == null) continue;
                    SetTag(new OverlayerTag(prop, attr));
                }
            });
        }
        public static void Unload(Assembly ass)
        {
            foreach (var t in ass.GetExportedTypes())
                Unload(t);
        }
        public static void Unload(Type type)
        {
            foreach (var key in tags.Where(kvp => kvp.Value.DeclaringType == type).Select(kvp => kvp.Key).ToList())
                tags.Remove(key);
        }
        public static OverlayerTag GetTag(string name)
        {
            if (name.StartsWith("INTERNAL_TESTER_TAG_1234512345"))
            {
                return testerTag;
            }
            return tags.TryGetValue(name, out var ot) ? ot : null;
        }
        public static void SetTag(OverlayerTag tag)
        {
            tags[tag.Name] = tag;
        }
        public static void RemoveTag(string name)
        {
            tags.Remove(name);
        }
        public static void UpdatePatch()
        {
            foreach (var tag in All)
                if (!tag.Referenced) LazyPatchManager.UnpatchAll(tag.Name);
                else LazyPatchManager.PatchAll(tag.Name);
        }
        public static bool HasReference(Type declaringType)
        {
            return tags.Values.Any(tag => tag.Referenced && tag.DeclaringType == declaringType);
        }
        public static void Initialize()
        {
            if (Initialized) return;
            tags = new Dictionary<string, OverlayerTag>();
            Initialized = true;
        }
        public static void Release()
        {
            if (!Initialized) return;
            tags = null;
            Initialized = false;
        }
    }
}
