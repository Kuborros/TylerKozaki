using HarmonyLib;

namespace TylerKozaki.Patches
{

    internal enum BossLastHitType
    {
        Generic,
        Boost,
        TailSpin,
        Kunai
    }

    internal class PatchFPBaseEnemy
    {
        //Horrible hack v2
        internal static BossLastHitType lastHit = BossLastHitType.Generic;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPBaseEnemy), "DamageCheck_FPPlayer", MethodType.Normal)]
        static void PatchBaseEnemyDamageCheckFPPlayer(FPBaseObject objectRef, ref FPBaseEnemy __instance)
        {
            if (FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                if (FPStage.ConfirmClassWithPoolTypeID(typeof(FPPlayer), FPPlayer.classID))
                {
                    while (FPStage.ForEach(FPPlayer.classID, ref objectRef))
                    {
                        FPPlayer fPPlayer = (FPPlayer)objectRef;
                        if (!(fPPlayer.faction != __instance.faction))
                        {
                            continue;
                        }
                        if (FPCollision.CheckOOBB(objectRef, fPPlayer.hbAttack, __instance, __instance.hbWeakpoint))
                        {
                            if (fPPlayer.state == new FPObjectState(PatchFPPlayer.State_Tyler_BoostP1) || fPPlayer.state == new FPObjectState(PatchFPPlayer.State_Tyler_BoostP2))
                            {
                                __instance.badgeLilac = true;
                                lastHit = BossLastHitType.Boost;
                            }
                            else if (fPPlayer.state == new FPObjectState(PatchFPPlayer.State_Tyler_TailSpin))
                            {
                                __instance.badgeCarol = true;
                                lastHit = BossLastHitType.TailSpin;
                            }
                            else lastHit = BossLastHitType.Generic;
                        }
                    }
                }
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPBaseEnemy), "DamageCheck_ProjectileBasic", MethodType.Normal)]
        static void PatchBaseEnemyDamageCheckProjectileBasic(FPBaseObject objectRef, ref FPBaseEnemy __instance)
        {
            if (FPSaveManager.character == TylerKozaki.currentTylerID) {
                if (FPStage.ConfirmClassWithPoolTypeID(typeof(ProjectileBasic), ProjectileBasic.classID))
                {
                    while (FPStage.ForEach(ProjectileBasic.classID, ref objectRef))
                    {
                        ProjectileBasic projectileBasic = (ProjectileBasic)objectRef;
                        if (!(projectileBasic.faction != __instance.faction) || !(projectileBasic.parentObject != __instance) || !(projectileBasic.timeBeforeCollisions <= 0f))
                        {
                            continue;
                        }
                        if (FPCollision.CheckOOBB(__instance, __instance.hbWeakpoint, objectRef, projectileBasic.hbTouch))
                        {
                            FPPlayer fPPlayer = projectileBasic.parentObject as FPPlayer;
                            if (fPPlayer != null)
                            {
                                if (projectileBasic.animator != null)
                                {
                                    if (projectileBasic.animator.GetCurrentAnimatorStateInfo(0).IsName("UmbralBladeThrow")
                                        || projectileBasic.animator.GetCurrentAnimatorStateInfo(0).IsName("KunaiThrow")
                                        || projectileBasic.animator.GetCurrentAnimatorStateInfo(0).IsName("BombThrow"))
                                    {
                                        __instance.badgeMilla = true;
                                        lastHit = BossLastHitType.Kunai;
                                    }
                                    else lastHit = BossLastHitType.Generic;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

