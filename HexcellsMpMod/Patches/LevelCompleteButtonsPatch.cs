using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace HexcellsMpMod.Patches
{
    /// <summary>
    /// Sync server host exiting a puzzle early
    /// </summary>
    [HarmonyPatch(typeof(LevelCompleteButtons), nameof(LevelCompleteButtons.OnMouseOver))]
    class LevelCompleteButtonsMouseOverPatch
    {
        static void Postfix(LevelCompleteButtons __instance)
        {
            var buttonTypeField = typeof(LevelCompleteButtons).GetField("buttonType", BindingFlags.Instance | BindingFlags.Public);
            if (Input.GetMouseButtonDown(0))
            {
                var buttonType = (LevelCompleteButtons.ButtonType)buttonTypeField.GetValue(__instance);
                if (buttonType == LevelCompleteButtons.ButtonType.Menu || buttonType == LevelCompleteButtons.ButtonType.MenuCustom)
                {
                    MpModManager.Instance.HostQuitGame();
                }
            }
        }
    }

    /// <summary>
    /// Disable next button on custom levels, if not the host
    /// </summary>
    [HarmonyPatch(typeof(LevelCompleteButtons), nameof(LevelCompleteButtons.Start))]
    class LevelCompleteButtonsStartrPatch
    {
        static void Postfix(LevelCompleteButtons __instance)
        {
            var buttonTypeField = typeof(LevelCompleteButtons).GetField("buttonType", BindingFlags.Instance | BindingFlags.Public);
            if (MpModManager.Instance.InSession && !MpModManager.Instance.IsHosting)
            {
                var buttonType = (LevelCompleteButtons.ButtonType)buttonTypeField.GetValue(__instance);
                if (buttonType == LevelCompleteButtons.ButtonType.NextCustom)
                {
                    __instance.GetComponent<BoxCollider>().enabled = false;
                }
            }
        }
    }
}
