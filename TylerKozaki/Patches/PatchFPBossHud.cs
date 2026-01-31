using FP2Lib.Badge;
using HarmonyLib;

namespace TylerKozaki.Patches
{
    internal class PatchFPBossHud
    {
        private static bool bossKillRan = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPBossHud), "Start", MethodType.Normal)]
        static void PatchFPBossHudStart()
        {
            bossKillRan = false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPBossHud), "LateUpdate", MethodType.Normal)]
        static void PatchFPBossHudLateUpdatePre(FPBossHud __instance)
        {
            if (__instance.bossDeathActionsExecuted && PatchFPBaseEnemy.lastHit != BossLastHitType.Generic && !bossKillRan)
            {
                switch(PatchFPBaseEnemy.lastHit)
                {
                    case BossLastHitType.Boost:
                        BadgeHandler.UnlockBadge("kubo.tylerboostkill");
                        break;
                    case BossLastHitType.TailSpin:
                        BadgeHandler.UnlockBadge("kubo.tylertailkill");
                        break;
                    case BossLastHitType.Kunai:
                        BadgeHandler.UnlockBadge("kubo.tylershadowkill");
                        break;
                }
                PatchFPBaseEnemy.lastHit = BossLastHitType.Generic;
                bossKillRan = true;
            }
        }
    }
}
