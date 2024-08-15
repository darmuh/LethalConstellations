using static LethalConstellations.PluginCore.Collections;
using static OpenLib.ConfigManager.ConfigSetup;
using LethalConstellations.ConfigManager;
using System.Collections.Generic;
using OpenLib.CoreMethods;
using UnityEngine;
using OpenLib.Common;
using LethalLevelLoader;

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
                Plugin.Spam($"ConstellationWord custom words updated to {Collections.ConstellationsWord}");

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
            Plugin.Spam($"GetCurrentConstellation: {LevelManager.CurrentExtendedLevel.NumberlessPlanetName}");
            LevelStuff.GetCurrentConstellation(LevelManager.CurrentExtendedLevel.NumberlessPlanetName);
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
            if (ConstellationsList.Count < 1)
                return false;

            CNameFix.Clear();
            List<string> newList = [];
            int wordsReplaced = 0;

            foreach(string item in ConstellationsList)
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
            for(int i = 0; i < StartOfRound.Instance.randomNames.Length - 1; i++)
            {
                int randomIndex = Random.Range(0, StartOfRound.Instance.randomNames.Length - 1);
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

            foreach(ClassMapper item in ConstellationStuff)
            {
                int catMoons = item.constelMoons.Count;
                if (catMoons < 1)
                    continue;
                string menuText = item.menuText;
                string defaultMoon = item.defaultMoon;
                string defaultWeather = TerminalManager.GetWeatherConditions(item.defaultMoonLevel);

                int getPrice = LevelStuff.GetConstPrice(item.consName);

                menuText = menuText.Replace("[~t]", "\t").Replace("[~n]","\n").Replace("[name]", item.consName).Replace("[price]", $"{getPrice}").Replace("[defaultmoon]", $"{defaultMoon}").Replace("[currentweather]", defaultWeather).Replace("[optionals]", item.optionalParams);

                if (Configuration.HideUnaffordableConstellations.Value && getPrice > Plugin.instance.Terminal.groupCredits)
                    continue;

                if(!item.isHidden)
                    ConstellationCats.Add(item.consName, menuText);
            }
        }

        internal static void ConstellationsMainMenu()
        {
            Plugin.Spam($"{ConstellationCats.Count}");
            
            ConstellationsNode = AddingThings.AddNodeManual("Constellations_Menu", ConstellationsWord, MainMenu.ReturnMainMenu, true, 0, defaultListing);

            if(Configuration.ConstellationsShortcuts.Value.Length > 0)
            {
                List<string> shortcuts = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.ConstellationsShortcuts.Value, ',');
                foreach(string shortcut in shortcuts)
                {
                    AddingThings.AddKeywordToExistingNode(shortcut, ConstellationsNode);
                    Plugin.Spam($"{shortcut} added to ConstellationsNode");
                } 
            }

            AddHintsToNodes();
            AddingThings.AddBasicCommand("constellations_info", $"info {ConstellationsWord}", Configuration.ConstellationsInfoText.Value, false, true);

        }

        internal static void AddHintsToNodes()
        {
            string hintText = Configuration.ConstellationsHintText.Value.Replace("[keyword]", $"{ConstellationsWord.ToUpper()}");
            TerminalNode otherNode = LogicHandling.GetFromAllNodes("OtherCommands");

            if (Configuration.AddHintTo.Value == "both" || Configuration.AddHintTo.Value == "help")
                AddingThings.AddToHelpCommand($"{hintText}");

            if (Configuration.AddHintTo.Value == "both" || Configuration.AddHintTo.Value == "other")
                AddingThings.AddToExistingNodeText($"\n{hintText}", ref otherNode);
        }

        internal static void CreateDummyNode()
        {
            Plugin.instance.dummyNode = AddingThings.CreateDummyNode("constellations_dummy", true, "");
        }

        internal static void CreateConstellationCommands()
        {
            if (ConstellationStuff.Count < 1)
                return;


            foreach (ClassMapper item in ConstellationStuff)
            {
                if (item.constelMoons.Count < 1)
                    continue;

                if (Configuration.RequireConfirmation.Value)
                {
                    TerminalNode newRouteNode = AddingThings.AddNodeManual($"{item.consName}", $"{item.consName.ToLower()}", LevelStuff.AskRouteConstellation, true, 1, defaultListing, 0, LevelStuff.RouteConstellation, LevelStuff.DenyRouteConstellation, "", "", false, 1, "", true);
                    
                    AddShortcuts(item, newRouteNode);
                    Plugin.Spam($"ConstellationWord command for {item.consName} added");
                }
                else
                {
                    TerminalNode newRouteNode = AddingThings.AddNodeManual($"{item.consName}", $"{item.consName.ToLower()}", LevelStuff.RouteConstellation, true, 0, defaultListing, 0, null, null, "", "", false, 1, "", true);

                    AddShortcuts(item, newRouteNode);
                    Plugin.Spam($"ConstellationWord command for {item.consName} added");
                }
                
            }

            if (Configuration.ConstellationSpecificInfoNodes.Value && ConstellationStuff.Count > 0)
            {
                foreach(ClassMapper item in ConstellationStuff)
                {
                    Plugin.Spam($"Adding info command for {item.consName}");
                    AddingThings.AddBasicCommand($"{item.consName}_info", $"info {item.consName}", item.infoText, false, true);
                }
            }
        }

        internal static void AddShortcuts(ClassMapper item, TerminalNode newRouteNode)
        {
            if (item.shortcutList.Length > 0)
            {
                List<string> shortcuts = CommonStringStuff.GetKeywordsPerConfigItem(item.shortcutList);
                foreach (string shortcut in shortcuts)
                {
                    AddingThings.AddKeywordToExistingNode(shortcut, newRouteNode);
                }
            }
        }

    }
}
