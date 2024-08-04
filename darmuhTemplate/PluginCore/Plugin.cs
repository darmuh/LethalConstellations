using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Constellations.ConfigManager;
using Constellations.EventStuff;
using System.Reflection;
using BepInEx.Configuration;
using System.IO;
using Constellations.PluginCore;


namespace Constellations
{
    [BepInPlugin("com.github.darmuh.Constellations", "LethalConstellations", (PluginInfo.PLUGIN_VERSION))]
    [BepInDependency("imabatby.lethallevelloader", "1.3.8")]
    [BepInDependency("darmuh.OpenLib", "0.1.6")]


    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "com.github.darmuh.LethalConstellations";
            public const string PLUGIN_NAME = "LethalConstellations";
            public const string PLUGIN_VERSION = "0.1.0";
        }
        
        internal static ManualLogSource Log;

        //Compatibility
        public bool LobbyCompat = false;
        public bool LethalConfig = false;
        public Terminal Terminal;


        private void Awake()
        {
            instance = this;
            Log = base.Logger;
            Log.LogInfo((object)$"{PluginInfo.PLUGIN_NAME} is loading with version {PluginInfo.PLUGIN_VERSION}!");
            Subscribers.Subscribe();
            Configuration.MainConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, $"{Plugin.PluginInfo.PLUGIN_GUID}_MainConfig.cfg"), true);
            Configuration.GeneratedConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, $"{Plugin.PluginInfo.PLUGIN_GUID}_GeneratedConfig.cfg"), true);
            Configuration.BindConfigSettings();
            //Collections.Constellation = Configuration.ConstellationWord.Value;
            Collections.Constellations = Configuration.ConstellationsWord.Value;


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