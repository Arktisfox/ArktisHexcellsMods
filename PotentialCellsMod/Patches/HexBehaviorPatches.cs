using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace PotentialCellsMod.Patches
{
    /// <summary>
    /// Create materials
    /// </summary>
    [HarmonyPatch(typeof(HexBehaviour), nameof(HexBehaviour.Start))]
    class HexBehaviorStartPatch
    {
        static void CreateMaterials(Material reference)
        {
            Data.Materials.potentialMaterialLight = Object.Instantiate(reference);
            Data.Materials.potentialMaterialDark = Object.Instantiate(reference);

            Texture2D darkTexture = new Texture2D(8, 8, TextureFormat.ARGB32, true);
            darkTexture.LoadImage(Properties.Resources.hex_black_potential);
            darkTexture.Apply(true);
            Data.Materials.potentialMaterialDark.mainTexture = darkTexture;

            Texture2D lightTexture = new Texture2D(8, 8, TextureFormat.ARGB32, true);
            lightTexture.LoadImage(Properties.Resources.hex_blue_potential);
            lightTexture.Apply(true);
            Data.Materials.potentialMaterialLight.mainTexture = lightTexture;
        }

        static void Postfix(HexBehaviour __instance)
        {
            var mainField = typeof(HexBehaviour).GetField("main", BindingFlags.Public | BindingFlags.Instance);
            if(Data.Materials.potentialMaterialLight == null)
            {
                Material mainMaterial = mainField.GetValue(__instance) as Material;
                CreateMaterials(mainMaterial);
            }
        }
    }

    /// <summary>
    /// Don't swap materials if we're using our custom ones
    /// </summary>
    [HarmonyPatch(typeof(HexBehaviour), nameof(HexBehaviour.OnMouseExit))]
    class HexBehaviorOnMouseExitPatch
    {
        static bool Prefix(HexBehaviour __instance)
        {
            var rendererField = typeof(HexBehaviour).GetField("thisRenderer", BindingFlags.Public | BindingFlags.Instance);
            Renderer renderer = rendererField.GetValue(__instance) as Renderer;

            return renderer.sharedMaterial != Data.Materials.potentialMaterialDark 
                && renderer.sharedMaterial != Data.Materials.potentialMaterialLight;
        }
    }

    /// <summary>
    /// Restore custom material after hover.. Kind of ugly :(
    /// </summary>
    [HarmonyPatch(typeof(HexBehaviour), nameof(HexBehaviour.OnMouseEnter))]
    class HexBehaviorOnMouseEnterPatch
    {
        static void Prefix(HexBehaviour __instance, out Material __state)
        {
            var rendererField = typeof(HexBehaviour).GetField("thisRenderer", BindingFlags.Public | BindingFlags.Instance);
            Renderer renderer = rendererField.GetValue(__instance) as Renderer;
            __state = renderer.sharedMaterial;
        }
        
        static void Postfix(HexBehaviour __instance, Material __state)
        {
            var rendererField = typeof(HexBehaviour).GetField("thisRenderer", BindingFlags.Public | BindingFlags.Instance);
            Renderer renderer = rendererField.GetValue(__instance) as Renderer;
            if(__state == Data.Materials.potentialMaterialDark || __state == Data.Materials.potentialMaterialLight)
            {
                renderer.material = __state;
            }
        }
    }

    /// <summary>
    /// Assign custom materials
    /// </summary>
    [HarmonyPatch(typeof(HexBehaviour), nameof(HexBehaviour.OnMouseOver))]
    class HexBehaviorOnMouseOverPatch
    {
        static void Postfix(HexBehaviour __instance)
        {
            var highlightedField = typeof(HexBehaviour).GetField("highlighted", BindingFlags.Public | BindingFlags.Instance);
            var rendererField = typeof(HexBehaviour).GetField("thisRenderer", BindingFlags.Public | BindingFlags.Instance);
            var musicDirectorField = typeof(HexBehaviour).GetField("musicDirector", BindingFlags.NonPublic | BindingFlags.Instance);

            if (Input.GetMouseButtonDown(2))
            {
                MusicDirector musicDirector = musicDirectorField.GetValue(__instance) as MusicDirector;
                Renderer renderer = rendererField.GetValue(__instance) as Renderer;
                Material highlightedMaterial = highlightedField.GetValue(__instance) as Material;
                Material nextMaterial;

                if (renderer.sharedMaterial == highlightedMaterial)
                    nextMaterial = Data.Materials.potentialMaterialLight;
                else if (renderer.sharedMaterial == Data.Materials.potentialMaterialLight)
                    nextMaterial = Data.Materials.potentialMaterialDark;
                else
                    nextMaterial = highlightedMaterial;
                renderer.material = nextMaterial;

                musicDirector.PlayNoteA(__instance.transform.position.x / 7.04f);
            }
        }
    }
}
