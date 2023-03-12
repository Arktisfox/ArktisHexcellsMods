using HarmonyLib;
using UnityEngine;

namespace HexcellsMpMod.Patches
{
    /// <summary>
    /// Sync server host exiting a puzzle early
    /// </summary>
    [HarmonyPatch(typeof(MenuExitButton), nameof(MenuExitButton.OnMouseOver))]
    class MenuExitButtonPatch
    {
        static void Postfix()
        {
            if (Input.GetMouseButtonDown(0))
            {
                MpModManager.Instance.HostQuitGame();
            }
        }
    }
}
