﻿using BepInEx.Configuration;
using LethalConstellations.PluginCore;
using static OpenLib.ConfigManager.ConfigSetup;


namespace LethalConstellations.ConfigManager
{
    public static class Configuration
    {
        public static ConfigFile GeneratedConfig;
        //public static List<ConfigEntry<int>> AutomatedIntConfigs = [];
        //public static List<ConfigEntry<string>> AutomatedStringConfigs = [];

        //Logging
        public static ConfigEntry<bool> ExtensiveLogging { get; internal set; }
        public static ConfigEntry<bool> DeveloperLogging { get; internal set; }
        //public static ConfigEntry<string> DetectedMoonsList { get; internal set; }
        
        //ConstellationWord Setup
        public static ConfigEntry<string> ConstellationList { get; internal set; }
        public static ConfigEntry<bool> HideUnaffordableConstellations { get; internal set; }
        public static ConfigEntry<bool> ModifyMoonPrices { get; internal set; }
        public static ConfigEntry<string> IgnoreList { get; internal set; }
        public static ConfigEntry<bool> RequireConfirmation { get; internal set; }
        public static ConfigEntry<string> AddHintTo {  get; internal set; }

        //Customization
        public static ConfigEntry<string> ConstellationsWord { get; internal set; }
        public static ConfigEntry<string> ConstellationWord { get; internal set; }
        public static ConfigEntry<string> ConstellationsShortcuts { get; internal set; }
        public static ConfigEntry<string> ConstellationsHintText { get; internal set; }
        public static ConfigEntry<string> ConstellationsInfoText { get; internal set; }

        //more customization at the autogenerated level
        public static ConfigEntry<bool> ConstellationSpecificInfoNodes { get; internal set; }

        public static void BindConfigSettings()
        {
            Plugin.Log.LogInfo("Binding configuration settings");

            //Debug
            ExtensiveLogging = MakeBool(Plugin.instance.Config, "Debug", "ExtensiveLogging", false, "Enable or Disable extensive logging for this mod.");
            DeveloperLogging = MakeBool(Plugin.instance.Config, "Debug", "DeveloperLogging", false, "Enable or Disable developer logging for this mod. (this will fill your log file FAST)");

            //Setup
            ConstellationList = MakeString(Plugin.instance.Config, "Setup", "ConstellationList", "Alpha,Bravo,Charlie", "Comma separated list of your ConstellationWord Names");
            IgnoreList = MakeString(Plugin.instance.Config, "Setup", "IgnoreList", "Liquidation,Moon2Example", "Comma separated list of moon names that should not be touched by this mod.\nGenerally you'll almost always have Liquidation in this list until it's a real moon as well as any moons you dont want associated with any particular constellation");
            ModifyMoonPrices = MakeBool(Plugin.instance.Config, "Setup", "ModifyMoonPrices", true, "Disable this to stop this mod from modifying any moon prices");
            RequireConfirmation = MakeBool(Plugin.instance.Config, "Setup", "RequireConfirmation", true, "Enable this to require add a confirmation check before routing to a constellation and spending credits");
            HideUnaffordableConstellations = MakeBool(Plugin.instance.Config, "Setup", "HideUnaffordableConstellations", false, "Enable this to hide constellations that you can't afford to route to.");
            AddHintTo = MakeClampedString(Plugin.instance.Config, "Setup", "AddHintTo", "both", "Choose where to add hints to the main 'constellations' command", new AcceptableValueList<string>("both", "none", "help", "other"));
            ConstellationSpecificInfoNodes = MakeBool(Plugin.instance.Config, "Setup", "ConstellationSpecificInfoNodes", false, "Enable this to add config options to the dynamic config for each constellation to have info nodes with customizable text");
            

            //Customization
            ConstellationsWord = MakeString(Plugin.instance.Config, "Customization", "ConstellationsWord", "Constellations", "Use this config item to change any instance of the word 'Constellations' with your own specific word.\nThe terminal keyword will use this one!!!");
            ConstellationWord = MakeString(Plugin.instance.Config, "Customization", "ConstellationWord", "Constellation", "Use this config item to change any instance of the word 'Constellation' with your own specific word.");
            ConstellationsShortcuts = MakeString(Plugin.instance.Config, "Customization", "ConstellationsShortcut", "", "Specify a list of shortcuts to use for the constellations menu command.\nEach shortcut keyword is separated by a ','");
            ConstellationsHintText = MakeString(Plugin.instance.Config, "Customization", "ConstellationsHintText", ">[keyword]\nTo display Constellations available for routing", "text displayed for the hint added to commands like 'help' and 'other' regarding the constellations menu command");
            ConstellationsInfoText = MakeString(Plugin.instance.Config, "Customization", "ConstellationsInfoText", "Use this command to display Constellations available for routing.\n\nRoute to a specific constellation to update your moons listing!\r\n\r\n", "Use this config item to change any instance of the word 'ConstellationWord' with your own specific word");


            Plugin.instance.Config.Save();

            Collections.ConstellationWord = ConstellationWord.Value;
            Collections.ConstellationsWord = ConstellationsWord.Value;
            //GetAutoGenerated

        }
    }
}