using BepInEx.Configuration;
using HarmonyLib;
using LethalConstellations.Compat;
using LethalConstellations.ConfigManager;
using LethalConstellations.PluginCore;
using LethalLevelLoader;
using OpenLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static LethalConstellations.PluginCore.Collections;
using static OpenLib.ConfigManager.ConfigSetup;
using Random = System.Random;


namespace LethalConstellations.EventStuff
{
    internal class LLLStuff
    {
        internal static bool usingTags = false;

        internal static List<string> GetConstellations()
        {
            if (Configuration.ConstellationList.Value.Length < 1)
                return GetDefaultConsellations();
            else
            {
                List<string> constellations = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.ConstellationList.Value, ',');
                constellations.RemoveAll(x => x.Length < 1);
                return constellations;
            }
        }

        internal static void InitManualSetup()
        {
            ManualSetupList = [];

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

        public static void LLLSetup()
        {
            Plugin.Spam("LLLSetup has started!");
            Start();
            ConstellationsList = GetConstellations();

            if (ConstellationsList.Count != ConstellationsList.Distinct(StringComparer.CurrentCultureIgnoreCase).Count())
                Plugin.WARNING($"REMOVING DUPLICATE CONSTELLATION NAMES!!\nOriginal [ {ConstellationsList.Count} ]\nDistinct [ {ConstellationsList.Distinct(StringComparer.CurrentCultureIgnoreCase).Count()} ]");

            ConstellationsList = [.. ConstellationsList.Distinct(StringComparer.CurrentCultureIgnoreCase)]; //remove duplicates that would throw errors

            Plugin.Spam("ConstellationList:");
            ConstellationsList.Do(x => Plugin.Spam(x));

            Plugin.Spam("ManualSetupList:");
            ManualSetupList.Do(x => Plugin.Spam($"{x.Key} - {x.Value}"));

            List<string> ignoreList = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.IgnoreList.Value, ',');
            ignoreList = ignoreList.ConvertAll(s => s.ToLower());
            Plugin.Spam("ignoreList created");

            ConstellationsList.Do(x => MapConstellation(x));

            Plugin.Spam("about to sort through extendedlevel");
            PatchedContent.ExtendedLevels.Do(x=> MapExtendedLevel(x, ignoreList));

            SetDefaultMoon(ConstellationStuff);

            Plugin.Spam($"ConfigCount: {Configuration.GeneratedConfig.Count}");
            Configuration.GeneratedConfig.Save();

            RemoveOrphanedEntries(Configuration.GeneratedConfig);
            LethalConfigStuff();
        }

        internal static void MapConstellation(string name)
        {
            string fixedName = CommonStringStuff.BepinFriendlyString(name);
            ConfigEntry<string> menuText = MakeString(Configuration.GeneratedConfig, $"{ConstellationWord} {fixedName}", $"{fixedName} menuText", $"Route to System $[price] [name][~n]Default Moon:[defaultmoon] [currentweather][~n][currentdistance] [optionals]", $"The text displayed for this {ConstellationWord}'s menu item\n[price] will display price information\n[name] will display the constellation name\n[~n] will create a new line\n[~t] will create a tab indent\n[defaultmoon] will display a constellation's default moon\n[currentweather] will display a moons current weather (retrieved from LLL)\n[currentdistance] will display the current distance value determined by positional data\n[optionals] will allow for other mods to add their own flavor text to this menu item.");

            ConfigEntry<string> shortCuts = MakeString(Configuration.GeneratedConfig, $"{ConstellationWord} {fixedName}", $"{fixedName} shortcuts", "", $"Specify a list of shortcuts to use for routing to the {fixedName} {ConstellationWord}.\nEach shortcut keyword is separated by a ','");

            ConfigEntry<bool> isHiding = MakeBool(Configuration.GeneratedConfig, $"{ConstellationWord} {fixedName}", $"{fixedName} isHidden", false, $"Enable this to hide this {ConstellationWord} from the constellation listing");

            ConfigEntry<bool> canGoCompany = MakeBool(Configuration.GeneratedConfig, $"{ConstellationWord} {fixedName}", $"{fixedName} canRouteCompany", true, $"Enable this to allow this {ConstellationWord} to route to the company moon");

            ConfigEntry<bool> buyOnce = MakeBool(Configuration.GeneratedConfig, $"{ConstellationWord} {fixedName}", $"{fixedName} One-Time Purchase", false, $"Enable this to allow routing to this {ConstellationWord} for free after paying for it once");

            ClassMapper constClass = new(name)
            {
                menuText = menuText.Value,
                isHidden = isHiding.Value,
                canRouteCompany = canGoCompany.Value,
                shortcutList = CommonStringStuff.GetKeywordsPerConfigItem(shortCuts.Value, ','),
                buyOnce = buyOnce.Value
            };

            if (Configuration.ConstellationSpecificInfoNodes.Value)
            {
                ConfigEntry<string> infoText = MakeString(Configuration.GeneratedConfig, $"{ConstellationWord} {fixedName}", $"{fixedName} infoText", $"{ConstellationWord} - {fixedName}\n\n\nThis [ConstellationWord] contains moons in it. Route to it and find out which!\r\n\r\n", $"The text that displays with the info command for this {ConstellationWord}'s shortcut keywords");
                if (infoText.Value.Contains("[ConstellationWord]"))
                    infoText.Value = infoText.Value.Replace("[ConstellationWord]", ConstellationWord);
                constClass.infoText = infoText.Value;
            }

            constClass.constelMoons = [];
            constClass.stayHiddenMoons = [];
            ConstellationStuff.Add(constClass);
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
            List<string> tagsfromLLL = [];
            if (PatchedContent.ExtendedLevels.Count < 1)
                return fail;

            foreach (ExtendedLevel level in PatchedContent.ExtendedLevels)
            {
                Plugin.Spam($"---------------- Checking {level.NumberlessPlanetName} tags ----------------");

                string constellation = GetFirstUniqueTag(level);

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
            else if (ManualSetupList.Count > 0 && levelName.Length > 0) //moon,constellation
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
            return level.ContentTags.Any(tag => tag.contentTagName.ToLower() == query.ToLower());
        }

        private static string GetFirstUniqueTag(ExtendedLevel level)
        {
            List<string> ignore = ["free", "paid", "custom", "vanilla", "company"];

            foreach (ContentTag tag in level.ContentTags)
            {
                if (ignore.Contains(tag.contentTagName.ToLower())) //ignore above list items
                    continue;
                if (tag.contentTagName.Length < 3) //ensure it meets the minimum for fauxkeywords
                    continue;
                if (tag.contentTagName.Contains(' ')) //skip tags with spaces
                    continue;

                return tag.contentTagName;
            }

            return "Unknown";
        }

        internal static string GetTagInfo(ExtendedLevel level, List<string> constList)
        {
            string fail = GetDefaultCName(constList, level.NumberlessPlanetName);

            if (constList.Any(constel => DoesLevelHaveTag(level, constel)))
                return constList.Find(constel => DoesLevelHaveTag(level, constel));

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

                // Not making clamped string to avoid issues with web config creation
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

                ConfigEntry<int> constellationPrice = MakeClampedInt(Configuration.GeneratedConfig, $"{ConstellationWord} {constel.consName}", $"{constel.consName} constellationPrice", defPrice, $"Set the price to route to this {ConstellationWord} and its defaultMoon", 0, 9999);
                constel.constelPrice = constellationPrice.Value;

                if (Configuration.AddConstellationPositionData.Value)
                {
                    ConfigEntry<string> postionalPricingMode = MakeClampedString(Configuration.GeneratedConfig, $"{ConstellationWord} {constel.consName}", $"{constel.consName} PostionalPricingMode", "SetPriceByDistance", "Determine how pricing for this constellation will be affected by positional data.\nUseOriginalPrice will use constellationPrice configuration item as starting route cost and the CostPerDistanceUnit value will not be used.\nSetPriceByDistance will ignore constellationPrice and set constellation's price to a new price value based on it's starting position (relative to the starter constellation).\nNone will keep from adjusting constellations based on distance when AddConstellationPositionData is enabled", new AcceptableValueList<string>("UseOriginalPrice", "SetPriceByDistance", "None"));
                    constel.positionalPriceMode = postionalPricingMode.Value;
                }


                ConfigEntry<string> constellationPosition = MakeString(Configuration.GeneratedConfig, $"{ConstellationWord} {constel.consName}", $"{constel.consName} constellationPosition", $"{Rand.Next(0, 200)}, {Rand.Next(0, 200)}, {Rand.Next(0, 200)}", $"Set the relative position of {constel.consName} in space\n0 will be considered the center of the universe, will affect route price");
                constel.SetPosition(constellationPosition.Value);
            }
        }

        internal static void MapExtendedLevel(ExtendedLevel extendedLevel, List<string> ignoreList)
        {
            if (extendedLevel == null) //skip null extendedLevel (this should never happen but just in case lol)
                return;

            string moonName = CommonStringStuff.BepinFriendlyString(extendedLevel.NumberlessPlanetName);
            Plugin.Spam($"moonName is {moonName}");

            if (moonName.Length < 1) //skip too short name
                return;

            if (ignoreList.Contains(moonName.ToLower())) //ignore moons specified by user config
                return;

            if (moonName.ToLower() == CompanyMoon.ToLower()) //ignore company moon
                return;

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
                    constellationList.Do(c => AddToConstelMoons(moonName, c, stayHiding.Value));
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

        internal static void AddToConstelMoons(string newMoon, string cName, bool stayHidden)
        {
            if (ConstellationStuff.Count < 0)
                return;

            List<ClassMapper> matching = ConstellationStuff.FindAll(x => x.consName == cName && !x.constelMoons.Contains(newMoon));

            matching.Do(x => AddMoonTo(x, newMoon, stayHidden));
        }

        private static void AddMoonTo(ClassMapper mapped, string newMoon, bool stayHidden)
        {
            mapped.constelMoons.Add(newMoon);
            if (stayHidden)
                mapped.stayHiddenMoons.Add(newMoon);
            Plugin.Spam($"adding {newMoon} to {mapped.consName} / stayHidden: {stayHidden}");
        }

        internal static int GetMoonPrice(string moonName)
        {
            if (MoonPrices.Count < 1)
                return 0;

            if(MoonPrices.Any(m => m.Key.NumberlessPlanetName.ToLower() == moonName.ToLower()))
                return MoonPrices.FirstOrDefault(m => m.Key.NumberlessPlanetName.ToLower() == moonName.ToLower()).Value;

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
