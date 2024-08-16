using BepInEx.Configuration;
using LethalConstellations.ConfigManager;
using LethalConstellations.PluginCore;
using LethalLevelLoader;
using OpenLib.Common;
using System.Collections.Generic;
using static OpenLib.ConfigManager.ConfigSetup;
using static LethalConstellations.PluginCore.Collections;
using Random = UnityEngine.Random;
using UnityEngine;
using LethalConstellations.Compat;
using System;


namespace LethalConstellations.EventStuff
{
    internal class LLLStuff
    {
        internal static bool usingRiskTags = false;
        public static void LLLSetup()
        {
            Start();

            if (Configuration.ConstellationList.Value.Length < 1)
                ConstellationsList = GetDefaultConsellations();
            else
                ConstellationsList = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.ConstellationList.Value, ',');

            Plugin.Spam("ConstellationList:");
            foreach(string item in ConstellationsList)
                Plugin.Spam(item);

            List<string> ignoreList = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.IgnoreList.Value, ',');
            ignoreList = ignoreList.ConvertAll(s  => s.ToLower());
            Plugin.Spam("list created");

            foreach (string name in ConstellationsList)
            {
                ConfigEntry<string> menuText = MakeString(Configuration.GeneratedConfig, $"Constellation {name}", $"{name} menuText", $"Route to {ConstellationWord} [name][~t]$[price][~n]Default Moon: [defaultmoon] [currentweather] [optionals]", "text displayed for this constellation's menu item");

                ConfigEntry<string> shortCuts = MakeString(Configuration.GeneratedConfig, $"Constellation {name}", $"{name} shortcuts", "", $"Specify a list of shortcuts to use for routing to the {name} constellation.\nEach shortcut keyword is separated by a ','");

                ConfigEntry<bool> isHiding = MakeBool(Configuration.GeneratedConfig, $"Constellation {name}", $"{name} isHidden", false, "Enable this to hide this constellation from the constellation listing");

                ConfigEntry<bool> canGoCompany = MakeBool(Configuration.GeneratedConfig, $"Constellation {name}", $"{name} canRouteCompany", true, "Enable this to allow this constellation to route to the company moon");

                ConfigEntry<bool> buyOnce = MakeBool(Configuration.GeneratedConfig, $"Constellation {name}", $"{name} One-Time Purchase", false, "Enable this to allow routing to this constellation for free after paying for it once");

                ClassMapper constClass = new(name);
                constClass.menuText = menuText.Value;
                constClass.isHidden = isHiding.Value;
                constClass.canRouteCompany = canGoCompany.Value;
                constClass.shortcutList = shortCuts.Value;
                constClass.buyOnce = buyOnce.Value;

                if (Configuration.ConstellationSpecificInfoNodes.Value)
                {
                    ConfigEntry<string> infoText = MakeString(Configuration.GeneratedConfig, $"Constellation {name}", $"{name} infoText", $"ConstellationWord - {name}\n\n\nThis constellation contains moons in it. Route to it and find out which!\r\n\r\n", "text that displays with the info command for this constellation");
                    constClass.infoText = infoText.Value;
                }

                constClass.constelMoons = [];
                constClass.stayHiddenMoons = [];
                ConstellationStuff.Add(constClass);
            }

            Plugin.Spam("about to sort through extendedlevel");
            GetLLLStuffForConfig(PatchedContent.ExtendedLevels, ignoreList);

            SetDefaultMoon(ConstellationStuff);

            Plugin.Spam($"ConfigCount: {Configuration.GeneratedConfig.Count}");
            Configuration.GeneratedConfig.Save();
            RemoveOrphanedEntries(Configuration.GeneratedConfig);
            LethalConfigStuff();
        }

        internal static void LethalConfigStuff()
        {
            if(Plugin.instance.LethalConfig)
                LethalConfigCompat.QueueConfig(Configuration.GeneratedConfig);
        }

        internal static List<string> GetDefaultConsellations()
        {
            List<string> fail = new() { "Alpha", "Bravo", "Charlie" };
            List<string> ignore = new() { "safe", "corruption detected", "???"};
            List<string> tagsfromLLL = new();
            if(PatchedContent.ExtendedLevels.Count < 1)
                return fail;

            foreach (ExtendedLevel level in PatchedContent.ExtendedLevels)
            {
                Plugin.Spam($"---------------- Checking {level.NumberlessPlanetName} tags ----------------");

                if (ignore.Contains(level.SelectableLevel.riskLevel.ToLower()))
                    continue;

                string constellation = $"{level.SelectableLevel.riskLevel} Tier";

                if (!tagsfromLLL.Contains(constellation))
                {
                    tagsfromLLL.Add(constellation);
                }
                        
                Plugin.Spam("---------------- End of checks ----------------");
            }

            usingRiskTags = true;
            return tagsfromLLL;
        }

        internal static string GetDefaultCName(List<string> constList)
        {
            if (constList.Count < 1)
                return "default";
            else
            {
                int index = Random.Range(0, constList.Count - 1);
                return constList[index];
            }       
        }

        internal static string GetLevelRisk(ExtendedLevel level, List<string> constList)
        {
            string fail = GetDefaultCName(constList);

            //List<string> ignore = new() { "company", "free", "paid", "custom" };

            foreach (string constel in constList)
            {
                if (constel.Contains(level.SelectableLevel.riskLevel))
                    return constel;
            }

            if (constList.Contains("Unknown Tier"))
                return "Unknown Tier";

            return fail;
        }

        internal static void SetDefaultMoon(List<ClassMapper> allConstell)
        {
            Plugin.Spam("Getting Default Moons/Prices");
            foreach(ClassMapper constel in allConstell)
            {
                Plugin.Spam($"Setting defaults for {constel.consName}");

                string defMoon = GetRandomDefault(constel);
                int defPrice = GetMoonPrice(defMoon);

                ConfigEntry<string> defaultMoon = MakeString(Configuration.GeneratedConfig, $"Constellation {constel.consName}", $"{constel.consName} defaultMoon", defMoon, "default moon to route to when selecting this constellation");
                constel.defaultMoon = defaultMoon.Value;
                Plugin.Spam($"Default Moon for {constel.consName} set to {defaultMoon.Value}");

                constel.defaultMoonLevel = MoonStuff.GetExtendedLevel(constel.defaultMoon);

                ConfigEntry<int> constellationPrice = MakeClampedInt(Configuration.GeneratedConfig, $"Constellation {constel.consName}", $"{constel.consName} constellationPrice", defPrice, $"Set the price to route to this constellation and it's defaultMoon", 0, 9999);

                constel.constelPrice = constellationPrice.Value;

            }

        }

        //iterate through extendedLevel list
        internal static void GetLLLStuffForConfig(List<ExtendedLevel> extendedLevels, List<string> ignoreList)
        {
            foreach (ExtendedLevel extendedLevel in extendedLevels)
            {
                string defaultValue = GetDefaultCName(ConstellationsList);
                Plugin.Spam(defaultValue);

                if (ignoreList.Contains(extendedLevel.NumberlessPlanetName.ToLower()))
                    continue;

                Plugin.Spam("not in ignoreList");

                if (extendedLevel.NumberlessPlanetName.ToLower() == CompanyMoon.ToLower())
                    continue;

                Plugin.Spam("not the company moon");

                ConfigEntry<int> levelPrice = MakeClampedInt(Configuration.GeneratedConfig, "Moons", $"{extendedLevel.NumberlessPlanetName} Price", extendedLevel.RoutePrice, "Set a custom route price for this moon (should autopopulate with the correct default price)", 0, 99999);

                ConfigEntry<bool> stayHiding = MakeBool(Configuration.GeneratedConfig, "Moons", $"{extendedLevel.NumberlessPlanetName} Stay Hidden", extendedLevel.IsRouteHidden, $"Set this to true to keep {extendedLevel.NumberlessPlanetName} hidden even when you're in it's constellation");

                if (usingRiskTags)
                {
                    string tagConstellation = GetLevelRisk(extendedLevel, ConstellationsList);
                    ConfigEntry<string> levelToConstellation = MakeString(Configuration.GeneratedConfig, "Moons", $"{extendedLevel.NumberlessPlanetName} Constellation", tagConstellation, $"Specify which constellation {extendedLevel.NumberlessPlanetName} belongs to\nShould match an item from [ConstellationList]");
                    AddToConstelMoons(extendedLevel.NumberlessPlanetName, levelToConstellation.Value, stayHiding.Value);
                }
                else
                {
                    ConfigEntry<string> levelToConstellation = MakeString(Configuration.GeneratedConfig, "Moons", $"{extendedLevel.NumberlessPlanetName} Constellation", defaultValue, $"Specify which constellation {extendedLevel.NumberlessPlanetName} belongs to\nShould match an item from [ConstellationList]\nIf adding to multiple constellations, separate each constellation by a comma");

                    if (levelToConstellation.Value.Contains(","))
                    {
                        List<string> constellationList = CommonStringStuff.GetKeywordsPerConfigItem(levelToConstellation.Value, ',');
                        foreach(string conName in constellationList)
                        {
                            AddToConstelMoons(extendedLevel.NumberlessPlanetName, conName, stayHiding.Value);
                        }    
                    }
                    else
                        AddToConstelMoons(extendedLevel.NumberlessPlanetName, levelToConstellation.Value, stayHiding.Value);
                }

                MoonPrices.Add(extendedLevel, levelPrice.Value);
            }
        }

        internal static void AddToConstelMoons(string newMoon, string cName, bool stayHidden)
        {
            if (ConstellationStuff.Count < 0)
                return;

            foreach(ClassMapper constel in ConstellationStuff)
            {
                if(constel.consName == cName && !constel.constelMoons.Contains(newMoon))
                {
                    constel.constelMoons.Add(newMoon);
                    if(stayHidden)
                        constel.stayHiddenMoons.Add(newMoon);
                    Plugin.Spam($"adding {newMoon} to {cName} / stayHidden: {stayHidden}");
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

        internal static int GetMoonPrice(string moonName)
        {
            if (MoonPrices.Count < 1)
                return 0;

            foreach (KeyValuePair<ExtendedLevel, int> moon in MoonPrices)
            {
                if(moon.Key.NumberlessPlanetName.ToLower() == moonName.ToLower())
                    return moon.Value;
            }

            return 0;
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
