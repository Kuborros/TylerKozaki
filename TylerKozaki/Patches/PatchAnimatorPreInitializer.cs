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
            AnimatorInitializationParams tylerInit = new AnimatorInitializationParams();
            tylerInit.animator = TylerKozaki.dataBundle.LoadAsset<Animator>("Tyler Animator Player");

            AnimatorInitializationClipParams[] clipsToInit = {

                new AnimatorInitializationClipParams("Idle"),
                new AnimatorInitializationClipParams("Running"),
                new AnimatorInitializationClipParams("Rolling"),
                new AnimatorInitializationClipParams("Jumping"),
                new AnimatorInitializationClipParams("ClawDive"),
                new AnimatorInitializationClipParams("Crouching"),
                new AnimatorInitializationClipParams("Throw"),
                new AnimatorInitializationClipParams("AirThrow"),
                new AnimatorInitializationClipParams("TailSpin"),
                new AnimatorInitializationClipParams("TailSpin_Loop"),
                new AnimatorInitializationClipParams("Pose1")

            };
            tylerInit.animationClipsToPlay = clipsToInit;

            ___animatorsToInit = ___animatorsToInit.AddToArray(tylerInit);
        }
    }
}
