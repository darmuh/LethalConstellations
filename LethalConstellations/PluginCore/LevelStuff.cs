using LethalLevelLoader;
using OpenLib.Common;
using Steamworks.Ugc;
using System.Collections.Generic;
using static LethalConstellations.PluginCore.Collections;

namespace LethalConstellations.PluginCore
{
    internal class LevelStuff
    {
        internal static bool gotConstellation;
        internal static bool cancelConfirmation;
        internal static string constellationName;


        internal static string RouteConstellation()
        {
            if (CantRouteConst(out string failText))
                return failText;

            string defaultLevel = GetDefaultLevel(constellationName);
            int newLevelID = GetLevelID(defaultLevel);
            if (newLevelID != -1)
            {
                CurrentConstellation = constellationName;
                int getPrice = GetConstPrice(constellationName);
                if (Plugin.instance.Terminal.groupCredits < getPrice)
                    return $"Unable to afford to travel to Constellation - {CurrentConstellation.ToUpper()}\r\n\r\n";
                Plugin.Spam($"oldcreds: {Plugin.instance.Terminal.groupCredits}");
                Plugin.instance.Terminal.groupCredits -= getPrice;
                Plugin.Spam($"newCreds amount = {Plugin.instance.Terminal.groupCredits}");
                StartOfRound.Instance.ChangeLevelServerRpc(newLevelID, Plugin.instance.Terminal.groupCredits);
                gotConstellation = false;
                return $"Travelling to Constellation - {CurrentConstellation.ToUpper()}\nYour new credits balance: ${Plugin.instance.Terminal.groupCredits}\r\n\r\n";
            }
            else
                return "ERROR: Unable to load constellation default level!\r\n\r\n";
        }

        internal static string AskRouteConstellation()
        {
            if (CantRouteConst(out string failText))
                return failText;

            return $"Travel to {Constellation} - {constellationName.ToUpper()}?\n\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";

        }

        internal static string DenyRouteConstellation()
        {
            string item = constellationName.ToUpper();
            ResetConstVars();

            return $"Route to {Constellation} {item} has been canceled.\r\n\r\n\r\n";
        }

        internal static void ResetConstVars()
        {
            cancelConfirmation = true;
            gotConstellation = false;
            constellationName = "";
        }

        internal static bool CantRouteConst(out string failText)
        {
            if (ConstellationsToMoons.Count < 1)
            {
                ResetConstVars();
                failText = "Constellation configuration failure detected!";
                return true;
            }

            if (StartOfRound.Instance.travellingToNewLevel)
            {
                ResetConstVars();
                failText = "Ship is currently in motion, unable to change routing at this time!\r\n\r\n";
                return true;
            }  

            if (!StartOfRound.Instance.inShipPhase)
            {
                ResetConstVars();
                failText = "Ship needs to be in orbit in order to travel to a new constellation!\r\n\r\n";
                return true;
            }
            string[] screenWords = CommonStringStuff.GetWords();
            
            if (screenWords[0].ToLower() == CurrentConstellation.ToLower())
            {
                failText = $"You are already located at {Constellation} - {CurrentConstellation}...\r\n\r\n";
                ResetConstVars();
                return true;
            }
            else
            {
                if (!gotConstellation)
                {
                    constellationName = screenWords[0];
                    Plugin.Spam($"keyword detected setting constellationName - {constellationName}");
                    gotConstellation = true;
                }   
            }

            failText = "";
            return false;
                
        }

        internal static int GetConstPrice(string constName)
        {
            if (ConstellationPrices.Count < 1)
                return 0;

            foreach (KeyValuePair<string, int> item in ConstellationPrices)
            {
                if (item.Key.ToLower() == constName.ToLower())
                {
                    return item.Value;
                }
            }

            return 0;
        }

        internal static string GetDefaultLevel(string constellation)
        {
            if (DefaultMoons.Count < 1)
                return "";

            foreach (KeyValuePair<string, string> item in DefaultMoons)
            {
                Plugin.Spam($"checking {item.Key} to {constellation}");
                if (item.Key.ToLower() == constellation.ToLower())
                    return item.Value;
            }

            return "";
        }

        internal static int GetLevelID(string levelName)
        {
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                Plugin.Spam($"checking {extendedLevel.NumberlessPlanetName} vs {levelName}");
                if(extendedLevel.NumberlessPlanetName.ToLower() == levelName.ToLower())
                {
                    return extendedLevel.SelectableLevel.levelID;
                }
            }

            return -1;
        }

        internal static void AdjustExtendedLevel(string levelName, string constellationName, bool thisConstellation)
        {
            List<ExtendedLevel> allLevels = PatchedContent.VanillaExtendedLevels;
            allLevels.AddRange(PatchedContent.CustomExtendedLevels);

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                Plugin.Spam($"checking {extendedLevel.NumberlessPlanetName} vs {levelName}");
                if (extendedLevel.NumberlessPlanetName == levelName)
                {
                    if (!thisConstellation)
                    {
                        Plugin.Spam($"{extendedLevel.NumberlessPlanetName} should be DISABLED");
                        extendedLevel.IsRouteHidden = true;
                        extendedLevel.IsRouteLocked = true;
                        extendedLevel.LockedRouteNodeText = $"{extendedLevel.NumberlessPlanetName} is not located in this Constellation.\n\n\tType \"CONSTELLATIONS\" to see a listing of LethalConstellations.\r\n\r\n";
                    }
                    else
                    {
                        Plugin.Spam($"{extendedLevel.NumberlessPlanetName} should be ENABLED");
                        extendedLevel.IsRouteLocked = false;
                        extendedLevel.IsRouteHidden = false;
                        extendedLevel.LockedRouteNodeText = "";
                    }      
                }
            }
        }

        internal static void AdjustToNewConstellation(string constellationName)
        {
            if (constellationName.Length == 0)
            {
                Plugin.WARNING("Invalid constellationName at AdjustToNewConstellation");
                return;
            }

            Plugin.Spam($"AdjustToNewConstellation({constellationName})");
            //ConstellationsToMoons.Add(extendedLevel.NumberlessPlanetName, levelToConstellation.Value);
            foreach (KeyValuePair<string, string> item in ConstellationsToMoons)
            {
                if (item.Value.ToLower() != constellationName.ToLower())
                {
                    Plugin.Spam($"{item.Key} is not in constellation: {constellationName}");
                    AdjustExtendedLevel(item.Key, constellationName, false);
                }
                else
                {
                    Plugin.Spam($"{item.Key} IS in constellation: {constellationName}");
                    AdjustExtendedLevel(item.Key, constellationName, true);
                }
                    
            }
        }


        internal static void GetCurrentConstellation(string levelName)
        {
            if(levelName.Length == 0)
            {
                Plugin.WARNING("Invalid levelName at GetCurrentConstellation");
                return;
            }

            //ConstellationsToMoons.Add(extendedLevel.NumberlessPlanetName, levelToConstellation.Value);
            foreach (KeyValuePair<string, string> item in ConstellationsToMoons)
            {
                if (item.Key.ToLower() == levelName.ToLower())
                {
                    CurrentConstellation = item.Value;
                    Plugin.Spam($"CurrentConstellation set to {CurrentConstellation}");
                    AdjustToNewConstellation(CurrentConstellation);
                }

            }
        }
    }
}
