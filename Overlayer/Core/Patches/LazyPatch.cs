using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Overlayer.Core.Patches
{
    public class LazyPatch
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
                OverlayerDebug.Log($"ID:{attr.Id}, {attr.TargetType}.{attr.TargetMethod} Could Not Be Resolved!");
            Patches.Add(attr.Id, this);
        }
        public void Patch()
        {
            if (Patched || target == null || patch != null) return;
            var pre_hm = prefix != null ? new HarmonyMethod(prefix) : null;
            var post_hm = postfix != null ? new HarmonyMethod(postfix) : null;
            var trans_hm = transpiler != null ? new HarmonyMethod(transpiler) : null;
            var final_hm = finalizer != null ? new HarmonyMethod(finalizer) : null;
            patch = harmony.Patch(target, pre_hm, post_hm, trans_hm, final_hm);
            OverlayerDebug.Log($"ID:{attr.Id} Patched!");
            Patched = true;
        }
        public void Unpatch()
        {
            if (!Patched || target == null || patch == null) return;
            harmony.Unpatch(target, patch);
            OverlayerDebug.Log($"ID:{attr.Id} Unpatched!");
            patch = null;
            Patched = false;
        }
    }
}
