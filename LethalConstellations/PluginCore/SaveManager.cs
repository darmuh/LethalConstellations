using Steamworks.Ugc;
using System.Collections.Generic;
using static LethalConstellations.PluginCore.Collections;

namespace LethalConstellations.PluginCore
{
    internal class SaveManager
    {
        internal static void InitSave()
        {
            Plugin.Spam("InitSave");
            ConstellationsOTP.Clear();
            ClassMapper.ResetUnlockedConstellations(ConstellationStuff);

            if (!ES3.KeyExists("LethalConstellations_Unlocks", GameNetworkManager.Instance.currentSaveFileName))
            {
                Plugin.Spam("Creating save key for LethalConstellations_Unlocks");
                ES3.Save<List<string>>("LethalConstellations_Unlocks", ConstellationsOTP, GameNetworkManager.Instance.currentSaveFileName);
            }
            else
            {
                ConstellationsOTP = ES3.Load<List<string>>("LethalConstellations_Unlocks", GameNetworkManager.Instance.currentSaveFileName);
                Plugin.Spam("Updating constellation unlocks from save file");
                ClassMapper.UpdateConstellationUnlocks();
            }
        }

        internal static void SaveUnlocks(List<string> unlockList)
        {
            Plugin.Spam("saving LethalConstellations_Unlocks");
            ES3.Save<List<string>>("LethalConstellations_Unlocks", unlockList, GameNetworkManager.Instance.currentSaveFileName);
        }


    }
}
