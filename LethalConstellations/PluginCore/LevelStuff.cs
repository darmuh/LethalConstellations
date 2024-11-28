using LethalConstellations.Compat;
using LethalConstellations.ConfigManager;
using LethalConstellations.EventStuff;
using LethalLevelLoader;
using System.Collections.Generic;
using static LethalConstellations.PluginCore.Collections;

namespace LethalConstellations.PluginCore
{
    internal class LevelStuff
    {
        internal static bool gotConstellation = false;
        internal static bool cancelConfirmation = false;
        internal static string constellationName = "";


        internal static string RouteConstellation()
        {
            if (CantRouteConst(out string failText))
                return failText;

            string defaultLevel = GetDefaultLevel(constellationName);
            int newLevelID = GetLevelID(defaultLevel);
            if (newLevelID != -1)
            {
                int getPrice = GetConstPrice(constellationName);
                if (Plugin.instance.Terminal.groupCredits < getPrice)
                    return $"Unable to afford to travel to {ConstellationWord} - {constellationName.ToUpper()}\r\n\r\n";
                CurrentConstellation = constellationName;
                Plugin.Spam($"oldcreds: {Plugin.instance.Terminal.groupCredits}");
                int newCreds = Plugin.instance.Terminal.groupCredits - getPrice;
                Plugin.Spam($"newCreds amount = {Plugin.instance.Terminal.groupCredits}");
                StartOfRound.Instance.ChangeLevelServerRpc(newLevelID, newCreds);
                gotConstellation = false;

                if (getPrice > 0)
                    Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);
                NewEvents.RouteConstellationSuccess.Invoke(); //for other mods to subscribe to successful route

                OneTimePurchaseCheck(constellationName);

                return $"Travelling to {ConstellationWord} - {CurrentConstellation.ToUpper()}\nYour new credits balance: ${newCreds}\r\n\r\n";
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
                failText = "Configuration failure detected!\r\n\r\n";
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

            if (Plugin.instance.Terminal.screenText.text == null || Plugin.instance.Terminal.textAdded < 0)
            {
                ResetConstVars();
                failText = $"Unable to determine terminal text\r\n\r\n";
                return true;
            }

            if (Plugin.instance.Terminal.screenText.text.Length <= Plugin.instance.Terminal.textAdded)
            {
                ResetConstVars();
                failText = $"Unable to determine terminal command given\r\n\r\n";
                return true;
            }


            Plugin.Spam("Getting screen text");
            string screen = Plugin.instance.Terminal.screenText.text.Substring(Plugin.instance.Terminal.screenText.text.Length - Plugin.instance.Terminal.textAdded);
            if (screen.ToLower().StartsWith("route"))
                screen = screen.Substring(5).TrimStart();
            else if(screen.ToLower().StartsWith("info"))
            {
                screen = screen.Substring(4).TrimStart();
                if (ClassMapper.TryGetConstellation(ConstellationStuff, screen, out ClassMapper infoConst))
                {
                    failText = $"{infoConst.infoText}\r\n";
                    ResetConstVars();
                    return true;
                }
                else
                {
                    ResetConstVars();
                    failText = $"Unable to determine terminal command given\r\n\r\n";
                    return true;
                }
            }
            Plugin.Spam(screen);
            Plugin.Spam($"{ConstellationStuff.Count}");
            if (ClassMapper.TryGetConstellation(ConstellationStuff, screen, out ClassMapper outConst))
            {
                Plugin.Spam($"Current Constellation: {CurrentConstellation}");
                if (CurrentConstellation == outConst.consName)
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

                if (outConst.isLocked)
                {
                    ResetConstVars();
                    failText = $"Unable to travel to {outConst.consName}. {ConstellationWord} is locked!\r\n\r\n";
                    return true;
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
                    if (!OneTimePurchaseDone(item))
                        return item.constelPrice;
                    else
                    {
                        Plugin.Spam($"{item.consName} is designated as a OneTimePurchase, and now costs 0 credits");
                        return 0;
                    }
                }
            }

            return 0;
        }

        internal static void OneTimePurchaseCheck(string constName)
        {
            if (ConstellationStuff.Count < 1)
                return;

            foreach (ClassMapper item in ConstellationStuff)
            {
                if (item.consName.ToLower() == constName.ToLower())
                {
                    if (!item.buyOnce || !Plugin.instance.LethalNetworkAPI)
                        return;
                    else
                    {
                        if (!item.oneTimePurchase)
                        {
                            Plugin.Spam("Updating oneTimePurchase to true");
                            item.oneTimePurchase = true;
                            if (!ConstellationsOTP.Contains(item.consName))
                            {
                                ConstellationsOTP.Add(item.consName);
                                SaveManager.SaveUnlocks(ConstellationsOTP);
                                NetworkThings.SyncUnlockSet(ConstellationsOTP);
                            }
                            else
                                Plugin.WARNING($"--- Error with oneTimePurchaseCheck, already in list ---");

                        }
                    }
                }
            }
        }

        internal static bool OneTimePurchaseDone(ClassMapper item)
        {
            if (!item.buyOnce)
                return false;
            else
            {
                if (item.oneTimePurchase)
                    return true;
            }

            return false;
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
                if (extendedLevel.NumberlessPlanetName.ToLower() == levelName.ToLower())
                {
                    return extendedLevel.SelectableLevel.levelID;
                }
            }

            return -1;
        }

        internal static void UpdateLevelList(ClassMapper thisConstellation, bool enableMoons)
        {
            if (!enableMoons)
            {
                Plugin.Spam($"Disabling all moons in: {thisConstellation.consName}");
                foreach (string name in thisConstellation.constelMoons)
                {
                    AdjustExtendedLevel(name, thisConstellation, false);
                }
            }
            else
            {
                Plugin.Spam($"Enabling all moons in: {thisConstellation.consName}");
                foreach (string name in thisConstellation.constelMoons)
                {
                    AdjustExtendedLevel(name, thisConstellation, true);
                }
            }
        }

        internal static void AdjustExtendedLevel(string levelName, ClassMapper myConst, bool thisConstellation)
        {
            if (Plugin.instance.LethalMoonUnlocks)
                return;

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
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
                        if (!myConst.stayHiddenMoons.Contains(extendedLevel.NumberlessPlanetName) && extendedLevel.NumberlessPlanetName.ToLower() != CompanyMoon.ToLower())
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

            Plugin.Spam($"Adjusting to {constellationName} from {levelName}");

            if (levelName.ToLower() == CompanyMoon.ToLower() && ClassMapper.TryGetConstellation(ConstellationStuff, constellationName, out ClassMapper conClass))
            {
                AdjustExtendedLevel(conClass.defaultMoon, conClass, conClass.canRouteCompany);
                return;
            }

            //ConstellationsToMoons.Add(extendedLevel.NumberlessPlanetName, levelToConstellation.Value);
            foreach (ClassMapper item in ConstellationStuff)
            {
                if (item.consName == constellationName)
                {
                    UpdateLevelList(item, true);

                    if (item.canRouteCompany)
                    {
                        AdjustExtendedLevel(CompanyMoon, item, true);
                    }
                    else
                    {
                        AdjustExtendedLevel(CompanyMoon, item, false);
                    }
                }
                else
                    UpdateLevelList(item, false);
            }

            CurrentConstellation = constellationName;
            if (GameNetworkManager.Instance.isHostingGame)
                SaveManager.SaveCurrentConstellation(CurrentConstellation);
        }


        internal static void GetCurrentConstellation(string levelName)
        {
            if (levelName.Length == 0)
            {
                Plugin.WARNING("Invalid levelName at GetCurrentConstellation");
                return;
            }

            if (levelName.ToLower() == CompanyMoon.ToLower())
            {
                DefaultConstellation();
                return;
            }

            //ConstellationsToMoons.Add(extendedLevel.NumberlessPlanetName, levelToConstellation.Value);
            foreach (ClassMapper item in ConstellationStuff)
            {
                List<string> lowerCaseMoons = item.constelMoons.ConvertAll(x => x.ToLower());
                if (lowerCaseMoons.Contains(levelName.ToLower()))
                {
                    CurrentConstellation = item.consName;
                    Plugin.Spam($"DefaultConstellation set to {CurrentConstellation}");
                    AdjustToNewConstellation(levelName, CurrentConstellation);
                }
            }
        }

        internal static void DefaultConstellation()
        {
            string currentLevel;
            if (LevelManager.CurrentExtendedLevel == null)
            {
                currentLevel = ExtendedLevel.GetNumberlessPlanetName(StartOfRound.Instance.currentLevel);
            }
            else
                currentLevel = LevelManager.CurrentExtendedLevel.NumberlessPlanetName;

            if (currentLevel.ToLower() == CompanyMoon.ToLower() && (CurrentConstellation.Length < 1 || !Configuration.ReturnToLastConstellationFromCompany.Value))
            {
                Plugin.Log.LogInfo("Currently located at the Company! Setting to default constellation!");
                if (TryGetDefaultConstellation(out ClassMapper theConst))
                {
                    if (theConst.canRouteCompany)
                    {
                        AdjustToNewConstellation(theConst.defaultMoon, theConst.consName);
                        return;
                    }
                }

                Plugin.WARNING("Unable to load default constellation from config item. Setting to first contellation in list.");
                AdjustToNewConstellation(ConstellationStuff[0].defaultMoon, ConstellationStuff[0].consName);
            }
        }

        internal static bool TryGetDefaultConstellation(out ClassMapper theConst)
        {
            if (ConstellationStuff.Count == 0)
            {
                Plugin.ERROR("Unable to detect ConstellationStuff listing!!");
                theConst = null;
                return false;
            }

            if (ClassMapper.TryGetConstellation(ConstellationStuff, Configuration.CompanyDefaultConstellation.Value, out theConst))
            {
                Plugin.Spam($"Returning {theConst.consName}");
                return true;
            }
            else
            {
                Plugin.Spam($"Config item not matching anything, returning first constellation: {ConstellationStuff[0].consName}");
                theConst = ConstellationStuff[0];
                return true;
            }
        }
    }
}
