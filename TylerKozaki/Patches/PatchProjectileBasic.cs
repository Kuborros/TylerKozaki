using HarmonyLib;

namespace TylerKozaki.Patches
{
    internal class PatchProjectileBasic
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProjectileBasic),"Update",MethodType.Normal)]
        static void PatchProjectileBasicUpdate(ProjectileBasic __instance)
        {
            if (__instance.animator != null)
            {
                if (__instance.animator.GetCurrentAnimatorStateInfo(0).IsName("UmbralBladeThrow"))
                {
                    if (__instance.velocity.x > 0)
                    {
                        __instance.velocity.x -= 0.05f;
                    }
                    else if (__instance.velocity.x < 0)
                    {
                        __instance.velocity.x += 0.05f;
                    }
                }
            }
        }
    }
}
