using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace HexcellsMpMod.Patches
{
    [HarmonyPatch(typeof(CustomLevelManager), nameof(CustomLevelManager.LoadNextLevel))]
    class CustomLevelManagerLoadNextPatch
    {
        static void Postfix(CustomLevelManager __instance)
        {
            MpModManager.Instance.HostStartGame(__instance.currentLevelIndex, __instance.levelDataString);
        }
    }

    [HarmonyPatch(typeof(CustomLevelManager), nameof(CustomLevelManager.TryToLoadCustomLevelDataFromClipBoard))]
    class CustomLevelManagerTryToLoadCustomLevelDataFromClipBoardPatch
    {
        static bool Prefix(CustomLevelManager __instance)
        {
            if(MpModManager.Instance.InSession && !MpModManager.Instance.IsHosting)
            {
                return false;
            }
            return true;
        }
    }
}
