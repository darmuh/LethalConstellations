using BepInEx.Configuration;
using Constellations.ConfigManager;
using Constellations.PluginCore;
using LethalLevelLoader;
using OpenLib.Common;
using System.Collections.Generic;
using static OpenLib.ConfigManager.ConfigSetup;
using static Constellations.PluginCore.Collections;
using System.IO;
using Constellations.Compat;

namespace Constellations.EventStuff
{
    internal class LLLStuff
    {
        public static void LLLSetup()
        {
            ConstellationsToMoons.Clear();
            MoonPrices.Clear();
            DefaultMoons.Clear();
            ConstellationPrices.Clear();

            Plugin.Spam(Configuration.ConstellationList.Value);
            ConstellationsList = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.ConstellationList.Value, ',');
            List<string> ignoreList = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.IgnoreList.Value, ',');
            ignoreList = ignoreList.ConvertAll(s  => s.ToLower());
            Plugin.Spam("list created");

            Plugin.Spam("about to sort through extendedlevel");
            GetLLLStuffForConfig(PatchedContent.VanillaExtendedLevels, ignoreList, GetDefaultCName(ConstellationsList, 0));
            GetLLLStuffForConfig(PatchedContent.CustomExtendedLevels, ignoreList, GetDefaultCName(ConstellationsList, 1));

            foreach (string name in ConstellationsList)
            {
                ConfigEntry<string> defaultMoon = MakeString(Configuration.GeneratedConfig, "Constellations", $"{name} defaultMoon", GetFirstMatch(ConstellationsToMoons, name), "default moon to route to when selecting this constellation");
                DefaultMoons.Add(name, defaultMoon.Value);

                ConfigEntry<int> constellationPrice = MakeClampedInt(Configuration.GeneratedConfig, "Constellations", $"{name} constellationPrice", 0, $"Set the price to route to this constellation and it's defaultMoon", 0, 9999);

                ConfigEntry<string> menuText = MakeString(Configuration.GeneratedConfig, "Constellations", $"{name} menuText", "Route to Constellation [name]\t$[price]", "text displayed for this constellation's menu item");
                ConstellationMenuText.Add(name, menuText.Value);

                ConstellationPrices.Add(name, constellationPrice.Value);

                if (Configuration.ConstellationSpecificInfoNodes.Value)
                {
                    ConfigEntry<string> infoText = MakeString(Configuration.GeneratedConfig, "Constellations", $"{name} infoText", $"Constellation - {name}\n\n\nThis constellation contains moons in it. Route to it and find out which!\r\n\r\n", "text that displays with the info command for this constellation");
                    ConstellationInfoNodes.Add(name, infoText.Value);
                }
                
            }

            Plugin.Spam($"ConfigCount: {Configuration.GeneratedConfig.Count}");
            Configuration.GeneratedConfig.Save();
            MoonStuff.ModifyMoonPrices();
        }

        internal static string GetDefaultCName(List<string> constList, int index)
        {
            if (ConstellationsList.Count < index+1)
                return "default";
            else
                return ConstellationsList[index];
        }

        //iterate through extendedLevel list
        internal static void GetLLLStuffForConfig(List<ExtendedLevel> extendedLevels, List<string> ignoreList, string defaultValue)
        {
            foreach (ExtendedLevel extendedLevel in extendedLevels)
            {
                if (ignoreList.Contains(extendedLevel.NumberlessPlanetName.ToLower()))
                    continue;

                ConfigEntry<string> levelToConstellation = MakeString(Configuration.GeneratedConfig, "Moons", $"{extendedLevel.NumberlessPlanetName} Constellation", defaultValue, $"Specify which constellation {extendedLevel.NumberlessPlanetName} belongs to\nShould match an item from [ConstellationList]");

                ConstellationsToMoons.Add(extendedLevel.NumberlessPlanetName, levelToConstellation.Value);

                ConfigEntry<int> levelPrice = MakeClampedInt(Configuration.GeneratedConfig, "Moons", $"{extendedLevel.NumberlessPlanetName} Price", extendedLevel.RoutePrice, "Set a custom route price for this moon (should autopopulate with the correct default price)", 0, 9999);

                MoonPrices.Add(extendedLevel.NumberlessPlanetName, levelPrice.Value);
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
        internal static string GetFirstMatch(Dictionary<string, string> listing, string query)
        {
            if (listing.Count == 0)
                return "";

            //level,constellation
            foreach(KeyValuePair<string,string> item in listing)
            {
                if (item.Value == query)
                {
                    return item.Key;
                }
            }

            return "";
        }
    }
}
