using HarmonyLib;
using UnityEngine;

namespace TylerKozaki.Patches
{
    internal class PatchFPHudMaster
    {
        internal static GameObject familyBraceletIcon;
        internal static GameObject overChargeBar;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPHudMaster), "Start", MethodType.Normal)]
        static void PatchFPHudMasterStart(FPHudMaster __instance, ref GameObject ___pfHudBase, ref GameObject ___pfHudEnergyIcon, ref GameObject ___pfHudEnergyBar, ref GameObject ___pfHudLifePetal)
        {
            if (FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                ___pfHudBase = TylerKozaki.dataBundle.LoadAsset<GameObject>("Hud Base Tyler");
                ___pfHudEnergyIcon = TylerKozaki.dataBundle.LoadAsset<GameObject>("Hud Energy Icon Tyler");
                ___pfHudEnergyBar = TylerKozaki.dataBundle.LoadAsset<GameObject>("Hud Energy Bar Tyler");
                ___pfHudLifePetal = TylerKozaki.dataBundle.LoadAsset<GameObject>("Hud Life Petal Tyler");

                overChargeBar = GameObject.Instantiate(TylerKozaki.dataBundle.LoadAsset<GameObject>("Hud Overcharge Bar Tyler"));
                overChargeBar.transform.parent = ___pfHudEnergyBar.transform;
                familyBraceletIcon = GameObject.Instantiate(TylerKozaki.dataBundle.LoadAsset<GameObject>("Hud Tyler Bike Icon"));
                familyBraceletIcon.transform.parent = __instance.gameObject.transform;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster),"LateUpdate",MethodType.Normal)]
        static void PatchFPHudMasterLateUpdate(FPHudMaster __instance, ref Vector3 ___hudEnergyBarScale,ref GameObject ___energyBarGraphic, FPPlayer ___targetPlayer)
        {
            if (FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                ___hudEnergyBarScale.x = Mathf.Min(___targetPlayer.energy * 0.011f, 1.1f);
                ___energyBarGraphic.transform.localScale = ___hudEnergyBarScale;

                if (___targetPlayer.hasSpecialItem)
                {
                    familyBraceletIcon.GetComponent<FPHudDigit>().SetDigitValue(1);
                }
                else
                {
                    familyBraceletIcon.GetComponent<FPHudDigit>().SetDigitValue(0);
                }

                if (PatchFPPlayer.burnoutState)
                {
                    overChargeBar.GetComponent<SpriteRenderer>().color = new Color(255, 0, 0, 255);
                    overChargeBar.transform.position = Vector3.zero;
                    Vector3 overScale = new Vector3(Mathf.Min(PatchFPPlayer.overCharge * 0.011f, 1.1f),1,1);
                    overChargeBar.transform.localScale = overScale;
                }
                else overChargeBar.GetComponent<SpriteRenderer>().color = new Color(255, 0, 0, 0);

                if (__instance.state == 1)
                {
                    familyBraceletIcon.transform.position = new Vector3(237.5f, -33.5f, 0f);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster), "GuideUpdate", MethodType.Normal)]
        static void PatchGuideUpdate(FPPlayer player, FPHudMaster __instance)
        {
            if (player == null || player.characterID != TylerKozaki.currentTylerID)
            {
                return;
            }

            string jumpText = "Eclipse Strike";
            string basicAttackText = "Attack";
            string specialAttackText = "<c=energy>Umbral Boost</c>";
            string guardText = "Guard";

            if (player.IsKOd(false))
            {
                jumpText = "-";
                basicAttackText = "-";
                specialAttackText = "-";
                guardText = "-";
            }

            //Boost
            if (player.energy > 0)
            {
                if (!PatchFPPlayer.burnoutState)
                {
                        specialAttackText = "<c=energy>Umbral Boost</c>";
                }
                else
                {
                    specialAttackText = "<j><c=red>Burnout</c></j>";
                }
            }

            //Mid-air
            if (!player.onGround && player.state != new FPObjectState(player.State_LadderClimb))
            {
                jumpText = "Tail Spin";
                if (player.input.up) basicAttackText = "Eclipse Fang";
                else if (player.input.down) basicAttackText = "Claw Dive";
                else basicAttackText = "Kick";
            }

            //On the ground, excluding funky states
            if (player.onGround && player.state != new FPObjectState(player.State_LadderClimb) && player.state != new FPObjectState(player.State_Ball) && player.state != new FPObjectState(player.State_Ball_Physics) && player.state != new FPObjectState(player.State_Ball_Vulnerable))
            {
                if (player.input.down)
                {
                    basicAttackText = "Tail Swipe";
                }
            }

            //Blink mode, no attacks allowed
            if (PatchFPPlayer.blinkState)
            {
                basicAttackText = "-";
                specialAttackText = "-";
            }

            if ((player.state == new FPObjectState(PatchFPPlayer.State_Tyler_BoostP1) || player.state == new FPObjectState(PatchFPPlayer.State_Tyler_BoostP2)))
            {
                specialAttackText = "<c=red>Umbral Break</c>";
            }

            if (player.displayMoveJump != string.Empty)
            {
                jumpText = player.displayMoveJump;
            }
            if (player.displayMoveAttack != string.Empty)
            {
                basicAttackText = player.displayMoveAttack;
            }
            if (player.displayMoveSpecial != string.Empty)
            {
                specialAttackText = player.displayMoveSpecial;
            }
            if (player.displayMoveGuard != string.Empty)
            {
                guardText = player.displayMoveGuard;
            }
            __instance.hudGuide.text = string.Concat(new string[]
            {
            jumpText,
            "\n",
            basicAttackText,
            "\n",
            specialAttackText,
            "\n",
            guardText,
            "\n "
            });
        }
    }
}
