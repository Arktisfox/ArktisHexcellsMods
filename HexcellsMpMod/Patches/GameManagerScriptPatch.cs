using HarmonyLib;
using UnityEngine;

namespace HexcellsMpMod.Patches
{
    /// <summary>
    /// Create multiplayer mod manager instance
    /// </summary>
    [HarmonyPatch(typeof(GameManagerScript), nameof(GameManagerScript.Awake))]
    class GameManagerScriptPatch
    {
        static void Prefix()
        {
            new GameObject("Multiplayer Mod").AddComponent<MpModManager>();
        }
    }
}
