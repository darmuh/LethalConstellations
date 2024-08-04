﻿using BepInEx.Configuration;
using LethalConstellations.PluginCore;
using static OpenLib.ConfigManager.ConfigSetup;


namespace LethalConstellations.ConfigManager
{
    public static class Configuration
    {
        public static ConfigFile MainConfig;
        public static ConfigFile GeneratedConfig;
        //public static List<ConfigEntry<int>> AutomatedIntConfigs = [];
        //public static List<ConfigEntry<string>> AutomatedStringConfigs = [];

        //Logging
        public static ConfigEntry<bool> ExtensiveLogging { get; internal set; }
        public static ConfigEntry<bool> DeveloperLogging { get; internal set; }
        //public static ConfigEntry<string> DetectedMoonsList { get; internal set; }
        
        //Constellation Setup
        public static ConfigEntry<string> ConstellationList { get; internal set; }
        public static ConfigEntry<bool> ModifyMoonPrices { get; internal set; }
        public static ConfigEntry<string> IgnoreList { get; internal set; }
        public static ConfigEntry<bool> RequireConfirmation { get; internal set; }

        //Customization
        public static ConfigEntry<string> ConstellationsWord { get; internal set; }
        public static ConfigEntry<string> ConstellationWord { get; internal set; }
        public static ConfigEntry<string> ConstellationsOtherText { get; internal set; }
        public static ConfigEntry<string> ConstellationsInfoText { get; internal set; }

        //more customization at the autogenerated level
        public static ConfigEntry<bool> ConstellationSpecificInfoNodes { get; internal set; }

        public static void BindConfigSettings()
        {
            Plugin.Log.LogInfo("Binding configuration settings");

            //Debug
            ExtensiveLogging = MakeBool(MainConfig, "Debug", "ExtensiveLogging", false, "Enable or Disable extensive logging for this mod.");
            DeveloperLogging = MakeBool(MainConfig, "Debug", "DeveloperLogging", false, "Enable or Disable developer logging for this mod. (this will fill your log file FAST)");

            //Setup
            ConstellationList = MakeString(MainConfig, "Setup", "ConstellationList", "Alpha,Bravo,Charlie", "Comma separated list of your Constellation Names");
            IgnoreList = MakeString(MainConfig, "Setup", "IgnoreList", "Liquidation,Moon2Example", "Comma separated list of moon names that should not be touched by this mod.\nGenerally you'll almost always have Liquidation in this list until it's a real moon as well as any moons you dont want associated with any particular constellation");
            ModifyMoonPrices = MakeBool(MainConfig, "Setup", "ModifyMoonPrices", true, "Disable this to stop this mod from modifying any moon prices");
            RequireConfirmation = MakeBool(MainConfig, "Setup", "RequireConfirmation", true, "Enable this to require add a confirmation check before routing to a constellation and spending credits");
            ConstellationSpecificInfoNodes = MakeBool(MainConfig, "Setup", "ConstellationSpecificInfoNodes", false, "Enable this to add config options to the dynamic config for each constellation to have info nodes with customizable text");
            

            //Customization
            ConstellationsWord = MakeString(MainConfig, "Customization", "ConstellationsWord", "Constellations", "Use this config item to change any instance of the word 'LethalConstellations' with your own specific word.\nThe terminal keyword will use this one!!!");
            ConstellationWord = MakeString(MainConfig, "Customization", "ConstellationWord", "Constellation", "Use this config item to change any instance of the word 'Constellation' with your own specific word.");
            ConstellationsOtherText = MakeString(MainConfig, "Customization", "ConstellationsOtherText", ">[keyword]\nTo display LethalConstellations available for routing", "text displayed in the 'Other' command output regarding the constellation menu command");
            ConstellationsInfoText = MakeString(MainConfig, "Customization", "ConstellationsInfoText", "Use this command to display LethalConstellations available for routing.\n\nRoute to a specific constellation to update your moons listing!\r\n\r\n", "Use this config item to change any instance of the word 'Constellation' with your own specific word");


            MainConfig.Save();

            Collections.Constellation = Configuration.ConstellationWord.Value;
            Collections.Constellations = Configuration.ConstellationsWord.Value;
            //GetAutoGenerated

        }
    }
}