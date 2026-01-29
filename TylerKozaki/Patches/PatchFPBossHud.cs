using FP2Lib.Badge;
using HarmonyLib;

namespace TylerKozaki.Patches
{
    internal class PatchFPBossHud
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPBossHud), "LateUpdate", MethodType.Normal)]
        static void PatchFPBossHudLateUpdatePre(FPBossHud __instance, out bool __state)
        {
            __state = __instance.bossDeathActionsExecuted;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPBossHud), "LateUpdate", MethodType.Normal)]
        static void PatchFPBossHudLateUpdate(FPBossHud __instance,bool __state, FPBaseEnemy[] ___weakpointCheck)
        {
            if (__instance.targetBoss == null || __state || !(__instance.targetBoss.health <= 0f))
            {
                return;
            }
            if (FPStage.currentStage.GetPlayerInstance_FPPlayer() != null)
            {
                //Engage horrible hack
                //To not add any extra variables we just use ones meant for other characters
                //Original code makes sure the flag only triggers when playing as right character, so we can do that without major risks. Hopefully.
                if (FPStage.currentStage.GetPlayerInstance_FPPlayer().characterID == TylerKozaki.currentTylerID)
                {
                    //Lilac = Umbral Boost
                    if (__instance.targetBoss.badgeLilac) BadgeHandler.UnlockBadge("kubo.tylerboostkill");
                    if (___weakpointCheck.Length <= 0)
                    {
                        return;
                    }
                    for (int i = 0; i < ___weakpointCheck.Length; i++)
                    {
                        if (___weakpointCheck[i] != null)
                        {
                            if (___weakpointCheck[i].badgeLilac) BadgeHandler.UnlockBadge("kubo.tylerboostkill");
                        }
                    }
                    //Carol = Tail
                    if (__instance.targetBoss.badgeCarol) BadgeHandler.UnlockBadge("kubo.tylertailkill");
                    if (___weakpointCheck.Length <= 0)
                    {
                        return;
                    }
                    for (int i = 0; i < ___weakpointCheck.Length; i++)
                    {
                        if (___weakpointCheck[i] != null)
                        {
                            if (___weakpointCheck[i].badgeCarol) BadgeHandler.UnlockBadge("kubo.tylertailkill");
                        }
                    }
                    //Milla = Shadow
                    if (__instance.targetBoss.badgeMilla) BadgeHandler.UnlockBadge("kubo.tylershadowkill");
                    if (___weakpointCheck.Length <= 0)
                    {
                        return;
                    }
                    for (int i = 0; i < ___weakpointCheck.Length; i++)
                    {
                        if (___weakpointCheck[i] != null)
                        {
                            if (___weakpointCheck[i].badgeMilla) BadgeHandler.UnlockBadge("kubo.tylershadowkill");
                        }
                    }
                }
            }
        }
    }
}
