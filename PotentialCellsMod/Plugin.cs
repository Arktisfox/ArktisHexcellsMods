using HarmonyLib;
using IllusionPlugin;
using PotentialCellsMod.Extensions;
using UnityEngine;

namespace PotentialCellsMod
{
    class Plugin : IPlugin
    {
        public string Name => "Hexcells Potential Cells";

        public string Version => "1.1.0";

        public void OnApplicationQuit() { }

        public void OnApplicationStart()
        {
            var harmony = new Harmony("HexcellsPotentialCells");
            harmony.PatchAll();
        }

        public void OnFixedUpdate() { }
        public void OnLevelWasInitialized(int level) { }
        public void OnLevelWasLoaded(int level) { }
        public void OnUpdate() 
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.X))
            {
                // clear all potential cells
                Object.FindObjectOfType<MusicDirector>().PlayNoteA(0.0f);
                foreach (var hexBehaviour in Object.FindObjectsOfType<HexBehaviour>())
                {
                    if (hexBehaviour.IsMarkedPotential())
                    {
                        hexBehaviour.ResetToOrange();
                    }
                }
            }
        }
    }
}
