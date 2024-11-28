using LethalConstellations.ConfigManager;
using LethalLevelLoader;
using OpenLib.Common;
using OpenLib.CoreMethods;
using System.Collections.Generic;
using System.Linq;
using static LethalConstellations.PluginCore.Collections;
using static OpenLib.ConfigManager.ConfigSetup;
using Random = System.Random;

namespace LethalConstellations.PluginCore
{
    internal class MenuStuff
    {
        internal static bool makeMenus = false;
        //StartOfRound.Instance.randomNames //use this for autoreplacing invalid keywords
        internal static void PreInit()
        {
            //update constellation category names here
            if (FixBadConfig())
                Plugin.Spam($"ConstellationWord custom words updated to {ConstellationsWord}");

            if (FixBadNames())
                UpdateBadNames();

            Init();
        }

        internal static void Init()
        {
            Plugin.Spam("CreateConstellationCategories()");
            CreateConstellationCategories();
            Plugin.Spam("ConstellationsMainMenu()");
            ConstellationsMainMenu();
            Plugin.Spam("CreateConstellationCommands()");
            CreateConstellationCommands();
            CreateDummyNode();
            MoonStuff.ModifyMoonPrices();
            //Plugin.Spam($"GetCurrentConstellation: {LevelManager.CurrentExtendedLevel.NumberlessPlanetName}");
            //LevelStuff.GetCurrentConstellation(LevelManager.CurrentExtendedLevel.NumberlessPlanetName);
        }

        internal static void UpdateBadNames()
        {
            Plugin.Spam("Updating dictionaries with corrected ConstellationWord Names");
            ClassMapper.UpdateCNames(ConstellationStuff, CNameFix);

        }

        internal static bool FixBadConfig()
        {
            if (DynamicBools.TryGetKeyword(ConstellationsWord))
            {
                ConstellationsWord = GetCleanKeyWord();
                ConstellationWord = ConstellationsWord;
                return true;
            }
            return false;
        }

        internal static bool FixBadNames()
        {
            if (ConstellationsList.Count < 1 || Configuration.ConstellationsUseFauxWords.Value)
                return false;

            CNameFix.Clear();
            List<string> newList = [];
            int wordsReplaced = 0;

            foreach (string item in ConstellationsList)
            {
                if (DynamicBools.TryGetKeyword(item))
                {
                    Plugin.Spam($"Keyword already exists for {item}");
                    string newWord = GetCleanKeyWord();
                    CNameFix.Add(item, newWord);
                    newList.Add(newWord);
                    wordsReplaced++;
                }
                else
                    newList.Add(item);
            }

            if (wordsReplaced > 0)
            {
                ConstellationsList = newList;
                Plugin.Spam("ConstellationsList replaced with newList with VALID KEYWORDS");
                return true;
            }
            else
                return false;

        }

        internal static string GetCleanKeyWord()
        {
            for (int i = 0; i < StartOfRound.Instance.randomNames.Length - 1; i++)
            {
                int randomIndex = Rand.Next(0, StartOfRound.Instance.randomNames.Length);
                string newName = StartOfRound.Instance.randomNames[randomIndex];

                if (!DynamicBools.TryGetKeyword(newName))
                {
                    return newName;
                }
            }
            return "invalidWord";
        }

        internal static void CreateConstellationCategories()
        {
            ConstellationCats.Clear();
            if (ConstellationStuff.Count < 1)
                return;

            foreach (ClassMapper item in ConstellationStuff)
            {
                Plugin.Spam($"{item.consName} category creation");
                int catMoons = item.constelMoons.Count;
                if (catMoons < 1)
                    continue;
                string menuText = item.menuText;
                string defaultMoon = item.defaultMoon;
                string defaultWeather = "";
                if (item.defaultMoonLevel == null)
                {
                    Plugin.WARNING($"{item.consName} defaultMoonLevel is NULL. This will affect performance of the mod");
                }
                else
                    defaultWeather = TerminalManager.GetWeatherConditions(item.defaultMoonLevel);

                int getPrice = LevelStuff.GetConstPrice(item.consName);

                menuText = menuText.Replace("[~t]", "\t").Replace("[~n]", "\n").Replace("[name]", item.consName).Replace("[price]", $"{getPrice}").Replace("[defaultmoon]", $"{defaultMoon}").Replace("[currentweather]", defaultWeather).Replace("[optionals]", item.optionalParams);

                if (Configuration.HideUnaffordableConstellations.Value && getPrice > Plugin.instance.Terminal.groupCredits)
                    continue;

                if (!item.isHidden)
                    ConstellationCats.Add(item.consName, menuText);
            }
        }

        internal static void ConstellationsMainMenu()
        {
            Plugin.Spam($"{ConstellationCats.Count}");

            ConstellationsNode = AddingThings.AddNodeManual("Constellations_Menu", ConstellationsWord, MainMenu.ReturnMainMenu, true, 0, defaultListing);
            TerminalNode consInfo = BasicTerminal.CreateNewTerminalNode();
            consInfo.displayText = $"{Configuration.ConstellationsInfoText.Value}\r\n";
            consInfo.clearPreviousText = true;

            if (DynamicBools.TryGetKeyword("info", out TerminalKeyword infokeyword))
            {
                AddingThings.AddCompatibleNoun(ref infokeyword, ConstellationsWord.ToLower(), consInfo);
                Plugin.Spam("Adding info stuff");
            }

            if (Configuration.ConstellationsShortcuts.Value.Length > 0)
            {
                List<string> shortcuts = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.ConstellationsShortcuts.Value, ',');
                foreach (string shortcut in shortcuts)
                {
                    AddingThings.AddKeywordToExistingNode(shortcut, ConstellationsNode);
                    Plugin.Spam($"{shortcut} added to ConstellationsNode");

                    if(infokeyword != null)
                        AddingThings.AddCompatibleNoun(ref infokeyword, shortcut.ToLower(), consInfo);
                }
            }

            AddHintsToNodes();

        }

        internal static void AddHintsToNodes()
        {
            string hintText = Configuration.ConstellationsHintText.Value.Replace("[keyword]", $"{ConstellationsWord.ToUpper()}");
            if (LogicHandling.TryGetFromAllNodes("OtherCommands", out TerminalNode otherNode))
            {
                if (Configuration.AddHintTo.Value == "both" || Configuration.AddHintTo.Value == "help")
                    AddingThings.AddToHelpCommand($"{hintText}");

                if (Configuration.AddHintTo.Value == "both" || Configuration.AddHintTo.Value == "other")
                    AddingThings.AddToExistingNodeText($"\n{hintText}", ref otherNode);
            }

        }

        internal static void CreateDummyNode()
        {
            Plugin.instance.dummyNode = AddingThings.CreateDummyNode("constellations_dummy", true, "");
        }

        private static void AddConstellationRoute(bool FauxWord, bool confirmation, ClassMapper item)
        {
            if (confirmation)
            {
                if (FauxWord)
                {
                    FauxKeyword consRoute = new(ConstellationsWord.ToLower(), item.consName.ToLower(), LevelStuff.AskRouteConstellation);
                    consRoute.AddConfirm(LevelStuff.RouteConstellation, LevelStuff.DenyRouteConstellation);

                    AddingThings.AddToFauxListing(consRoute, defaultListing);
                    Plugin.Spam($"route command for {item.consName} added");

                    AddShortcuts(item, true, true);

                }
                else
                {
                    TerminalNode newRouteNode = AddingThings.AddNodeManual($"{item.consName}", $"{item.consName.ToLower()}", LevelStuff.AskRouteConstellation, true, 1, defaultListing, 0, LevelStuff.RouteConstellation, LevelStuff.DenyRouteConstellation, "", "", false, 1, "", true);

                    if (item.consName.Any(c => !char.IsLetterOrDigit(c)))
                        CommandRegistry.AddSpecialListString(ref defaultListing, newRouteNode, item.consName);

                    if (DynamicBools.TryGetKeyword("route", out TerminalKeyword routeKW))
                    {
                        AddingThings.AddCompatibleNoun(ref routeKW, item.consName.ToLower(), newRouteNode);
                    }

                    Plugin.Spam($"route command for {item.consName} added");

                    AddShortcuts(item, false, true, newRouteNode);
                }
            }
            else
            {
                if (FauxWord)
                {
                    FauxKeyword consRoute = new(ConstellationsWord.ToLower(), item.consName.ToLower(), LevelStuff.RouteConstellation);

                    AddingThings.AddToFauxListing(consRoute, defaultListing);
                    Plugin.Spam($"route command for {item.consName} added");

                    AddShortcuts(item, true, false);

                }
                else
                {
                    TerminalNode newRouteNode = AddingThings.AddNodeManual($"{item.consName}", $"{item.consName.ToLower()}", LevelStuff.RouteConstellation, true, 0, defaultListing);

                    if (DynamicBools.TryGetKeyword("route", out TerminalKeyword routeKW))
                    {
                        AddingThings.AddCompatibleNoun(ref routeKW, item.consName.ToLower(), newRouteNode);
                    }

                    Plugin.Spam($"route command for {item.consName} added");

                    AddShortcuts(item, false, true, newRouteNode);
                }
            }
        }

        internal static void CreateConstellationCommands()
        {
            if (ConstellationStuff.Count < 1)
                return;


            foreach (ClassMapper item in ConstellationStuff)
            {
                if (item.constelMoons.Count < 1)
                    continue;

                AddConstellationRoute(Configuration.ConstellationsUseFauxWords.Value, Configuration.RequireConfirmation.Value, item);

            }

            if (Configuration.ConstellationSpecificInfoNodes.Value && ConstellationStuff.Count > 0 && !Configuration.ConstellationsUseFauxWords.Value)
            {
                if (DynamicBools.TryGetKeyword("info", out TerminalKeyword infoKeyword))
                {
                    foreach (ClassMapper item in ConstellationStuff)
                    {
                        TerminalNode itemNode = BasicTerminal.CreateNewTerminalNode();
                        itemNode.name = $"{item.consName}_info";
                        itemNode.clearPreviousText = true;
                        itemNode.displayText = $"{item.infoText}\r\n";
                        Plugin.Spam($"Adding info command for {item.consName}");
                        AddingThings.AddCompatibleNoun(ref infoKeyword, item.consName.ToLower(), itemNode);
                        List<string> shortcuts = CommonStringStuff.GetKeywordsPerConfigItem(item.shortcutList, ',');
                        
                        foreach (string shortcut in shortcuts)
                        {
                            AddingThings.AddCompatibleNoun(ref infoKeyword, shortcut.ToLower(), itemNode);
                        }
                    }
                }
            }
        }

        internal static void AddShortcuts(ClassMapper item, bool FauxWords, bool addConfirm, TerminalNode newRouteNode = null)
        {
            if (item.shortcutList.Length > 0)
            {
                List<string> shortcuts = CommonStringStuff.GetKeywordsPerConfigItem(item.shortcutList, ',');
                foreach (string shortcut in shortcuts)
                {
                    if (!FauxWords)
                        AddingThings.AddKeywordToExistingNode(shortcut, newRouteNode);
                    else
                    {
                        FauxKeyword consRoute;
                        if (addConfirm)
                        {
                            consRoute = new(ConstellationsWord.ToLower(), shortcut, LevelStuff.AskRouteConstellation);

                            consRoute.AddConfirm(LevelStuff.RouteConstellation, LevelStuff.DenyRouteConstellation);
                        }
                        else
                        {
                            consRoute = new(ConstellationsWord.ToLower(), shortcut, LevelStuff.RouteConstellation);
                        }


                        AddingThings.AddToFauxListing(consRoute, defaultListing);
                    }
                }
            }
        }

    }
}
