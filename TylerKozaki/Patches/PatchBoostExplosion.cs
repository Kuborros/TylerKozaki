using HarmonyLib;
using UnityEngine;

namespace TylerKozaki.Patches
{
    internal class PatchBoostExplosion
    {

        static RuntimeAnimatorController baseAnim;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BoostExplosion), "Start", MethodType.Normal)]
        static void PatchBoostExplosionStart(BoostExplosion __instance)
        {
            if (baseAnim == null && FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                baseAnim = __instance.animator.runtimeAnimatorController;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BoostExplosion), "ObjectCreated", MethodType.Normal)]
        static void PatchBoostExplosionCreated(BoostExplosion __instance)
        {
            if (FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                __instance.animator.runtimeAnimatorController = baseAnim;
            }
        }
    }
}
