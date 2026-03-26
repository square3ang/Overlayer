using HarmonyLib;
using Overlayer.Core.Patches;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Overlayer.Patches;

public static class HitFixPatch {

    [LazyPatch("Patches.HitFixPatch.ChangeAddHit", "scrController", "Hit")]
    public static class ChangeAddHit {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var list = new List<CodeInstruction>(instructions);

            for(int i = 0; i < list.Count; i++) {
                if(list[i].opcode == OpCodes.Call && list[i].operand is MethodInfo method && method.Name == "get_auto") {
                    if(Main.Settings.UseShowTrueAutoJudgment) {
                        list[i].opcode = OpCodes.Ldc_I4_0;
                        list[i].operand = null;
                    }
                }
            }

            return list;
        }
    }
}
