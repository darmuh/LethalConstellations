﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OpenLib.ConfigManager;
using OpenLib.CoreMethods;
using OpenLib.Events;
using System.Collections.Generic;
using System.Reflection;


namespace OpenLib
{
    [BepInPlugin("darmuh.OpenLib", "OpenLib", (PluginInfo.PLUGIN_VERSION))]


    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "darmuh.OpenLib";
            public const string PLUGIN_NAME = "OpenLib";
            public const string PLUGIN_VERSION = "0.0.1";
        }
        
        internal static ManualLogSource Log;

        //Compatibility
        public bool LobbyCompat = false;
        public bool TerminalFormatter = false;

        public static List<TerminalKeyword> keywordsAdded = [];
        public static List<TerminalNode> nodesAdded = [];

        public Terminal Terminal;
        public static List<TerminalNode> Allnodes = [];
        public static List<TerminalNode> ShopNodes = [];


        private void Awake()
        {
            instance = this;
            Log = base.Logger;
            Log.LogInfo((object)$"{PluginInfo.PLUGIN_NAME} is loading with version {PluginInfo.PLUGIN_VERSION}!");
            ConfigSetup.defaultManagedBools = new();
            ConfigSetup.defaultListing = new();
            CommandRegistry.InitListing(ref ConfigSetup.defaultListing);
            ConfigSetup.BindConfigSettings(ConfigSetup.defaultManagedBools);
            Config.ConfigReloaded += ConfigMisc.OnConfigReloaded;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            EventUsage.Subscribers();
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