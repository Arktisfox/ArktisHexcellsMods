using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PotentialCellsMod.Extensions
{
    internal static class Extensions
    {
        public static void ResetToOrange(this HexBehaviour behaviour)
        {
            var rendererField = typeof(HexBehaviour).GetField("thisRenderer", BindingFlags.Public | BindingFlags.Instance);
            Renderer renderer = rendererField.GetValue(behaviour) as Renderer;
            var mainField = typeof(HexBehaviour).GetField("main", BindingFlags.Public | BindingFlags.Instance);
            Material mainMaterial = mainField.GetValue(behaviour) as Material;
            renderer.material = mainMaterial;
        }

        public static bool IsMarkedPotential(this HexBehaviour behaviour)
        {
            var rendererField = typeof(HexBehaviour).GetField("thisRenderer", BindingFlags.Public | BindingFlags.Instance);
            Renderer renderer = rendererField.GetValue(behaviour) as Renderer;
            var material = renderer.sharedMaterial;
            return material.name.StartsWith("potential_cells_");
        }
    }
}
