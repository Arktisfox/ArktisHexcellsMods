using HarmonyLib;
using UnityEngine;

namespace HexcellsMpMod.Patches
{
    /// <summary>
    /// Prevent clients from starting custom levels
    /// </summary>
    [HarmonyPatch(typeof(RedditListObject), nameof(RedditListObject.Awake))]
    class RedditListObjectPatch
    {
        static void Postfix(RedditListObject __instance)
        {
            if(MpModManager.Instance.InSession && ! MpModManager.Instance.IsHosting)
            {
                __instance.GetComponent<Collider>().enabled = false;
            }
        }
    }
}
