using HarmonyLib;

namespace TylerKozaki.Patches
{
    internal class PatchItemFuel
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemFuel), "State_Collected", MethodType.Normal)]
        static bool PatchItemFuelCollected()
        {
            //Disables respawning of the item.
            if (FPSaveManager.character == TylerKozaki.currentTylerID) return false;
            else return true;
        }
    }
}
