using BepInEx;
using BepInEx.Logging;
using LethalConstellations.ConfigManager;
using LethalConstellations.EventStuff;
using BepInEx.Configuration;
using System.IO;
using LethalConstellations.PluginCore;


namespace LethalConstellations
{
    [BepInPlugin("com.github.darmuh.LethalConstellations", "LethalConstellations", (PluginInfo.PLUGIN_VERSION))]
    [BepInDependency("imabatby.lethallevelloader", "1.3.8")]
    [BepInDependency("darmuh.OpenLib", "0.1.8")]


    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "com.github.darmuh.LethalConstellations";
            public const string PLUGIN_NAME = "LethalConstellations";
            public const string PLUGIN_VERSION = "0.2.2";
        }
        
        internal static ManualLogSource Log;

        //Compatibility
        public bool LobbyCompat = false;
        public bool LethalConfig = false;
        public bool LethalMoonUnlocks = false;
        public bool LethalNetworkAPI = false;
        public Terminal Terminal;
        public TerminalNode dummyNode;

        private void Awake()
        {
            instance = this;
            Log = base.Logger;
            Log.LogInfo((object)$"{PluginInfo.PLUGIN_NAME} is loading with version {PluginInfo.PLUGIN_VERSION}!");
            Subscribers.Subscribe();
            Configuration.GeneratedConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, $"{PluginInfo.PLUGIN_NAME}_Generated.cfg"), true);
            Configuration.BindConfigSettings();


            Log.LogInfo($"{PluginInfo.PLUGIN_NAME} load complete!");
        }

        internal static void MoreLogs(string message)
        {
            if (Configuration.ExtensiveLogging.Value)
                Log.LogInfo(message);
            else
                return;
        }

        internal static void Spam(string message)
        {
            if (Configuration.DeveloperLogging.Value)
                Log.LogDebug(message);
            else
                return;
        }

        internal static void ERROR(string message)
        {
            Log.LogError(message);
        }

        internal static void WARNING(string message)
        {
            Log.LogWarning(message);
        }
    }

}