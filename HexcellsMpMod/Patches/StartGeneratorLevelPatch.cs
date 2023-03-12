using HarmonyLib;
using TMPro;
using UnityEngine;
using System.Reflection;

namespace HexcellsMpMod.Patches
{
    /// <summary>
    /// Sync server host starting a puzzle
    /// </summary>
    [HarmonyPatch(typeof(StartGeneratorLevel), nameof(StartGeneratorLevel.OnMouseOver))]
    class StartGeneratorLevelPatch
    {
        static void Postfix(StartGeneratorLevel __instance)
        {
            var isActiveField = typeof(StartGeneratorLevel).GetField("isActive", BindingFlags.NonPublic | BindingFlags.Instance);
            var seedTextField = typeof(StartGeneratorLevel).GetField("seedText", BindingFlags.NonPublic | BindingFlags.Instance);

            bool isActive = (bool)isActiveField.GetValue(__instance);
            if (isActive && Input.GetMouseButtonDown(0))
            {
                TextMeshPro seedText = seedTextField.GetValue(__instance) as TextMeshPro;
                bool hardMode = GameObject.Find("Game Manager(Clone)").GetComponent<OptionsManager>().currentOptions.levelGenHardModeActive;
                MpModManager.Instance.HostStartGame(int.Parse(seedText.text), hardMode);
            }
        }
    }

    /// <summary>
    /// Prevent a client from starting on their own
    /// </summary>
    [HarmonyPatch(typeof(StartGeneratorLevel), nameof(StartGeneratorLevel.Update))]
    class StartGeneratorLevelUpdatePatch
    {
        static void Postfix(StartGeneratorLevel __instance)
        {
            var isActiveField = typeof(StartGeneratorLevel).GetField("isActive", BindingFlags.NonPublic | BindingFlags.Instance);
            if(MpModManager.Instance.InSession && !MpModManager.Instance.IsHosting)
            {
                isActiveField.SetValue(__instance, false);
            }
        }
    }
}
