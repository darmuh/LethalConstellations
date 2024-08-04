using BepInEx.Configuration;
using LethalConfig;
namespace Constellations.Compat
{
    internal class LethalConfigCompat
    {
        internal static void QueueConfig(ConfigFile configName)
        {
            if (!Plugin.instance.LethalConfig)
                return;

            Plugin.Log.LogInfo($"Queuing file {configName.ConfigFilePath}");
            LethalConfigManager.QueueCustomConfigFileForAutoGeneration(configName);
        }
    }
}
