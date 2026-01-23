using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TylerKozaki.Patches
{
    internal class PatchFPEventSequence
    {
        //Tyler Anywhere System™️ v2
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPEventSequence), "Start", MethodType.Normal)]
        static void PatchStateDefault(FPEventSequence __instance)
        {
            if (__instance != null && FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                if (__instance.transform.parent != null)
                {
                    Transform cutsceneLilac = __instance.transform.parent.gameObject.transform.Find("Cutscene_Lilac");
                    if (cutsceneLilac != null)
                    {
                        if (cutsceneLilac.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Tyler Animator Player")
                        {
                            cutsceneLilac.gameObject.GetComponent<Animator>().runtimeAnimatorController = TylerKozaki.dataBundle.LoadAsset<RuntimeAnimatorController>("Tyler Animator Player");
                            cutsceneLilac.Find("tail").gameObject.SetActive(false);
                        }
                    }
                }

                //Post-Merga fight special case
                if (__instance.transform.parent != null && FPStage.stageNameString == "Merga")
                {
                    Transform eventSequence = __instance.transform.parent.gameObject.transform;
                    if (eventSequence != null)
                    {
                        Transform cutsceneLilac = eventSequence.parent.gameObject.transform.Find("Cutscene_Lilac");
                        if (cutsceneLilac != null)
                        {
                            if (cutsceneLilac.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Tyler Animator Player")
                            {
                                cutsceneLilac.gameObject.GetComponent<Animator>().runtimeAnimatorController = TylerKozaki.dataBundle.LoadAsset<RuntimeAnimatorController>("Tyler Animator Player");
                                cutsceneLilac.Find("tail").gameObject.SetActive(false);
                            }
                        }
                    }
                }

                //Snowfields magic
                if (__instance.transform.Find("Cutscene_Lilac_Classic") != null)
                {
                    Transform cutsceneLilacClassic = __instance.transform.Find("Cutscene_Lilac_Classic");
                    if (cutsceneLilacClassic != null)
                    {
                        if (cutsceneLilacClassic.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Tyler Animator Player")
                        {
                            cutsceneLilacClassic.gameObject.GetComponent<Animator>().runtimeAnimatorController = TylerKozaki.dataBundle.LoadAsset<RuntimeAnimatorController>("Tyler Animator Player");
                            cutsceneLilacClassic.Find("tail").gameObject.SetActive(false);
                        }
                    }
                }
            }
        }


        //Special ending cutscene skipping code.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPEventSequence), "State_Event", MethodType.Normal)]
        static void PatchStateEvent(FPEventSequence __instance)
        {
            if (__instance != null && FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                if (__instance.transform.parent != null && (FPStage.stageNameString == "Merga"))
                {
                    Transform eventSequence = __instance.transform.parent.gameObject.transform;
                    if (eventSequence != null)
                    {
                        Transform cutsceneLilac = eventSequence.parent.gameObject.transform.Find("Cutscene_Lilac");
                        if (cutsceneLilac != null)
                        {
                            __instance.Action_SkipScene();
                        }
                    }
                }
            }
            if (__instance != null)
            {
                if (__instance.name == "Event Activator (Classic)" && __instance.transform.parent != null)
                {
                    if (__instance.transform.parent.gameObject.name == "Ending")
                        __instance.Action_SkipScene();
                }
            }
        }
    }
}
