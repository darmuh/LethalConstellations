using BepInEx.Configuration;
using LethalConstellations.Compat;
using LethalConstellations.ConfigManager;
using LethalConstellations.PluginCore;
using LethalLevelLoader;
using OpenLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static LethalConstellations.PluginCore.Collections;
using static OpenLib.ConfigManager.ConfigSetup;
using Random = System.Random;


namespace LethalConstellations.EventStuff
{
    internal class LLLStuff
    {
        private static List<ClassMapper> constellations = new List<ClassMapper>();
        internal static bool usingTags = false;
        public static void LLLSetup()
        {
            Start();

            if (Configuration.ConstellationList.Value.Length < 1)
                ConstellationsList = GetDefaultConsellations();
            else
            {
                ConstellationsList = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.ConstellationList.Value, ',');
                ConstellationsList.RemoveAll(x => x.Length < 1);

                if (Configuration.ManualSetupListing.Value.Length > 0)
                {
                    List<string> pairs = [.. Configuration.ManualSetupListing.Value.Split(';')];
                    
                    foreach (string item in pairs)
                    {
                        List<string> items = [.. item.Split(':')];
                        string keyVal = "FailedToParseConsName";
                        for (int x = 0; x < items.Count; x++)
                        {
                            if (!items[x].Contains(','))
                            {
                                keyVal = items[x];
                                continue;
                            }

                            List<string> allValues = [.. items[x].Split(',')];
                            for (int i = 0; i < allValues.Count; i++)
                            {
                                //allvalues are moons, keyVal should be last parsed constellation name
                                ManualSetupList.Add(allValues[i], keyVal); //moon, constellation
                            }
                        }
                        
                    }


                }
            }

            if (ConstellationsList.Count != ConstellationsList.Distinct(StringComparer.CurrentCultureIgnoreCase).Count())
                Plugin.WARNING($"REMOVING DUPLICATE CONSTELLATION NAMES!!\nOriginal [ {ConstellationsList.Count} ]\nDistinct [ {ConstellationsList.Distinct(StringComparer.CurrentCultureIgnoreCase).Count()} ]");

            ConstellationsList = [.. ConstellationsList.Distinct(StringComparer.CurrentCultureIgnoreCase)]; //remove duplicates that would throw errors

            Plugin.Spam("ConstellationList:");
            foreach (string item in ConstellationsList)
                Plugin.Spam(item);

            Plugin.Spam("ManualSetupList:");
            foreach(KeyValuePair<string,string> pair in ManualSetupList)
                Plugin.Spam($"{pair.Key} - {pair.Value}");

            List<string> ignoreList = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.IgnoreList.Value, ',');
            ignoreList = ignoreList.ConvertAll(s => s.ToLower());
            Plugin.Spam("ignoreList created");

            foreach (string name in ConstellationsList)
            {

                    ConfigEntry<string> menuText = MakeString(Configuration.GeneratedConfig, $"{ConstellationWord} {name}", $"{name} menuText", $"Route to {ConstellationWord} [name][~t]$[price][~n]Default Moon: [defaultmoon] [currentweather][~n][lightyears] light years away [optionals]", $"The text displayed for this {ConstellationWord}'s menu item");

                    ConfigEntry<string> shortCuts = MakeString(Configuration.GeneratedConfig, $"{ConstellationWord} {name}", $"{name} shortcuts", "", $"Specify a list of shortcuts to use for routing to the {name} {ConstellationWord}.\nEach shortcut keyword is separated by a ','");

                    ConfigEntry<bool> isHiding = MakeBool(Configuration.GeneratedConfig, $"{ConstellationWord} {name}", $"{name} isHidden", false, $"Enable this to hide this {ConstellationWord} from the constellation listing");

                    ConfigEntry<bool> canGoCompany = MakeBool(Configuration.GeneratedConfig, $"{ConstellationWord} {name}", $"{name} canRouteCompany", true, $"Enable this to allow this {ConstellationWord} to route to the company moon");

                    ConfigEntry<bool> buyOnce = MakeBool(Configuration.GeneratedConfig, $"{ConstellationWord} {name}", $"{name} One-Time Purchase", false, $"Enable this to allow routing to this {ConstellationWord} for free after paying for it once");

                    ClassMapper constClass = new(name);
                    constClass.menuText = menuText.Value;
                    constClass.isHidden = isHiding.Value;
                    constClass.canRouteCompany = canGoCompany.Value;
                    constClass.shortcutList = shortCuts.Value;
                    constClass.buyOnce = buyOnce.Value;

                    if (Configuration.ConstellationSpecificInfoNodes.Value)
                    {
                        ConfigEntry<string> infoText = MakeString(Configuration.GeneratedConfig, $"{ConstellationWord} {name}", $"{name} infoText", $"{ConstellationWord} - {name}\n\n\nThis [ConstellationWord] contains moons in it. Route to it and find out which!\r\n\r\n", $"The text that displays with the info command for this {ConstellationWord}");
                        if (infoText.Value.Contains("[ConstellationWord]"))
                            infoText.Value = infoText.Value.Replace("[ConstellationWord]", ConstellationWord);
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
            if (!OpenLib.Plugin.instance.LethalConfig)
                return;

            LConfig.QueueConfig(Configuration.GeneratedConfig);
        }

        internal static List<string> GetDefaultConsellations()
        {
            List<string> fail = ["Alpha", "Bravo", "Charlie"];
            List<string> ignore = ["safe", "corruption detected", "???"];
            List<string> tagsfromLLL = [];
            if (PatchedContent.ExtendedLevels.Count < 1)
                return fail;

            foreach (ExtendedLevel level in PatchedContent.ExtendedLevels)
            {
                Plugin.Spam($"---------------- Checking {level.NumberlessPlanetName} tags ----------------");

                string constellation;
                if (!Configuration.ConstellationsUseFauxWords.Value)
                {
                    if (ignore.Contains(level.SelectableLevel.riskLevel.ToLower()))
                        continue;

                    constellation = $"{level.SelectableLevel.riskLevel} Tier";
                }
                else
                    constellation = GetFirstUniqueTag(level);

                if (!tagsfromLLL.Contains(constellation))
                {
                    tagsfromLLL.Add(constellation);
                }

                Plugin.Spam("---------------- End of checks ----------------");
            }

            usingTags = true;
            return tagsfromLLL;
        }

        internal static string GetDefaultCName(List<string> constList, string levelName = "")
        {
            if (constList.Count < 1)
                return "default";
            else if(ManualSetupList.Count > 0 && levelName.Length > 0) //moon,constellation
            {
                Plugin.Spam($"Attempting to get MANUAL constellation setup for [ {levelName} ]");
                if (ManualSetupList.TryGetValue(levelName.ToLower(), out string consName))
                    return consName;
                else
                    return IndexRandom(constList);
            }
            else
                return IndexRandom(constList);
        }

        private static string IndexRandom(List<string> listing)
        {
            Plugin.Spam($"Setting IndexRandom string from given listing!");
            int index = Rand.Next(0, listing.Count);
            return listing[index];
        }

        private static bool DoesLevelHaveTag(ExtendedLevel level, string query)
        {
            foreach (ContentTag tag in level.ContentTags)
            {
                if (tag.contentTagName.ToLower() == query.ToLower())
                    return true;
            }

            return false;
        }

        private static string GetFirstUniqueTag(ExtendedLevel level)
        {
            List<string> ignore = ["free", "paid", "custom", "vanilla", "company"];

            foreach (ContentTag tag in level.ContentTags)
            {
                if (ignore.Contains(tag.contentTagName.ToLower())) //ignore above list items
                    continue;
                if(tag.contentTagName.Length < 3) //ensure it meets the minimum for fauxkeywords
                    continue;
                if(tag.contentTagName.Contains(' ')) //skip tags with spaces
                    continue;

                return tag.contentTagName;
            }

            return "Unknown";
        }

        internal static string GetTagInfo(ExtendedLevel level, List<string> constList)
        {
            string fail = GetDefaultCName(constList, level.NumberlessPlanetName);

            foreach (string constel in constList)
            {
                if (Configuration.ConstellationsUseFauxWords.Value)
                {
                    if (DoesLevelHaveTag(level, constel))
                        return constel;
                }
                else
                {
                    if (constel.Contains(level.SelectableLevel.riskLevel))
                        return constel;
                }
            }

            if (constList.Contains("Unknown Tier"))
                return "Unknown Tier";

            return fail;
        }

        internal static void SetDefaultMoon(List<ClassMapper> allConstell)
        {
            Plugin.Spam("Getting Default Moons/Prices");
            foreach (ClassMapper constel in allConstell)
            {
                Plugin.Spam($"Setting defaults for {constel.consName}");

                string defMoon = GetRandomDefault(constel);
                int defPrice = GetMoonPrice(defMoon);
                ConfigEntry<string> defaultMoon;

                Plugin.Spam($"constelMoons - {constel.constelMoons.Count}");

                //not making clamped string to avoid issues with web config creation

                defaultMoon = MakeString(Configuration.GeneratedConfig, $"{ConstellationWord} {constel.consName}", $"{constel.consName} defaultMoon", defMoon, $"Default moon to route to when selecting this {ConstellationWord}");

                constel.defaultMoon = defaultMoon.Value;
                Plugin.Spam($"Default Moon for {constel.consName} set to {defaultMoon.Value}");

                constel.defaultMoonLevel = MoonStuff.GetExtendedLevel(constel.defaultMoon);

                if (constel.defaultMoonLevel == null)
                {
                    Plugin.WARNING("defaultMoonLevel was NULL due to invalid config item.\n\nSetting default moon to new random and updating config item!");
                    string newDef = GetRandomDefault(constel);
                    constel.defaultMoon = newDef;
                    defaultMoon.Value = newDef;
                    constel.defaultMoonLevel = MoonStuff.GetExtendedLevel(newDef);

                }

                ConfigEntry<int> constellationPrice = MakeClampedInt(Configuration.GeneratedConfig, $"{ConstellationWord} {constel.consName}", $"{constel.consName} constellationPrice", defPrice, $"Set the price to route to this {ConstellationWord} and it's defaultMoon", 0, 9999);
                ConfigEntry<int> Distance = MakeClampedInt(Configuration.GeneratedConfig, $"{ConstellationWord} {constel.consName}", $"{constel.consName} constellationDistance", defPrice, $"Set the distance in light-years from the starting {ConstellationWord} to this {ConstellationWord} to route to its defaultMoon", 0, 9999);

                constel.Distance = Distance.Value;
                constel.constelPrice = constellationPrice.Value;

            }

        }

        //iterate through extendedLevel list
        internal static void GetLLLStuffForConfig(List<ExtendedLevel> extendedLevels, List<string> ignoreList)
        {
            foreach (ExtendedLevel extendedLevel in extendedLevels)
            {
                if(extendedLevel == null) //skip null extendedLevel (this should never happen but just in case lol)
                    continue;

                string moonName = BepinFriendlyString(extendedLevel.NumberlessPlanetName);
                Plugin.Spam($"moonName is {moonName}");

                if (moonName.Length < 1)
                    continue;

                if (ignoreList.Contains(moonName.ToLower())) //ignore moons specified by user config
                    continue;

                if (moonName.ToLower() == CompanyMoon.ToLower()) //ignore company moon
                    continue;

                string defaultValue = GetDefaultCName(ConstellationsList, moonName);
                Plugin.Spam($"{moonName} default constellation set to - " + defaultValue);

                ConfigEntry<int> levelPrice = MakeClampedInt(Configuration.GeneratedConfig, "Moons", $"{moonName} Price", extendedLevel.RoutePrice, "Set a custom route price for this moon (should autopopulate with the correct default price)", 0, 99999);

                ConfigEntry<bool> stayHiding = MakeBool(Configuration.GeneratedConfig, "Moons", $"{moonName} Stay Hidden", extendedLevel.IsRouteHidden, $"Set this to true to keep {moonName} hidden even when you're in it's {ConstellationWord}");

                if (usingTags)
                {
                    string tagConstellation = GetTagInfo(extendedLevel, ConstellationsList);
                    ConfigEntry<string> levelToConstellation = MakeClampedString(Configuration.GeneratedConfig, "Moons", $"{moonName} {ConstellationWord}", tagConstellation, $"Specify which {ConstellationWord} {moonName} belongs to.\nClamped to what is set in [ConstellationList] (default listing)", new AcceptableValueList<string>([.. ConstellationsList]));
                    AddToConstelMoons(moonName, levelToConstellation.Value, stayHiding.Value);
                }
                else
                {
                    ConfigEntry<string> levelToConstellation = MakeString(Configuration.GeneratedConfig, "Moons", $"{moonName} {ConstellationWord}", defaultValue, $"Specify which {ConstellationWord} {moonName} belongs to.\nShould match an item from [ConstellationList]\nIf adding to multiple {ConstellationsWord}, separate each {ConstellationWord} by a comma.\nWill be autoset to a random {ConstellationWord} if not matching one.");

                    if (levelToConstellation.Value.Contains(","))
                    {
                        List<string> constellationList = CommonStringStuff.GetKeywordsPerConfigItem(levelToConstellation.Value, ',');
                        foreach (string conName in constellationList)
                        {
                            AddToConstelMoons(moonName, conName, stayHiding.Value);
                        }
                    }
                    else
                    {
                        if (ConstellationsList.Any(c => c.ToLower() == levelToConstellation.Value.ToLower()))
                            AddToConstelMoons(moonName, levelToConstellation.Value, stayHiding.Value);
                        else
                        {
                            int chosen = Rand.Next(0, ConstellationsList.Count);
                            levelToConstellation.Value = ConstellationsList[chosen];
                            AddToConstelMoons(moonName, levelToConstellation.Value, stayHiding.Value);
                        }
                    }

                }

                MoonPrices.Add(extendedLevel, levelPrice.Value);
            }
        }

        internal static void AddToConstelMoons(string newMoon, string cName, bool stayHidden)
        {
            if (ConstellationStuff.Count < 0)
                return;

            foreach (ClassMapper constel in ConstellationStuff)
            {
                if (constel.consName == cName && !constel.constelMoons.Contains(newMoon))
                {
                    constel.constelMoons.Add(newMoon);
                    if (stayHidden)
                        constel.stayHiddenMoons.Add(newMoon);
                    Plugin.Spam($"adding {newMoon} to {cName} / stayHidden: {stayHidden}");
                }
            }
        }

        internal static int GetMoonPrice(string moonName)
        {
            if (MoonPrices.Count < 1)
                return 0;

            foreach (KeyValuePair<ExtendedLevel, int> moon in MoonPrices)
            {
                if (moon.Key.NumberlessPlanetName.ToLower() == moonName.ToLower())
                    return moon.Value;
            }

            return 0;
        }

        //set defaultMoon for a constellation
        internal static string GetRandomDefault(ClassMapper constellation)
        {
            if (constellation.constelMoons.Count == 0)
                return "";

            Random ran = new();
            int rand = ran.Next(0, constellation.constelMoons.Count);
            return constellation.constelMoons[rand];
        }

        internal static string BepinFriendlyString(string input)
        {
            char[] invalidChars = ['\'', '\n', '\t', '\\', '"', '[', ']'];
            string result = "";

            foreach (char c in input)
            {
                if (!invalidChars.Contains(c))
                    result += c;
                else
                    continue;
            }

            return result;
        }
    }
}
