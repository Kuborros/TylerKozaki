using FP2Lib.Player;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TylerKozaki.Patches
{
    internal class PatchFuelIcons
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(FPHudDigit), "SetDigitValue", MethodType.Normal)]
        static void PatchSetDigitValue(ref Sprite[] ___digitFrames, ref FPHudDigit __instance)
        {
            //Do we even have frames and if we are playing as Tyler
            if (___digitFrames != null && ___digitFrames != null && SceneManager.GetActiveScene().name != "MainMenu")
            {
                if (___digitFrames.Length > 40 && FPSaveManager.character == TylerKozaki.currentTylerID)
                {
                    //Does frame one exist (it sometimes doesnt just to spite you)
                    if (___digitFrames[1] != null && ___digitFrames[12] != null)
                    {
                        //Check if we are in Item page, and the sprite is still Carol's
                        if (___digitFrames[12].name == "powerup_start_carol")
                        {
                            //If so replace it
                            ___digitFrames[12] = PlayerHandler.currentCharacter.itemFuel;
                            if (__instance.digitValue == 12) __instance.SetDigitValue(12);
                        }
                    }
                }
            }
        }

        //Above works everywhere but main menu. So we need to fix that.
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(MenuFile), "Start", MethodType.Normal)]
        private static void FileMenu(ref MenuFilePanel[] ___files)
        {
            foreach (MenuFilePanel file in ___files)
            {
                if (file != null)
                {
                    //The most evil and fucked up hack. The character icon in the menu! It has the same digit as character ID!
                    //We can use that!
                    if (file.pfCharacterPortrait.digitValue == (int)TylerKozaki.currentTylerID)
                    {
                        foreach (FPHudDigit digit in file.itemIcon)
                        {
                            if (digit != null)
                            {
                                //Does frame one exist (it sometimes doesnt just to spite you)
                                if (digit.digitFrames[1] != null && digit.digitFrames[12] != null)
                                {
                                    //Check if we are in Item page, and the sprite is still Carol's
                                    if (digit.digitFrames[12].name == "powerup_start_carol")
                                    {
                                        //If so replace it
                                        digit.digitFrames[12] = PlayerHandler.GetPlayableCharaByRuntimeId((int)TylerKozaki.currentTylerID).itemFuel;
                                        if (digit.digitValue == 12) digit.SetDigitValue(12);
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
