using HarmonyLib;
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

            string text = "Jump";
            string text2 = "Single Shot";
            string text3 = "<c=energy>Wing Special</c>";
            string text4 = "Guard";

            if (player.IsKOd(false))
            {
                text = "-";
                text2 = "-";
                text3 = "-";
                text4 = "-";
            }

            //Mid-air
            if (!player.onGround && player.state != new FPObjectState(player.State_LadderClimb))
            {
                text = "Double Jump";
                if (player.state != new FPObjectState(player.State_Ball) && player.state != new FPObjectState(player.State_Ball_Physics) && player.state != new FPObjectState(player.State_Ball_Vulnerable))
                {
                    if (player.input.left || player.input.right)
                    {
                        text3 = "<c=energy>Wing Smash</c>";
                    }
                    else if (player.input.up || player.input.down)
                    {
                        text3 = "<c=energy>Gravity Boots</c>";
                    }
                    else
                        text3 = "<c=energy>-</c>";
                }
                if (!player.input.attackHold)
                {
                    text2 = "Single Shot";
                }
                else
                {
                    text2 = "<c=energy>(Hold) Charge Shot</c>";
                }

            }

            //On the ground, excluding funky states
            if (player.state != new FPObjectState(player.State_LadderClimb) && player.state != new FPObjectState(player.State_Ball) && player.state != new FPObjectState(player.State_Ball_Physics) && player.state != new FPObjectState(player.State_Ball_Vulnerable))
            {
                if (player.input.down)
                {
                    text2 = "Crouch shot";
                }
                else
                {
                    if (!player.input.attackHold)
                    {
                        text2 = "Single Shot";
                    }
                    else
                    {
                        text2 = "<c=energy>(Hold) Charge Shot</c>";
                    }
                }
                if (player.input.up && !player.input.down)
                {
                    text3 = "<c=energy>Gravity Boots</c>";
                }
                if (player.onGround)
                {
                    text3 = "<c=energy>-</c>";
                }
            }

            if (player.displayMoveJump != string.Empty)
            {
                text = player.displayMoveJump;
            }
            if (player.displayMoveAttack != string.Empty)
            {
                text2 = player.displayMoveAttack;
            }
            if (player.displayMoveSpecial != string.Empty)
            {
                text3 = player.displayMoveSpecial;
            }
            if (player.displayMoveGuard != string.Empty)
            {
                text4 = player.displayMoveGuard;
            }
            __instance.hudGuide.text = string.Concat(new string[]
            {
            text,
            "\n",
            text2,
            "\n",
            text3,
            "\n",
            text4,
            "\n "
            });
        }
    }
}
