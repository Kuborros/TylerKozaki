using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace TylerKozaki.Patches
{
    internal class PatchZLBaseballFlyer
    {
        //Should not affect others too much? We use the other hitbox anyways...
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ZLBaseballFlyer), "State_Target", MethodType.Normal)]
        [HarmonyPatch(typeof(ZLBaseballFlyer), "State_Flying", MethodType.Normal)]
        static IEnumerable<CodeInstruction> ZLBaseballFlyerStateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && codes[i - 1].opcode == OpCodes.Ldfld)
                {
                    if ((string)codes[i].operand == "Rolling")
                        codes[i].operand = "Hide";
                }
            }
            return codes;
        }
    }
}
