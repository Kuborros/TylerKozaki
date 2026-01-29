using HarmonyLib;

namespace TylerKozaki.Patches
{
    internal class PatchZLBaseballFlyer
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZLBaseballFlyer), "State_Target", MethodType.Normal)]
        static void PatchZLBaseballFlyerTarget(ref FPPlayer ___targetPlayer)
        {
            if (___targetPlayer == null) return;
            if (___targetPlayer.characterID == TylerKozaki.currentTylerID)
            {
                ___targetPlayer.SetPlayerAnimation("Hide");
            }
        }
    }
}
