using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Template.ConfigManager;
using Template.Events;
using System.Reflection;


namespace Template
{
    [BepInPlugin("darmuh.Template", "Template", (PluginInfo.PLUGIN_VERSION))]


    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "darmuh.Template";
            public const string PLUGIN_NAME = "Template";
            public const string PLUGIN_VERSION = "0.0.1";
        }
        
        internal static ManualLogSource Log;

        //Compatibility
        public bool LobbyCompat = false;
        public Terminal Terminal;


        private void Awake()
        {
            instance = this;
            Log = base.Logger;
            Log.LogInfo((object)$"{PluginInfo.PLUGIN_NAME} is loading with version {PluginInfo.PLUGIN_VERSION}!");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ConfigSetup.BindConfigSettings();
            Subscribers.Subscribe();
            Log.LogInfo($"{PluginInfo.PLUGIN_NAME} load complete!");
        }

        internal static void MoreLogs(string message)
        {
            if (ConfigSetup.ExtensiveLogging.Value)
                Log.LogInfo(message);
            else
                return;
        }

        internal static void Spam(string message)
        {
            if (ConfigSetup.DeveloperLogging.Value)
                Log.LogDebug(message);
            else
                return;
        }

        internal static void ERROR(string message)
        {
            Log.LogError(message);
        }
    }

}