using HarmonyLib;
using IllusionPlugin;

namespace PotentialCellsMod
{
    class Plugin : IPlugin
    {
        public string Name => "Hexcells Potential Cells";

        public string Version => "1.0.0";

        public void OnApplicationQuit() { }

        public void OnApplicationStart()
        {
            var harmony = new Harmony("HexcellsPotentialCells");
            harmony.PatchAll();
        }

        public void OnFixedUpdate() { }
        public void OnLevelWasInitialized(int level) { }
        public void OnLevelWasLoaded(int level) { }
        public void OnUpdate() { }
    }
}
