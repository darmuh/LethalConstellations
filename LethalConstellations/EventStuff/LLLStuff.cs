using BepInEx.Configuration;
using LethalConstellations.Compat;
using LethalConstellations.ConfigManager;
using LethalConstellations.PluginCore;
using LethalLevelLoader;
using OpenLib.Common;
using System.Collections.Generic;
using System.Linq;
using static LethalConstellations.PluginCore.Collections;
using static OpenLib.ConfigManager.ConfigSetup;
using static OpenLib.ConfigManager.WebHelper;
using Random = System.Random;


namespace LethalConstellations.EventStuff
{
    internal class LLLStuff
    {
        internal static bool usingTags = false;
        public static void LLLSetup()
        {
            Start();

            if (Configuration.ConstellationList.Value.Length < 1)
                ConstellationsList = GetDefaultConsellations();
            else
                ConstellationsList = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.ConstellationList.Value, ',');

            Plugin.Spam("ConstellationList:");
            foreach (string item in ConstellationsList)
                Plugin.Spam(item);

            List<string> ignoreList = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.IgnoreList.Value, ',');
            ignoreList = ignoreList.ConvertAll(s => s.ToLower());
            Plugin.Spam("list created");

            foreach (string name in ConstellationsList)
            {
                ConfigEntry<string> menuText = MakeString(Configuration.GeneratedConfig, $"{ConstellationWord} {name}", $"{name} menuText", $"Route to {ConstellationWord} [name][~t]$[price][~n]Default Moon: [defaultmoon] [currentweather] [optionals]", $"The text displayed for this {ConstellationWord}'s menu item");

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
            //LConfig.QueueConfig(LethalLevelLoader.Tools.ConfigLoader.configFile);
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

        internal static string GetDefaultCName(List<string> constList)
        {
            if (constList.Count < 1)
                return "default";
            else
            {
                Random rand = new();
                int index = rand.Next(0, constList.Count);
                return constList[index];
            }
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
            string fail = GetDefaultCName(constList);

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

                ConfigEntry<bool> stayHiding = MakeBool(Configuration.GeneratedConfig, "Moons", $"{extendedLevel.NumberlessPlanetName} Stay Hidden", extendedLevel.IsRouteHidden, $"Set this to true to keep {extendedLevel.NumberlessPlanetName} hidden even when you're in it's {ConstellationWord}");

                if (usingTags)
                {
                    string tagConstellation = GetTagInfo(extendedLevel, ConstellationsList);
                    ConfigEntry<string> levelToConstellation = MakeClampedString(Configuration.GeneratedConfig, "Moons", $"{extendedLevel.NumberlessPlanetName} {ConstellationWord}", tagConstellation, $"Specify which {ConstellationWord} {extendedLevel.NumberlessPlanetName} belongs to.\nClamped to what is set in [ConstellationList] (default listing)", new AcceptableValueList<string>([.. ConstellationsList]));
                    AddToConstelMoons(extendedLevel.NumberlessPlanetName, levelToConstellation.Value, stayHiding.Value);
                }
                else
                {
                    ConfigEntry<string> levelToConstellation = MakeString(Configuration.GeneratedConfig, "Moons", $"{extendedLevel.NumberlessPlanetName} {ConstellationWord}", defaultValue, $"Specify which {ConstellationWord} {extendedLevel.NumberlessPlanetName} belongs to.\nShould match an item from [ConstellationList]\nIf adding to multiple {ConstellationsWord}, separate each {ConstellationWord} by a comma.\nWill be autoset to a random {ConstellationWord} if not matching one.");

                    if (levelToConstellation.Value.Contains(","))
                    {
                        List<string> constellationList = CommonStringStuff.GetKeywordsPerConfigItem(levelToConstellation.Value, ',');
                        foreach (string conName in constellationList)
                        {
                            AddToConstelMoons(extendedLevel.NumberlessPlanetName, conName, stayHiding.Value);
                        }
                    }
                    else
                    {
                        if (ConstellationsList.Any(c => c.ToLower() == levelToConstellation.Value.ToLower()))
                            AddToConstelMoons(extendedLevel.NumberlessPlanetName, levelToConstellation.Value, stayHiding.Value);
                        else
                        {
                            Random rand = new();
                            int chosen = rand.Next(0, ConstellationsList.Count);
                            levelToConstellation.Value = ConstellationsList[chosen];
                            AddToConstelMoons(extendedLevel.NumberlessPlanetName, levelToConstellation.Value, stayHiding.Value);
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

        //check for configitem, no out
        internal static bool CheckForConfigName(string configName)
        {
            foreach (ConfigDefinition item in Plugin.instance.Config.Keys)
            {
                if (item.Key == configName)
                    return true;
            }

            return false;
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
    }
}
