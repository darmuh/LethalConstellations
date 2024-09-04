using LethalConstellations.Compat;
using LethalLevelLoader;
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
