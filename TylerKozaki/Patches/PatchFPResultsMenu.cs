using FP2Lib.Badge;
using HarmonyLib;

namespace TylerKozaki.Patches
{
    internal class PatchFPResultsMenu
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPResultsMenu), "Update", MethodType.Normal)]
        private static void PatchResultsUpdate(float ___badgeCheckTimer)
        {
            if (___badgeCheckTimer < 61f && !FPStage.currentStage.disableBadgeChecks && FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                if ((___badgeCheckTimer + FPStage.deltaTime) >= 60f)
                {
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.tylerrunner"].id);
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.tylerspeedrunner"].id);
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.tylermaster"].id);
                }
            }
        }
    }
}
