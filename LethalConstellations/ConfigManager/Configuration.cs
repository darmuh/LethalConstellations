﻿using BepInEx.Configuration;
using LethalConstellations.PluginCore;
using static OpenLib.ConfigManager.ConfigSetup;


namespace LethalConstellations.ConfigManager
{
    public static class Configuration
    {
        public static ConfigFile GeneratedConfig;

        //Logging
        public static ConfigEntry<bool> ExtensiveLogging { get; internal set; }
        public static ConfigEntry<bool> DeveloperLogging { get; internal set; }
        //public static ConfigEntry<string> DetectedMoonsList { get; internal set; }

        //ConstellationWord Setup
        public static ConfigEntry<string> ConstellationList { get; internal set; }
        public static ConfigEntry<string> ManualSetupListing { get; internal set; }
        public static ConfigEntry<bool> HideUnaffordableConstellations { get; internal set; }
        public static ConfigEntry<bool> ModifyMoonPrices { get; internal set; }
        public static ConfigEntry<string> IgnoreList { get; internal set; }
        public static ConfigEntry<bool> RequireConfirmation { get; internal set; }
        public static ConfigEntry<string> AddHintTo { get; internal set; }
        public static ConfigEntry<string> CompanyDefaultConstellation { get; internal set; }
        public static ConfigEntry<string> StartingConstellation { get; internal set; }
        public static ConfigEntry<string> AcceptableStartingConstellations { get; internal set; }
        public static ConfigEntry<bool> ConstellationsUseFauxWords { get; internal set; }
        public static ConfigEntry<bool> ReturnToLastConstellationFromCompany { get; internal set; }

        //Customization
        public static ConfigEntry<string> ConstellationsWord { get; internal set; }
        public static ConfigEntry<string> ConstellationWord { get; internal set; }
        public static ConfigEntry<string> ConstellationsShortcuts { get; internal set; }
        public static ConfigEntry<string> ConstellationsHintText { get; internal set; }
        public static ConfigEntry<string> ConstellationsInfoText { get; internal set; }
        internal static ConfigEntry<string> MainConfigCode;
        internal static ConfigEntry<string> GeneratedConfigCode;

        //more customization at the autogenerated level
        public static ConfigEntry<bool> ConstellationSpecificInfoNodes { get; internal set; }

        public static void BindConfigSettings()
        {
            Plugin.Log.LogInfo("Binding configuration settings");

            //Debug
            ExtensiveLogging = MakeBool(Plugin.instance.Config, "Debug", "ExtensiveLogging", false, "Enable or Disable extensive logging for this mod.");
            DeveloperLogging = MakeBool(Plugin.instance.Config, "Debug", "DeveloperLogging", false, "Enable or Disable developer logging for this mod. (this will fill your log file FAST)");

            //Setup
            ConstellationList = MakeString(Plugin.instance.Config, "Setup", "ConstellationList", "", "Comma separated list of your ConstellationWord Names, or leave blank for default LLL moon tags\nWARNING: Changing this setting will reset your generated config!\nThis setting cannot be updated in-game!");
            ManualSetupListing = MakeString(Plugin.instance.Config, "Setup", "ManualSetupListing", "Constellation1:experimentation,assurance,vow;Constellation2:titan,rend,dine;Constellation3:offense,march,artifice,embrion", "This configuration item will attempt to match constellation names listed in ConstellationList to moon names.\nThis can be left blank if you are already working within your generated config and is mostly here as an option to streamline initial setup\nNOTE: This will not override an already generated config!");
            CompanyDefaultConstellation = MakeString(Plugin.instance.Config, "Setup", "CompanyDefaultConstellation", "", "The company's default constellation.\nThis constellation will be assigned when loading saves starting at the company moon.");
            AcceptableStartingConstellations = MakeString(Plugin.instance.Config, "Setup", "AcceptableStartingConstellations", "", "When StartingConstellation is set to \"~random~\" this comma separated list will be used to choose a random starting constellation.\nLeave blank to choose from all possible constellations!\nNOTE: You can put duplicate entries of a constellation name to raise the odds that it will be chosen, this list does not need to have unique entries.");
            StartingConstellation = MakeString(Plugin.instance.Config, "Setup", "StartingConstellation", "", "Set this to customize the constellation you start at on a new save.\nIf set to \"~random~\" you will be routed to a random constellation at game start.\nIf set to a valid constellation, you will be routed to the default moon of the constellation when starting a new game.\nLeave blank to leave starting moon unaffected (experimentation or handled by another mod)\n");
            IgnoreList = MakeString(Plugin.instance.Config, "Setup", "IgnoreList", "Liquidation,Moon2Example", "Comma separated list of moon names that should not be touched by this mod.\nGenerally you'll almost always have Liquidation in this list until it's a real moon as well as any moons you dont want associated with any particular constellation");
            ModifyMoonPrices = MakeBool(Plugin.instance.Config, "Setup", "ModifyMoonPrices", true, "Disable this to stop this mod from modifying any moon prices");
            RequireConfirmation = MakeBool(Plugin.instance.Config, "Setup", "RequireConfirmation", true, "Enable this to require add a confirmation check before routing to a constellation and spending credits");
            HideUnaffordableConstellations = MakeBool(Plugin.instance.Config, "Setup", "HideUnaffordableConstellations", false, "Enable this to hide constellations that you can't afford to route to.");
            AddHintTo = MakeClampedString(Plugin.instance.Config, "Setup", "AddHintTo", "both", "Choose where to add hints to the main 'constellations' command", new AcceptableValueList<string>("both", "none", "help", "other"));
            ConstellationSpecificInfoNodes = MakeBool(Plugin.instance.Config, "Setup", "ConstellationSpecificInfoNodes", false, "Enable this to add config options to the dynamic config for each constellation to have info nodes with customizable text");
            ConstellationsUseFauxWords = MakeBool(Plugin.instance.Config, "Setup", "ConstellationsUseFauxWords", true, "Disable this to use normal terminal keywords instead of Faux Keywords.\nWith this enabled you can only use a constellation word to route to it when in the constellations menu.");
            ReturnToLastConstellationFromCompany = MakeBool(Plugin.instance.Config, "Setup", "ReturnToLastConstellationFromCompany", true, "Disable this to always travel to the CompanyDefaultConstellation when returning from the company moon.\nEnable this if you wish to return to the constellation you routed to the company moon from.");
           
            //Customization
            ConstellationsWord = MakeString(Plugin.instance.Config, "Customization", "ConstellationsWord", "Constellations", "Use this config item to change any instance of the word 'Constellations' with your own specific word.\nThe terminal keyword will use this one!!!\nWARNING: Changing this setting will reset your generated config!\nThis setting cannot be updated in-game!");
            ConstellationWord = MakeString(Plugin.instance.Config, "Customization", "ConstellationWord", "Constellation", "Use this config item to change any instance of the word 'Constellation' with your own specific word.\nWARNING: Changing this setting will reset your generated config!\nThis setting cannot be updated in-game!");
            ConstellationsShortcuts = MakeString(Plugin.instance.Config, "Customization", "ConstellationsShortcut", "", "Specify a list of shortcuts to use for the constellations menu command.\nEach shortcut keyword is separated by a ','");
            ConstellationsHintText = MakeString(Plugin.instance.Config, "Customization", "ConstellationsHintText", ">[keyword]\nTo display Constellations available for routing", "text displayed for the hint added to commands like 'help' and 'other' regarding the constellations menu command");
            ConstellationsInfoText = MakeString(Plugin.instance.Config, "Customization", "ConstellationsInfoText", "Use this command to display Constellations available for routing.\n\nRoute to a specific constellation to update your moons listing!\r\n\r\n", "Use this config item to change any instance of the word 'ConstellationWord' with your own specific word");

            MainConfigCode = MakeString(Plugin.instance.Config, "Tools", "MainConfigCode", "", "Paste a code here from the web config editor for the main config\nWARNING: Some settings require a complete relaunch of the game and sometimes even deleting a previously generated config!");
            GeneratedConfigCode = MakeString(Plugin.instance.Config, "Tools", "GeneratedConfigCode", "", "Paste a code here from the web config editor for the generated config.\nThis will only apply if you have already loaded into the lobby once.");

            Plugin.instance.Config.Save();

            RemoveOrphanedEntries(Plugin.instance.Config);

            Collections.ConstellationWord = ConstellationWord.Value;
            Collections.ConstellationsWord = ConstellationsWord.Value;
        }
    }
}