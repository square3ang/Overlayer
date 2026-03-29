using HarmonyLib;
using Overlayer.Core.Patches;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Overlayer.Patches;

public class HitFixPatch : SafeConditionalPatch {
    public HitFixPatch() : base("HitFixPatch") { }

    protected override bool ShouldApply() => Main.Settings.ShowTrueAutoJudgment;

    protected override MethodBase GetTargetMethod() =>
        SafePatch.GetMethodSafe("scrController", "Hit");

    protected override HarmonyMethod Transpiler() =>
        new(typeof(HitFixPatch).GetMethod(nameof(TranspilerImpl), BindingFlags.Static | BindingFlags.NonPublic));

    private static IEnumerable<CodeInstruction> TranspilerImpl(IEnumerable<CodeInstruction> instructions) {
        var list = new List<CodeInstruction>(instructions);
        for(int i = 0; i < list.Count; i++) {
            if(list[i].opcode == OpCodes.Call && list[i].operand is MethodInfo method && method.Name == "get_auto") {
                list[i].opcode = OpCodes.Ldc_I4_0;
                list[i].operand = null;
            }
        }
        return list;
    }
}