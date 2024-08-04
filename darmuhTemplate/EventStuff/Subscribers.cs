using OpenLib.Events;
using Constellations.PluginCore;
using Constellations.Compat;
using Constellations.ConfigManager;
using LethalLevelLoader;

namespace Constellations.EventStuff
{
    public class Subscribers
    {
        public static void Subscribe()
        {
            EventManager.TerminalAwake.AddListener(OnTerminalAwake);
            EventManager.TerminalStart.AddListener(OnTerminalStart);
            EventManager.StartOfRoundChangeLevel.AddListener(OnLevelChange);
            //EventManager.GameNetworkManagerStart.AddListener(OnStartup);
            LethalLevelLoader.AssetBundleLoader.onBundlesFinishedLoading += OnStartup;
            LethalLevelLoader.Plugin.onSetupComplete += LLLStuff.LLLSetup;

        }

        public static void OnTerminalAwake(Terminal instance)
        {
            Plugin.instance.Terminal = instance;
            Plugin.MoreLogs($"Setting Plugin.instance.Terminal"); 
        }

        public static void OnTerminalStart()
        {
            MenuStuff.Init();
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

            if (Plugin.instance.LethalConfig)
            {
                LethalConfigCompat.QueueConfig(Configuration.GeneratedConfig);
                LethalConfigCompat.QueueConfig(Configuration.MainConfig);
            }

        }
    }
}
