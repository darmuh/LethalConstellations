using BepInEx.Configuration;
using LethalConstellations.ConfigManager;
using LethalConstellations.PluginCore;
using LethalLevelLoader;
using OpenLib.Common;
using System.Collections.Generic;
using static OpenLib.ConfigManager.ConfigSetup;
using static LethalConstellations.PluginCore.Collections;
using UnityEngine;
using LethalConstellations.Compat;


namespace LethalConstellations.EventStuff
{
    internal class LLLStuff
    {
        public static void LLLSetup()
        {
            Start();

            Plugin.Spam(Configuration.ConstellationList.Value);
            ConstellationsList = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.ConstellationList.Value, ',');
            List<string> ignoreList = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.IgnoreList.Value, ',');
            ignoreList = ignoreList.ConvertAll(s  => s.ToLower());
            Plugin.Spam("list created");

            foreach (string name in ConstellationsList)
            {
                ConfigEntry<int> constellationPrice = MakeClampedInt(Configuration.GeneratedConfig, "Constellations", $"{name} constellationPrice", 0, $"Set the price to route to this constellation and it's defaultMoon", 0, 9999);

                ConfigEntry<string> menuText = MakeString(Configuration.GeneratedConfig, "Constellations", $"{name} menuText", $"Route to {ConstellationWord} [name][~t]$[price][~n]Default Moon: [defaultmoon]", "text displayed for this constellation's menu item");

                ConfigEntry<string> shortCuts = MakeString(Configuration.GeneratedConfig, "Constellations", $"{name} shortcuts", "", $"Specify a list of shortcuts to use for routing to the {name} constellation.\nEach shortcut keyword is separated by a ','");

                ConfigEntry<bool> isHiding = MakeBool(Configuration.GeneratedConfig, "Constellations", $"{name} isHidden", false, "Enable this to hide this constellation from the constellation listing");

                ConfigEntry<bool> canGoCompany = MakeBool(Configuration.GeneratedConfig, "Constellations", $"{name} canRouteCompany", true, "Enable this to allow this constellation to route to the company moon");

                ClassMapper constClass = new(name,constellationPrice.Value);
                constClass.menuText = menuText.Value;
                constClass.isHidden = isHiding.Value;
                constClass.canRouteCompany = canGoCompany.Value;
                constClass.shortcutList = shortCuts.Value;

                if (Configuration.ConstellationSpecificInfoNodes.Value)
                {
                    ConfigEntry<string> infoText = MakeString(Configuration.GeneratedConfig, "Constellations", $"{name} infoText", $"ConstellationWord - {name}\n\n\nThis constellation contains moons in it. Route to it and find out which!\r\n\r\n", "text that displays with the info command for this constellation");
                    constClass.infoText = infoText.Value;
                }

                constClass.constelMoons = [];
                ConstellationStuff.Add(constClass);
            }

            Plugin.Spam("about to sort through extendedlevel");
            GetLLLStuffForConfig(PatchedContent.VanillaExtendedLevels, ignoreList, GetDefaultCName(ConstellationsList, 0));
            GetLLLStuffForConfig(PatchedContent.CustomExtendedLevels, ignoreList, GetDefaultCName(ConstellationsList, 1));

            SetDefaultMoon();
            Plugin.Spam($"ConfigCount: {Configuration.GeneratedConfig.Count}");
            Configuration.GeneratedConfig.Save();
            LethalConfigStuff();
            MoonStuff.ModifyMoonPrices();
        }

        internal static void LethalConfigStuff()
        {
            if(Plugin.instance.LethalConfig)
                LethalConfigCompat.QueueConfig(Configuration.GeneratedConfig);
        }

        internal static string GetDefaultCName(List<string> constList, int index)
        {
            if (ConstellationsList.Count < index+1)
                return "default";
            else
                return ConstellationsList[index];
        }

        internal static void SetDefaultMoon()
        {
            Plugin.Spam("Setting default moons for Constellation Stuff");
            Plugin.Spam($"{ConstellationStuff.Count}");

            foreach(ClassMapper constel in ConstellationStuff)
            {
                ConfigEntry<string> defaultMoon = MakeString(Configuration.GeneratedConfig, "Constellations", $"{constel.consName} defaultMoon", GetRandomDefault(constel), "default moon to route to when selecting this constellation");
                constel.defaultMoon = defaultMoon.Value;
                Plugin.Spam($"Default Moon for {constel.consName} set to {defaultMoon.Value}");
            }
            
        }

        //iterate through extendedLevel list
        internal static void GetLLLStuffForConfig(List<ExtendedLevel> extendedLevels, List<string> ignoreList, string defaultValue)
        {
            foreach (ExtendedLevel extendedLevel in extendedLevels)
            {
                if (ignoreList.Contains(extendedLevel.NumberlessPlanetName.ToLower()))
                    continue;

                if (extendedLevel.NumberlessPlanetName.ToLower() == CompanyMoon.ToLower())
                    continue;

                ConfigEntry<string> levelToConstellation = MakeString(Configuration.GeneratedConfig, "Moons", $"{extendedLevel.NumberlessPlanetName} ConstellationWord", defaultValue, $"Specify which constellation {extendedLevel.NumberlessPlanetName} belongs to\nShould match an item from [ConstellationList]");

                AddToConstelMoons(extendedLevel.NumberlessPlanetName, levelToConstellation.Value);

                ConfigEntry<int> levelPrice = MakeClampedInt(Configuration.GeneratedConfig, "Moons", $"{extendedLevel.NumberlessPlanetName} Price", extendedLevel.RoutePrice, "Set a custom route price for this moon (should autopopulate with the correct default price)", 0, 9999);

                MoonPrices.Add(extendedLevel.NumberlessPlanetName, levelPrice.Value);
            }
        }

        internal static void AddToConstelMoons(string newMoon, string cName)
        {
            if (ConstellationStuff.Count < 0)
                return;

            foreach(ClassMapper constel in ConstellationStuff)
            {
                if(constel.consName == cName && !constel.constelMoons.Contains(newMoon))
                {
                    constel.constelMoons.Add(newMoon);
                    Plugin.Spam($"adding {newMoon} to {cName}");
                }
            }
        }

        //check for configitem, no out
        internal static bool CheckForConfigName(string configName)
        {
            foreach(ConfigDefinition item in Plugin.instance.Config.Keys)
            {
                if (item.Key == configName)
                    return true;
            }

            return false;
        }

        //get string out config
        internal static bool CheckForConfigName(string configName, out ConfigEntry<string> configEntry)
        {
            configEntry = null;
            foreach (ConfigDefinition item in Configuration.GeneratedConfig.Keys)
            {
                if (item.Key == configName)   
                    return Configuration.GeneratedConfig.TryGetEntry(item, out configEntry);
            }

            return false;
        }

        //get int out config
        internal static bool CheckForConfigName(string configName, out ConfigEntry<int> configEntry)
        {
            configEntry = null;
            foreach (ConfigDefinition item in Configuration.GeneratedConfig.Keys)
            {
                if (item.Key == configName)
                    return Configuration.GeneratedConfig.TryGetEntry(item, out configEntry);
            }

            return false;
        }

        //set defaultMoon for a constellation
        internal static string GetRandomDefault(ClassMapper constellation)
        {
            if (constellation.constelMoons.Count == 0)
                return "";

            int rand = Random.Range(0, constellation.constelMoons.Count - 1);
            return constellation.constelMoons[rand];
        }
    }
}
