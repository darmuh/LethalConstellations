using LethalConstellations.Compat;
using LethalConstellations.ConfigManager;
using LethalLevelLoader;
using OpenLib.Common;
using System.Collections.Generic;
using static LethalConstellations.PluginCore.Collections;

namespace LethalConstellations.PluginCore
{
    internal class SaveManager
    {
        internal static void InitSave()
        {
            Plugin.Spam("InitSave");

            if (!Plugin.instance.LethalNetworkAPI)
            {
                Plugin.WARNING("Networking is disabled!");
                LevelStuff.DefaultConstellation(); //set current constellation
                return;
            }


            ConstellationsOTP.Clear();
            ClassMapper.ResetUnlockedConstellations(ConstellationStuff);
            NetworkThings.InitNetworkThings();

            if (!GameNetworkManager.Instance.isHostingGame)
            {
                NetworkThings.RequestSyncFromHost();
                return;
            }

            InitUnlocks();
            InitCurrent();

        }

        internal static void InitHostStuff()
        {
            if (!GameNetworkManager.Instance.isHostingGame)
                return;

            if(StartOfRound.Instance.gameStats.daysSpent == 0 && StartOfRound.Instance.defaultPlanet == StartOfRound.Instance.currentLevelID)
            {
                if(Configuration.StartingConstellation.Value.Length > 0)
                {
                    if(Configuration.StartingConstellation.Value == "~random~")
                    {
                        ClassMapper starter = null!;

                        if(Configuration.AcceptableStartingConstellations.Value.Length > 0)
                        {
                            int chosenIndex;
                            List<string> acceptableStarters = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.AcceptableStartingConstellations.Value, ',');
                            acceptableStarters.RemoveAll(x => x.Length < 1);
                            int loopCount = 0;

                            do
                            {
                                chosenIndex = Rand.Next(acceptableStarters.Count);
                                loopCount++;
                                if(ClassMapper.TryGetConstellation(ConstellationStuff, acceptableStarters[chosenIndex], out starter))
                                {
                                    Plugin.Spam($"Random Starter Constellation Chosen! [ {starter.consName} ]");
                                }
                                else if(loopCount > 10)
                                {
                                    Plugin.WARNING($"FAILED TO GRAB A VALID RANDOM STARTER CONSTELLATION FROM LIST [ {Configuration.AcceptableStartingConstellations.Value} ]");
                                    Plugin.WARNING($"Setting Constellation to ANY random constellation from listing (Count:{ConstellationStuff.Count}");
                                    starter = ConstellationStuff[Rand.Next(ConstellationStuff.Count)];
                                }

                            } while (starter == null);
                            
                        }
                        else
                            starter = ConstellationStuff[Rand.Next(ConstellationStuff.Count)];
                        if (StartOfRound.Instance.currentLevel != starter.defaultMoonLevel.SelectableLevel)
                        {
                            StartOfRound.Instance.ChangeLevelServerRpc(starter.defaultMoonLevel.SelectableLevel.levelID, Plugin.instance.Terminal.groupCredits);
                            StartOfRound.Instance.defaultPlanet = starter.defaultMoonLevel.SelectableLevel.levelID;
                            Plugin.Spam($"Setting level to [ {starter.defaultMoonLevel.NumberlessPlanetName} ] from Random Starter Constellation!");
                            return;
                        }

                        LevelStuff.AdjustToNewConstellation(starter.defaultMoon, starter.consName);
                        StartOfRound.Instance.defaultPlanet = starter.defaultMoonLevel.SelectableLevel.levelID;
                        Plugin.Spam($"Constellation set to [ {starter.consName} ] from Random Starter Constellation!");
                        return;
                    }

                    if (ClassMapper.TryGetConstellation(ConstellationStuff, Configuration.StartingConstellation.Value, out ClassMapper outConst))
                    {
                        StartOfRound.Instance.ChangeLevelServerRpc(outConst.defaultMoonLevel.SelectableLevel.levelID, Plugin.instance.Terminal.groupCredits);
                        Plugin.Spam($"Setting level to [ {outConst.defaultMoonLevel.NumberlessPlanetName} ]");
                        StartOfRound.Instance.defaultPlanet = outConst.defaultMoonLevel.SelectableLevel.levelID;
                    }    
                    else
                        Plugin.WARNING($"Unable to set constellation to StartingConstellation config - [ {Configuration.StartingConstellation.Value} ]");
                }
            }
        }

        internal static void InitUnlocks()
        {
            if (!ES3.KeyExists("LethalConstellations_Unlocks", GameNetworkManager.Instance.currentSaveFileName))
            {
                Plugin.Spam("Creating save key for LethalConstellations_Unlocks");
                ES3.Save<List<string>>("LethalConstellations_Unlocks", ConstellationsOTP, GameNetworkManager.Instance.currentSaveFileName);
                NetworkThings.SyncUnlockSet(ConstellationsOTP);
            }
            else
            {
                ConstellationsOTP = ES3.Load<List<string>>("LethalConstellations_Unlocks", GameNetworkManager.Instance.currentSaveFileName);
                Plugin.Spam("Updating constellation unlocks from save file");
                NetworkThings.SyncUnlockSet(ConstellationsOTP);
                ClassMapper.UpdateConstellationUnlocks();
            }
        }

        internal static void SaveUnlocks(List<string> unlockList)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
                return;

            Plugin.Spam("saving LethalConstellations_Unlocks");
            ES3.Save<List<string>>("LethalConstellations_Unlocks", unlockList, GameNetworkManager.Instance.currentSaveFileName);
        }

        internal static void InitCurrent()
        {

            if (!ES3.KeyExists("LethalConstellations_Current", GameNetworkManager.Instance.currentSaveFileName))
            {
                Plugin.Spam("Creating save key for LethalConstellations_Current");
                LevelStuff.GetCurrentConstellation(LevelManager.CurrentExtendedLevel.NumberlessPlanetName);
                string current = CurrentConstellation;
                Plugin.Spam($"CurrentConstellation: {CurrentConstellation}");
                ES3.Save<string>("LethalConstellations_Current", CurrentConstellation, GameNetworkManager.Instance.currentSaveFileName);
                NetworkThings.SyncCurrentSet(current);
            }
            else
            {
                string fromSave = ES3.Load<string>("LethalConstellations_Current", GameNetworkManager.Instance.currentSaveFileName);
                Plugin.Spam($"CurrentConstellation fromSave: {fromSave}");
                Plugin.Spam("Updating current constellation from save file");
                NetworkThings.SyncCurrentSet(fromSave);
            }
        }

        internal static void SaveCurrentConstellation(string currentConstellation)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
                return;

            Plugin.Spam("saving LethalConstellations_Current");
            ES3.Save<string>("LethalConstellations_Current", currentConstellation, GameNetworkManager.Instance.currentSaveFileName);
        }


    }
}
