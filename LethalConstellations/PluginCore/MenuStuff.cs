using static LethalConstellations.PluginCore.Collections;
using static OpenLib.Menus.MenuBuild;
using static OpenLib.ConfigManager.ConfigSetup;
using LethalConstellations.ConfigManager;
using System.Collections.Generic;
using OpenLib.CoreMethods;
using UnityEngine;
using Steamworks.Ugc;

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
                Plugin.Spam($"Constellation custom words updated to {Collections.Constellations}");

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

            //Plugin.Spam("setting currentLevel");
            //LevelStuff.GetCurrentConstellation(LevelManager.CurrentExtendedLevel.NumberlessPlanetName);
        }

        internal static void UpdateBadNames()
        {
            Plugin.Spam("Updating dictionaries with corrected Constellation Names");
            UpdateDictionaryKey(ref ConstellationPrices, CNameFix);
            UpdateDictionaryKey(ref DefaultMoons, CNameFix);
            UpdateDictionaryKey(ref ConstellationMenuText, CNameFix);
        }

        internal static bool FixBadConfig()
        {
            if (DynamicBools.TryGetKeyword(Collections.Constellations))
            {
                Collections.Constellations = GetCleanKeyWord();
                Constellation = Collections.Constellations;
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
            //Dictionary<string, string> ConstellationCategories = [];

            foreach(string item in ConstellationsList)
            {
                int catMoons = GetMoonCount(ConstellationsToMoons, item);
                if (catMoons < 1)
                    continue;
                string menuText = GetConstText(item);
                string defaultMoon = LevelStuff.GetDefaultLevel(item);
                int price = LevelStuff.GetConstPrice(item);
                menuText = menuText.Replace("[~t]", "\t").Replace("[~n]","\n").Replace("[name]", item).Replace("[price]", $"{price}").Replace("[defaultmoon]", $"{defaultMoon}");


                ConstellationCats.Add(item, menuText);
            }

            if (ConstellationCats.Count == 0)
            {
                Plugin.WARNING($"Failed to get constellations from config item: {Configuration.ConstellationList.Value}");
            }
        }

        internal static string GetConstText(string query)
        {
            foreach(KeyValuePair<string,string> pair in ConstellationMenuText)
            {
                if(pair.Key.ToLower() == query.ToLower())
                {
                    return pair.Value;
                }
            }

            return "GetConstText FAILED";
        }

        internal static int GetMoonCount(Dictionary<string, string> listing, string query)
        {
            int count = 0;

            foreach (KeyValuePair<string, string> item in listing)
            {
                if (listing.ContainsValue(query))
                {
                    count++;
                }
            }

            return count;
        }

        internal static void ConstellationsMainMenu()
        {
            Plugin.Spam($"{ConstellationCats.Count}");
            string menuText = AssembleMainMenuText($"========== {Collections.Constellations} Routing Matrix ==========\r\n", ConstellationCats);
            Plugin.Spam(menuText);
            string otherText = Configuration.ConstellationsOtherText.Value.Replace("[keyword]", $"{Collections.Constellations}");
            AddingThings.AddBasicCommand("Constellations_Menu", Collections.Constellations, menuText, false, true, "other", $"\n{otherText}");
            AddingThings.AddBasicCommand("constellations_info", $"info {Collections.Constellations}", Configuration.ConstellationsInfoText.Value, false, true);

        }

        internal static void CreateDummyNode()
        {
            Plugin.instance.dummyNode = AddingThings.CreateDummyNode("constellations_dummy", true, "");
        }

        internal static void CreateConstellationCommands()
        {
            if (ConstellationsToMoons.Count < 1)
                return;


            foreach (string item in ConstellationsList)
            {
                int catMoons = GetMoonCount(ConstellationsToMoons, item);
                if (catMoons < 1)
                    continue;

                if (Configuration.RequireConfirmation.Value)
                {
                    TerminalNode newRouteNode = AddingThings.AddNodeManual($"{item}", $"{item.ToLower()}", LevelStuff.AskRouteConstellation, true, 1, defaultListing, 0, LevelStuff.RouteConstellation, LevelStuff.DenyRouteConstellation, "", "", false, 1, "", true);
                    //ConstellationNodes.Add(newRouteNode);
                    Plugin.Spam($"Constellation command for {item} added");
                }
                else
                {
                    TerminalNode newRouteNode = AddingThings.AddNodeManual($"{item}", $"{item.ToLower()}", LevelStuff.RouteConstellation, true, 0, defaultListing, 0, null, null, "", "", false, 1, "", true);
                    //ConstellationNodes.Add(newRouteNode);
                    Plugin.Spam($"Constellation command for {item} added");
                }
                
            }

            if (Configuration.ConstellationSpecificInfoNodes.Value && ConstellationInfoNodes.Count > 0)
            {
                foreach(KeyValuePair<string,string> constellation in ConstellationInfoNodes)
                {
                    Plugin.Spam($"Adding info command for {constellation.Key}");
                    AddingThings.AddBasicCommand($"{constellation.Key}_info", $"info {constellation.Key}", constellation.Value, false, true);
                }

            }
        }

    }
}
