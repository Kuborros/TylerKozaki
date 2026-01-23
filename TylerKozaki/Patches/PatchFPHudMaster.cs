using HarmonyLib;
using System.ComponentModel.Design;
using UnityEngine;

namespace TylerKozaki.Patches
{
    internal class PatchFPHudMaster
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPHudMaster), "Start", MethodType.Normal)]
        static void PatchFPHudMasterStart(ref GameObject ___pfHudBase, ref GameObject ___pfHudEnergyIcon, ref GameObject ___pfHudEnergyBar)
        {
            if (FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                ___pfHudBase = TylerKozaki.dataBundle.LoadAsset<GameObject>("Hud Base Tyler");
                ___pfHudEnergyIcon = TylerKozaki.dataBundle.LoadAsset<GameObject>("Hud Energy Icon Tyler");
                ___pfHudEnergyBar = TylerKozaki.dataBundle.LoadAsset<GameObject>("Hud Energy Bar Tyler");
            }
        }
        //Not needed for Tyler hopefully
        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster), "Start", MethodType.Normal)]
        static void PatchFPHudMasterStartPost(FPPlayer ___targetPlayer, ref FPHudDigit[] ___hudLifePetals, ref FPHudDigit[] ___hudShields, ref FPHudDigit[] ___hudEnergy)
        {
            if (___targetPlayer.characterID == TylerKozaki.currentTylerID)
            {
                Vector3 posEnergy = ___hudEnergy[0].transform.position;
                posEnergy.x = 6;
                ___hudEnergy[0].transform.position = posEnergy;

                //Can be 7 without the item if someone is using some other mod
                if (___targetPlayer.IsPowerupActive(FPPowerup.MAX_LIFE_UP) || ___targetPlayer.healthMax == 7)
                {
                    Vector3 pos = ___hudLifePetals[0].transform.position;
                    pos.x += 2;
                    ___hudLifePetals[0].transform.position = pos;

                    for (int i = 1; i < 7; i++)
                    {
                        pos = ___hudLifePetals[i].transform.position;
                        pos.x -= 2*i;
                        ___hudLifePetals[i].transform.position = pos;
                    }

                    //Shields need moving too
                    pos = ___hudShields[0].transform.position;
                    pos.x += 1;
                    ___hudShields[0].transform.position = pos;

                    for (int i = 1; i < 14; i++)
                    {
                        pos = ___hudShields[i].transform.position;
                        pos.x -= i;
                        ___hudShields[i].transform.position = pos;
                    }

                }
            }
        }
        */

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster), "GuideUpdate", MethodType.Normal)]
        static void PatchGuideUpdate(FPPlayer player, FPHudMaster __instance)
        {
            if (player == null || player.characterID != TylerKozaki.currentTylerID)
            {
                return;
            }

            string jumpText = "Jump";
            string basicAttackText = "Attack";
            string specialAttackText = "<c=black>Kunai Throw</c>";
            string guardText = "Guard";

            if (player.IsKOd(false))
            {
                jumpText = "-";
                basicAttackText = "-";
                specialAttackText = "-";
                guardText = "-";
            }

            //Kunai throw and it's variants.
            if (player.energy > 0)
            {
                if (!PatchFPPlayer.burnoutState)
                {
                    if (PatchFPPlayer.throwCharge < 12.5)
                    {
                        specialAttackText = "<c=black>Kunai Throw</c>";
                    }
                    else if (PatchFPPlayer.throwCharge < 25)
                    {
                        specialAttackText = "<c=black>Umbral Blade</c>";
                    }
                    else if (PatchFPPlayer.throwCharge >= 25)
                    {
                        specialAttackText = "<c=black>Eclipse Orb</c>";
                    }
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
            }

            //On the ground, excluding funky states
            if (player.state != new FPObjectState(player.State_LadderClimb) && player.state != new FPObjectState(player.State_Ball) && player.state != new FPObjectState(player.State_Ball_Physics) && player.state != new FPObjectState(player.State_Ball_Vulnerable))
            {
                if (player.input.down)
                {
                    basicAttackText = "Crouch Attack";
                }
            }

            //Blink mode, no attacks allowed
            if (PatchFPPlayer.blinkState)
            {
                basicAttackText = "-";
                specialAttackText = "-";
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
