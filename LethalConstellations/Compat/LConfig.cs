using BepInEx.Configuration;
using LethalConfig;
using LethalConstellations.ConfigManager;

namespace LethalConstellations.Compat
{
    internal class LConfig
    {
        internal static void QueueConfig(ConfigFile configName)
        {
            if (!OpenLib.Plugin.instance.LethalConfig)
                return;

            if (OpenLib.Compat.LethalConfigSoft.IsLethalConfigUpdated())
            {
                Plugin.Spam("Queuing file " + configName.ConfigFilePath);
                LethalConfigManager.QueueCustomConfigFileForLateAutoGeneration(configName);
            }
        }

        internal static void LoadCodesButton()
        {
            if (!OpenLib.Plugin.instance.LethalConfig)
                return;

            if (Configuration.GeneratedConfigCode.Value.Length > 1)
                OpenLib.ConfigManager.WebHelper.ReadCompressedConfig(ref Configuration.GeneratedConfigCode, Configuration.GeneratedConfig);
            if (Configuration.MainConfigCode.Value.Length > 1)
                OpenLib.ConfigManager.WebHelper.ReadCompressedConfig(ref Configuration.MainConfigCode, Plugin.instance.Config);
        }

        internal static void GenerateWebConfig()
        {
            if (!OpenLib.Plugin.instance.LethalConfig)
                return;

            OpenLib.ConfigManager.WebHelper.WebConfig(Configuration.GeneratedConfig);
            OpenLib.ConfigManager.WebHelper.WebConfig(Plugin.instance.Config);
        }
    }
}
