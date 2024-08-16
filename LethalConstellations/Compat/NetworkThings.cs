using LethalConstellations.PluginCore;
using System.Collections.Generic;
using LethalNetworkAPI;
using LethalNetworkAPI.Utils;


namespace LethalConstellations.Compat
{
    internal class NetworkThings
    {

        internal static void InitNetworkThings()
        {
            if (!Plugin.instance.LethalNetworkAPI)
                return;

            LNetworkEvent consUnlockedSyncREQ = LNetworkEvent.Connect("consUnlockedSyncREQ", HostClientSync);
            LNetworkMessage<List<string>> constellationsUnlocked = LNetworkMessage<List<string>>.Connect("constellationsUnlocked", SyncUnlockHost, SyncUnlockClient);

        }

        internal static void SyncUnlockSet(List<string> unlockedConst)
        {
            if (!Plugin.instance.LethalNetworkAPI)
                return;

            if(unlockedConst.Count == 0)
            {
                Plugin.WARNING("Attempting to sync blank ConstellationsOTP!!!");
            }

            LNetworkMessage<List<string>> constellationsUnlocked = LNetworkMessage<List<string>>.Connect("constellationsUnlocked", SyncUnlockHost, SyncUnlockClient);

            if(LNetworkUtils.IsHostOrServer)
                constellationsUnlocked.SendClients(unlockedConst, LNetworkUtils.AllConnectedClients);
            else
                constellationsUnlocked.SendServer(unlockedConst);

            //Collections.ConstellationsOTP = unlockedConst;
        }

        internal static void RequestSyncFromHost()
        {
            if (!Plugin.instance.LethalNetworkAPI)
                return;

            Plugin.Spam("Requesting sync from host");
            LNetworkMessage<List<string>> constellationsUnlocked = LNetworkMessage<List<string>>.Connect("constellationsUnlocked", null, SyncUnlockClient);

            LNetworkEvent consUnlockedSyncREQ = LNetworkEvent.Connect("consUnlockedSyncREQ", HostClientSync);
            consUnlockedSyncREQ.InvokeServer();
        }

        internal static void HostClientSync(ulong clientRequesting)
        {
            if (!Plugin.instance.LethalNetworkAPI)
                return;

            Plugin.Spam($"HostClientSync, requested by {clientRequesting}");

            if (!LNetworkUtils.IsHostOrServer)
                return;

            Plugin.Spam("Host sending collection");

            SyncUnlockSet(Collections.ConstellationsOTP);
        }

        internal static void SyncUnlockHost(List<string> newValue, ulong clientSending)
        {
            if (!Plugin.instance.LethalNetworkAPI)
                return;

            //Plugin.Spam("Host received new list");
            if (Collections.ConstellationsOTP == newValue)
                return;

            Plugin.Spam($"SYNCING NEW UNLOCK LIST - HOST");
            Collections.ConstellationsOTP = newValue;
            ClassMapper.UpdateConstellationUnlocks();
            SyncUnlockSet(Collections.ConstellationsOTP);
            SaveManager.SaveUnlocks(Collections.ConstellationsOTP);
        }

        internal static void SyncUnlockClient(List<string> newValue)
        {
            if (!Plugin.instance.LethalNetworkAPI)
                return;

            //Plugin.Spam("Sync Called");

            if (LNetworkUtils.IsHostOrServer)
                return;

            //Plugin.Spam("Not host");

            if (Collections.ConstellationsOTP == newValue)
                return;

            Plugin.Spam($"SYNCING NEW UNLOCK LIST");
            Collections.ConstellationsOTP = newValue;
            ClassMapper.UpdateConstellationUnlocks();
        }
    }
}
