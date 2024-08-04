using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SimLibGUI
{
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class UniversalPlugin : BaseUnityPlugin
    {
        private const string modGUID = "com.SimPleased.SimLibGUI";
        private const string modName = "SimLibGUI";
        private const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            Logger.LogInfo($"{modName} has started!");
        }
    }
}
