using HarmonyLib;
using UnityEngine;

namespace TylerKozaki.Patches
{
    internal class PatchAnimatorPreInitializer
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AnimatorPreInitializer), "Start", MethodType.Normal)]
        static void PatchAnimatorPreInit(ref AnimatorInitializationParams[] ___animatorsToInit)
        {
            AnimatorInitializationParams lightingInit = new AnimatorInitializationParams();
            lightingInit.animator = TylerKozaki.dataBundle.LoadAsset<Animator>("Tyler Animator Player");

            AnimatorInitializationClipParams[] clipsToInit = {

                new AnimatorInitializationClipParams("Idle"),
                new AnimatorInitializationClipParams("Running"),
                new AnimatorInitializationClipParams("Rolling"),
                new AnimatorInitializationClipParams("Jumping"),
                new AnimatorInitializationClipParams("Pose1")

            };
            lightingInit.animationClipsToPlay = clipsToInit;

            ___animatorsToInit = ___animatorsToInit.AddToArray(lightingInit);
        }
    }
}
