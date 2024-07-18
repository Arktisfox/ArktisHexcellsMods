using UnityEngine;

namespace PotentialCellsMod.Data
{
    class Materials
    {
        public static Material potentialMaterialLight;
        public static Material potentialMaterialDark;

        private static Texture2D LoadTexture(byte[] data)
        {
            Texture2D texture = new Texture2D(8, 8, TextureFormat.ARGB32, true);
            texture.LoadImage(data);
            texture.Apply(true);
            return texture;
        }

        public static void CreateMaterials(Material reference)
        {
            if(potentialMaterialDark == null)
            {
                potentialMaterialDark = Object.Instantiate(reference);
                potentialMaterialDark.name = "potential_cells_dark";
                potentialMaterialDark.mainTexture = LoadTexture(Properties.Resources.hex_black_potential);
            }
            if (potentialMaterialLight == null)
            {
                potentialMaterialLight = Object.Instantiate(reference);
                potentialMaterialLight.name = "potential_cells_light";
                potentialMaterialLight.mainTexture = LoadTexture(Properties.Resources.hex_blue_potential);
            }
        }
    }
}