using HarmonyLib;
using System.Linq;
using System.Security.Policy;
using UnityEngine;

namespace TylerKozaki.Patches
{
    internal class PatchFPHudMaster
    {
        internal static GameObject familyBraceletIcon;
        private static Sprite hudNormalMode;
        private static Sprite hudDarkMode;

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

                familyBraceletIcon = GameObject.Instantiate(TylerKozaki.dataBundle.LoadAsset<GameObject>("Hud Tyler Bike Icon"));
                familyBraceletIcon.transform.parent = __instance.gameObject.transform;

                hudNormalMode = ___pfHudBase.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
                hudDarkMode = TylerKozaki.dataBundle.LoadAssetWithSubAssets<Sprite>("health bar background")[2];
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster),"LateUpdate",MethodType.Normal)]
        static void PatchFPHudMasterLateUpdate(FPHudMaster __instance, ref Vector3 ___hudEnergyBarScale,ref GameObject ___energyBarGraphic, FPPlayer ___targetPlayer,ref SpriteRenderer ___hudBaseSprite)
        {
            if (___targetPlayer.characterID == TylerKozaki.currentTylerID)
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
                //Horrible hack but we do run it quite often. We edit a lot of HUD already, so if other mod tries to as well..
                if (___hudBaseSprite != null)
                {
                    if ((___targetPlayer.energy == 0 || PatchFPPlayer.burnoutState))
                    {
                        ___hudBaseSprite.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hudDarkMode;
                    }
                    else
                    {
                        ___hudBaseSprite.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = hudNormalMode;
                    }
                }

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

            string jumpText = "Jump";
            string basicAttackText = "Attack";
            string specialAttackText = "<c=energy>Kunai Throw</c>";
            string guardText = "-";

            //Boost
            if (player.energy >= 100)
            {
                    guardText = "<c=energy>Umbral Boost</c>";
            }

            //Mid-air
            if (!player.onGround && player.state != new FPObjectState(player.State_LadderClimb))
            {
                jumpText = "Tail Spin";
                specialAttackText = "<c=energy>Multi Kunai</c>";
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

            if (PatchFPPlayer.throwCharge > 25 && player.onGround)
            {
                if (PatchFPPlayer.throwCharge > 90) specialAttackText = "<c=energy><j>Umbral Implosion</j></c>";
                else specialAttackText = "<c=energy>Blade Throw</c>";
            }

            if (PatchFPPlayer.burnoutState)
            {
                specialAttackText = "<j><c=red>Burnout</c></j>";
                guardText = "<j><c=red>Burnout</c></j>";
            }

            if ((player.state == new FPObjectState(PatchFPPlayer.State_Tyler_BoostP1) || player.state == new FPObjectState(PatchFPPlayer.State_Tyler_BoostP2)))
            {
                guardText = "<c=red>Umbral Break</c>";
            }

            if (player.IsKOd(false))
            {
                jumpText = "-";
                basicAttackText = "-";
                specialAttackText = "-";
                guardText = "-";
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
