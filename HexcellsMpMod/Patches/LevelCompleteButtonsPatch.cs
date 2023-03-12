using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace HexcellsMpMod.Patches
{
    /// <summary>
    /// Sync server host exiting a puzzle early
    /// </summary>
    [HarmonyPatch(typeof(LevelCompleteButtons), nameof(LevelCompleteButtons.OnMouseOver))]
    class LevelCompleteButtonsPatch
    {
        static void Postfix(LevelCompleteButtons __instance)
        {
            var buttonTypeField = typeof(LevelCompleteButtons).GetField("buttonType", BindingFlags.Instance | BindingFlags.Public);
            if (Input.GetMouseButtonDown(0))
            {
                var buttonType = (LevelCompleteButtons.ButtonType)buttonTypeField.GetValue(__instance);
                if (buttonType == LevelCompleteButtons.ButtonType.Menu)
                {
                    MpModManager.Instance.HostQuitGame();
                }
            }
        }
    }
}
