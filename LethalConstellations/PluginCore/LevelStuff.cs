﻿using LethalLevelLoader;
using System.Collections.Generic;
using static LethalConstellations.PluginCore.Collections;

namespace LethalConstellations.PluginCore
{
    internal class LevelStuff
    {
        internal static bool gotConstellation = false;
        internal static bool cancelConfirmation = false;
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
                    return $"Unable to afford to travel to {ConstellationWord} - {CurrentConstellation.ToUpper()}\r\n\r\n";
                Plugin.Spam($"oldcreds: {Plugin.instance.Terminal.groupCredits}");
                Plugin.instance.Terminal.groupCredits -= getPrice;
                Plugin.Spam($"newCreds amount = {Plugin.instance.Terminal.groupCredits}");
                StartOfRound.Instance.ChangeLevelServerRpc(newLevelID, Plugin.instance.Terminal.groupCredits);
                gotConstellation = false;
                return $"Travelling to {ConstellationWord} - {CurrentConstellation.ToUpper()}\nYour new credits balance: ${Plugin.instance.Terminal.groupCredits}\r\n\r\n";
            }
            else
                return "ERROR: Unable to load constellation default level!\r\n\r\n";
        }

        internal static string AskRouteConstellation()
        {
            if (CantRouteConst(out string failText))
                return failText;

            return $"Travel to {ConstellationWord} - {constellationName.ToUpper()}?\n\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";

        }

        internal static string DenyRouteConstellation()
        {
            string item = constellationName.ToUpper();
            ResetConstVars();

            return $"Route to {ConstellationWord} {item} has been canceled.\r\n\r\n\r\n";
        }

        internal static void ResetConstVars()
        {
            cancelConfirmation = true;
            gotConstellation = false;
            constellationName = "";
        }

        internal static bool CantRouteConst(out string failText)
        {
            Plugin.Spam($"CantRouteConst");

            if (ConstellationStuff.Count < 1)
            {
                ResetConstVars();
                failText = "Configuration failure detected!";
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
                failText = $"Ship needs to be in orbit in order to travel to new {ConstellationWord}!\r\n\r\n";
                return true;
            }

            if (Plugin.instance.Terminal == null)
                Plugin.ERROR("terminal instance is null?!?");

            Plugin.Spam("Getting screen text");
            string screen = Plugin.instance.Terminal.screenText.text.Substring(Plugin.instance.Terminal.screenText.text.Length - Plugin.instance.Terminal.textAdded);
            Plugin.Spam(screen);
            Plugin.Spam($"{ConstellationStuff.Count}");
            if(ClassMapper.TryGetConstellation(ConstellationStuff, screen, out ClassMapper outConst))
            {
                Plugin.Spam($"Current Constellation: {CurrentConstellation}");
                if(CurrentConstellation == outConst.consName)
                {
                    failText = $"You are already located at {ConstellationWord} - {CurrentConstellation}...\r\n\r\n";
                    ResetConstVars();
                    return true;
                }
                else
                {
                    if (!gotConstellation)
                    {
                        constellationName = outConst.consName;
                        Plugin.Spam($"keyword detected setting constellationName - {constellationName}");
                        gotConstellation = true;
                    }
                }
            }

            failText = "";
            return false;
                
        }

        internal static int GetConstPrice(string constName)
        {
            if (ConstellationStuff.Count < 1)
                return 0;

            foreach (ClassMapper item in ConstellationStuff)
            {
                if (item.consName.ToLower() == constName.ToLower())
                {
                    return item.constelPrice;
                }
            }

            return 0;
        }

        internal static string GetDefaultLevel(string constellation)
        {
            if (ConstellationStuff.Count < 1)
                return "";

            foreach (ClassMapper item in ConstellationStuff)
            {
                Plugin.Spam($"checking {item.consName} to {constellation}");
                if (item.consName.ToLower() == constellation.ToLower())
                    return item.defaultMoon;
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

        internal static void UpdateLevelList(List<string> moonNames, bool enableMoons)
        {
            if (!enableMoons)
            {
                Plugin.Spam($"Disabling all moons in: {constellationName}");
                foreach(string name in moonNames)
                {
                    AdjustExtendedLevel(name, constellationName, false);
                }
            }
            else
            {
                Plugin.Spam($"Enabling all moons in: {constellationName}");
                foreach (string name in moonNames)
                {
                    AdjustExtendedLevel(name, constellationName, true);
                }
            }
        }

        internal static void AdjustExtendedLevel(string levelName, string constellationName, bool thisConstellation)
        {
            List<ExtendedLevel> allLevels = PatchedContent.VanillaExtendedLevels;
            allLevels.AddRange(PatchedContent.CustomExtendedLevels);

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                Plugin.Spam($"checking {extendedLevel.NumberlessPlanetName} vs {levelName}");
                if (extendedLevel.NumberlessPlanetName.ToLower() == levelName.ToLower())
                {
                    if (!thisConstellation)
                    {
                        Plugin.Spam($"{extendedLevel.NumberlessPlanetName} should be DISABLED");
                        extendedLevel.IsRouteHidden = true;
                        extendedLevel.IsRouteLocked = true;
                        extendedLevel.LockedRouteNodeText = $"{extendedLevel.NumberlessPlanetName} is not located in this {ConstellationWord}.\n\n\tType \"{ConstellationsWord}\" to see a listing of LethalConstellations.\r\n\r\n";
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

        internal static void AdjustToNewConstellation(string levelName, string constellationName)
        {
            if (constellationName.Length == 0)
            {
                Plugin.WARNING("Invalid constellationName at AdjustToNewConstellation");
                return;
            }

            if(levelName.ToLower() == CompanyMoon.ToLower() && ClassMapper.TryGetConstellation(ConstellationStuff, constellationName, out ClassMapper conClass))
            {
                AdjustExtendedLevel(levelName, constellationName, conClass.canRouteCompany);
                return;
            }

            //ConstellationsToMoons.Add(extendedLevel.NumberlessPlanetName, levelToConstellation.Value);
            foreach (ClassMapper item in ConstellationStuff)
            {
                if(item.consName == constellationName)
                    UpdateLevelList(item.constelMoons, true);
                else
                    UpdateLevelList(item.constelMoons, false);
            }
        }


        internal static void GetCurrentConstellation(string levelName)
        {
            if(levelName.Length == 0)
            {
                Plugin.WARNING("Invalid levelName at GetCurrentConstellation");
                return;
            }

            if (levelName.ToLower() == CompanyMoon.ToLower())
                return;

            //ConstellationsToMoons.Add(extendedLevel.NumberlessPlanetName, levelToConstellation.Value);
            foreach (ClassMapper item in ConstellationStuff)
            {
                List<string> lowerCaseMoons = item.constelMoons.ConvertAll(x => x.ToLower());
                if (lowerCaseMoons.Contains(levelName.ToLower()))
                {
                    CurrentConstellation = item.consName;
                    Plugin.Spam($"CurrentConstellation set to {CurrentConstellation}");
                    AdjustToNewConstellation(levelName, CurrentConstellation);
                }

            }
        }
    }
}
