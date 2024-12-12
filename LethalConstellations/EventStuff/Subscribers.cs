using LethalConstellations.Compat;
using LethalConstellations.ConfigManager;
using LethalConstellations.PluginCore;
using LethalLevelLoader;
using OpenLib.Events;
using static LethalConstellations.PluginCore.SaveManager;

namespace LethalConstellations.EventStuff
{
    public class Subscribers
    {
        public static void Subscribe()
        {
            EventManager.TerminalAwake.AddListener(OnTerminalAwake);
            EventManager.TerminalStart.AddListener(OnTerminalStart);
            EventManager.TerminalLoadNewNode.AddListener(OnLoadNode);
            EventManager.StartOfRoundChangeLevel.AddListener(OnLevelChange);
            //EventManager.OnClientConnect.AddListener(OnClientConnected);
            EventManager.GameNetworkManagerStart.AddListener(OnStartup);
            EventManager.StartOfRoundStart.AddListener(NewLobbyStuff);
            EventManager.ShipReset.AddListener(NewLobbyStuff);
            LethalLevelLoader.Plugin.onSetupComplete += LLLStuff.LLLSetup;
        }

        public static void OnTerminalAwake(Terminal instance)
        {
            Plugin.instance.Terminal = instance;
            Plugin.MoreLogs($"Setting Plugin.instance.Terminal");
        }

        public static void NewLobbyStuff()
        {
            if (GameNetworkManager.Instance.isHostingGame)
                InitHostStuff();
        }

        public static void OnLoadNode(TerminalNode node)
        {
            if (LevelStuff.cancelConfirmation)
            {
                LevelStuff.cancelConfirmation = false;
                Plugin.Spam("cancelConfirmation is true and node is in confirmation, routing to dummy node");
                Plugin.instance.dummyNode.displayText = node.displayText;
                Plugin.instance.Terminal.LoadNewNode(Plugin.instance.dummyNode);

            }
        }

        public static void OnTerminalStart()
        {
            MenuStuff.PreInit();
            InitSave();
            ClassMapper.UpdatePricesBasedOnCurrent(Collections.ConstellationStuff);
        }

        public static void OnLevelChange()
        {
            Plugin.Spam("setting currentLevel");
            Plugin.Spam($"{LevelManager.CurrentExtendedLevel.NumberlessPlanetName}, {LevelManager.CurrentExtendedLevel.IsRouteLocked}, {LevelManager.CurrentExtendedLevel.IsRouteHidden}, {LevelManager.CurrentExtendedLevel.LockedRouteNodeText}");
            LevelStuff.GetCurrentConstellation(LevelManager.CurrentExtendedLevel.NumberlessPlanetName);
        }

        public static void OnStartup()
        {
            if (OpenLib.Plugin.instance.LobbyCompat)
            {
                Plugin.Log.LogInfo($"BMX_LobbyCompat detected!");
                OpenLib.Compat.BMX_LobbyCompat.SetBMXCompat(false);
            }

            if (OpenLib.Common.StartGame.SoftCompatibility("com.xmods.lethalmoonunlocks", ref Plugin.instance.LethalMoonUnlocks))
                Plugin.Log.LogInfo("LethalMoonUnlocks Detected! Disabling moon unlock/hiding from this mod.");

            if (OpenLib.Common.StartGame.SoftCompatibility("LethalNetworkAPI", ref Plugin.instance.LethalNetworkAPI))
            {
                Plugin.Log.LogInfo("NetworkApi detected, networking unlocked!");
            }

            if (OpenLib.Plugin.instance.LethalConfig)
            {
                Plugin.Log.LogInfo("LethalConfig Detected!");
                OpenLib.Compat.LethalConfigSoft.AddButton("Tools", "LoadCodes", "Click this to refresh all mod configs from code values assigned to [GeneratedConfigCode] and [MainConfigCode]", "Load Config Codes", LConfig.LoadCodesButton);
                OpenLib.Compat.LethalConfigSoft.AddButton("Tools", "GenerateWebConfigs", "Click this to generate web pages for both config files.\nThese web pages can be used to generate config codes", "Generate WebConfigs", LConfig.GenerateWebConfig);
                Plugin.Spam("Buttons added!");
            }
        }

        public static void OnClientConnected()
        {
            if (!GameNetworkManager.Instance.isHostingGame)
                return;

            if (!Plugin.instance.LethalNetworkAPI)
                return;

            if (Collections.ConstellationsOTP == null)
                return;

            if (Collections.ConstellationsOTP.Count == 0)
                return;

            NetworkThings.SyncUnlockSet(Collections.ConstellationsOTP);
        }
    }
}
