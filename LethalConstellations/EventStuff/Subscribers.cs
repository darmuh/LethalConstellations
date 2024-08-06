using OpenLib.Events;
using LethalConstellations.PluginCore;
using LethalConstellations.Compat;
using LethalConstellations.ConfigManager;
using LethalLevelLoader;

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
            //EventManager.GameNetworkManagerStart.AddListener(OnStartup);
            AssetBundleLoader.onBundlesFinishedLoading += OnStartup;
            LethalLevelLoader.Plugin.onSetupComplete += LLLStuff.LLLSetup;
        }

        public static void OnTerminalAwake(Terminal instance)
        {
            Plugin.instance.Terminal = instance;
            Plugin.MoreLogs($"Setting Plugin.instance.Terminal");
        }

        public static void OnLoadNode(TerminalNode node)
        {
            if(node.terminalOptions != null && LevelStuff.cancelConfirmation)
            {
                Plugin.Spam("cancelConfirmation is true and node is in confirmation, routing to dummy node");
                Plugin.instance.dummyNode.displayText = node.displayText;
                Plugin.instance.Terminal.LoadNewNode(Plugin.instance.dummyNode);
            }
            else
            {
                Plugin.Spam("setting cancelConfirmation to false");
                LevelStuff.cancelConfirmation = false;
            }
        }

        public static void OnTerminalStart()
        {
            MenuStuff.PreInit();
        }

        public static void OnLevelChange()
        {
            Plugin.Spam("setting currentLevel");
            Plugin.Spam($"{LevelManager.CurrentExtendedLevel.NumberlessPlanetName}, {LevelManager.CurrentExtendedLevel.IsRouteLocked}, {LevelManager.CurrentExtendedLevel.IsRouteHidden}, {LevelManager.CurrentExtendedLevel.LockedRouteNodeText}");
            LevelStuff.GetCurrentConstellation(LevelManager.CurrentExtendedLevel.NumberlessPlanetName);
        }

        public static void OnStartup()
        {
            if (OpenLib.Common.StartGame.SoftCompatibility("BMX.LobbyCompatibility", ref Plugin.instance.LobbyCompat))
            {
                Plugin.Log.LogInfo($"BMX_LobbyCompat detected!");
                BMX_LobbyCompat.SetCompat(false);
            }
            if (OpenLib.Common.StartGame.SoftCompatibility("ainavt.lc.lethalconfig", ref Plugin.instance.LethalConfig))
                Plugin.Log.LogInfo("LethalConfig Detected!");

        }
    }
}
