using HarmonyLib;
using UnityEngine;

namespace TylerKozaki.Patches
{
    internal class PatchBFSyntaxHunt
    {
        [HarmonyPostfix]
        [HarmonyPatch]
        static void PatchBSSyntaxHuntStart(ref GameObject ___playerBody)
        {
            if (FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                GameObject lilacDed = ___playerBody.transform.GetChild(0).gameObject;
                lilacDed.GetComponent<SpriteRenderer>().sprite = TylerKozaki.dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's defeated")[7];
            }
        }
    }
}
