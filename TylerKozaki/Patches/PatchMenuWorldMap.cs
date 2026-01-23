using FP2Lib.Badge;
using HarmonyLib;

namespace TylerKozaki.Patches
{
    internal class PatchMenuWorldMap
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuWorldMap), "State_Default", MethodType.Normal)]
        private static void PatchStateDefault(bool ___cutsceneCheck, float ___badgeCheckTimer)
        {
            if (___cutsceneCheck && ___badgeCheckTimer > 0f && ___badgeCheckTimer < 26f && FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                if ((___badgeCheckTimer + FPStage.deltaTime) >= 25f)
                {
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.tylermaster"].id);
                }
            }
        }
    }
}
