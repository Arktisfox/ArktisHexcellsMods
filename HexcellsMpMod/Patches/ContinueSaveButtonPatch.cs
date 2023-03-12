using HarmonyLib;
using UnityEngine;

namespace HexcellsMpMod.Patches
{
    /// <summary>
    /// Disables the continue saved button if in a multiplayer session (hosting or joining)
    /// </summary>
    [HarmonyPatch(typeof(ContinueSaveButton), nameof(ContinueSaveButton.Update))]
    class ContinueSaveButtonPatch
    {
        static void Postfix(ContinueSaveButton __instance)
        {
            if (MpModManager.Instance.InSession)
            {
                __instance.GetComponent<MeshCollider>().enabled = false;
            }
        }
    }
}
