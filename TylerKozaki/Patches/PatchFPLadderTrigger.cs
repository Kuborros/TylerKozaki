using HarmonyLib;

namespace TylerKozaki.Patches
{
    internal class PatchFPLadderTrigger
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPLadderTrigger), "Update", MethodType.Normal)]
        static void PatchFPLadderTriggerUpdate(FPLadderTrigger __instance)
        {
            if (FPSaveManager.character == TylerKozaki.currentTylerID) {
                FPBaseObject fpbaseObject = null;
                while (FPStage.ForEach(FPPlayer.classID, ref fpbaseObject))
                {
                    FPPlayer fpplayer = (FPPlayer)fpbaseObject;
                    if (!fpplayer.IsKOdOrRecovering(false) && fpplayer.barTimer <= 0f)
                    {
                        if (FPCollision.CheckAABB(__instance, __instance.ladderDimensions, fpbaseObject, fpplayer.hbTouch, false, false, false))
                        {
                            if (fpplayer.state == new FPObjectState(PatchFPPlayer.State_Tyler_BoostP2) || fpplayer.state == new FPObjectState(PatchFPPlayer.State_Tyler_TailSpin) || fpplayer.state == new FPObjectState(PatchFPPlayer.State_Tyler_Roll))
                            {
                                if (fpplayer.input.up && fpplayer.position.y < __instance.position.y + __instance.ladderDimensions.top)
                                {
                                    if (!__instance.allowXMovement)
                                    {
                                        fpplayer.position.x = __instance.position.x;
                                    }
                                    fpplayer.state = new FPObjectState(fpplayer.State_LadderClimb);
                                    fpplayer.targetGimmick = __instance;
                                    fpplayer.onGround = false;
                                    fpplayer.allowLadderXMovement = __instance.allowXMovement;
                                    fpplayer.Action_StopSound();
                                    if (FPCamera.stageCamera.targetPlayer == fpplayer)
                                    {
                                        FPCamera.stageCamera.lookOffsetX = 0f;
                                        FPCamera.stageCamera.ySlack = 0f;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
