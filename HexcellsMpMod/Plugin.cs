using HarmonyLib;
using IllusionPlugin;

namespace HexcellsMpMod
{
    class Plugin : IPlugin
    {
        public string Name =>  "Hexcells Multiplayer";

        public string Version => "1.0.0";

        public void OnApplicationQuit() {}

        public void OnApplicationStart()
        {
            var harmony = new Harmony("HexcellsMultiplayerMod");
            harmony.PatchAll();
        }

        public void OnFixedUpdate() {}
        public void OnLevelWasInitialized(int level) {}
        public void OnLevelWasLoaded(int level) {}
        public void OnUpdate() {}
    }
}
