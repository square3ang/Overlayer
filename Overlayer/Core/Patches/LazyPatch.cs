using HarmonyLib;
using Overlayer.Patches;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Overlayer.Core.Patches
{
    internal class LazyPatch
    {
        public const string InternalTrigger = "INTERNAL";
        public static readonly Dictionary<string, LazyPatch> Patches = new Dictionary<string, LazyPatch>();
        public Type patchType;
        public Harmony harmony;
        public MethodInfo prefix;
        public MethodInfo postfix;
        public MethodInfo transpiler;
        public MethodInfo finalizer;
        public MethodBase target;
        public MethodInfo patch;
        public LazyPatchAttribute attr;
        public bool Patched { get; private set; }
        public bool Locked { get; set; }
        public int IgnorePatchCount { get; set; }
        public int IgnoreUnpatchCount { get; set; }
        public LazyPatch(Harmony harmony, Type patchType, LazyPatchAttribute attr)
        {
            this.patchType = patchType!;
            this.attr = attr!;
            this.harmony = harmony!;
            prefix = patchType.GetMethod("Prefix", (BindingFlags)15420);
            postfix = patchType.GetMethod("Postfix", (BindingFlags)15420);
            transpiler = patchType.GetMethod("Transpiler", (BindingFlags)15420);
            finalizer = patchType.GetMethod("Finalizer", (BindingFlags)15420);
            target = attr.Resolve();
            if (attr.IsCompatible && target == null)
                Main.Logger.Log($"ID:{attr.Id}, {attr.TargetType}.{attr.TargetMethod} Could Not Be Resolved!");
            Patches.Add(attr.Id, this);
        }
        public void Patch(bool force = false)
        {
            if (Patched || IgnorePatchCount-- > 0 || target == null || patch != null) return;
            if (!force && Locked)
            {
                Main.Logger.Log($"ID:{attr.Id} Is Locked! Cannot Be Patched!");
                return;
            }
            if (force) Locked = false;
        }
        public void Unpatch(bool force = false)
        {
            if (!Patched || IgnoreUnpatchCount-- > 0 || target == null || patch == null) return;
            if (!force && Locked)
            {
                Main.Logger.Log($"ID:{attr.Id} Is Locked! Cannot Be Unpatched!");
                return;
            }
            if (force) Locked = false;
        }
    }
}
