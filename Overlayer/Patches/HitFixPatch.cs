using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Overlayer.Core.Patches;

namespace Overlayer.Patches;

public class HitFixPatch : SafeConditionalPatch {
    public HitFixPatch() : base(nameof(HitFixPatch)) { }

    protected override bool ShouldApply() {
        return Main.Settings.ShowTrueAutoJudgment;
    }

    protected override MethodBase GetTargetMethod() {
        return SafePatch.GetMethodSafe("scrController", "Hit");
    }

    protected override HarmonyMethod Transpiler() {
        return new HarmonyMethod(typeof(HitFixPatch).GetMethod(nameof(TranspilerImpl),
            BindingFlags.Static | BindingFlags.NonPublic));
    }

    private static IEnumerable<CodeInstruction> TranspilerImpl(IEnumerable<CodeInstruction> instructions) {
        var list = new List<CodeInstruction>(instructions);
        for (var i = 0; i < list.Count; i++)
            if (list[i].opcode == OpCodes.Call && list[i].operand is MethodInfo method && method.Name == "get_auto") {
                list[i].opcode = OpCodes.Ldc_I4_0;
                list[i].operand = null;
            }

        return list;
    }
}