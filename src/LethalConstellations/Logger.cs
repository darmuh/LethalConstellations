using System;
using System.Collections.Generic;
using System.Text;

namespace LethalConstellations;

//ripped from openlib
public class Loggers
{
    public enum LoggingLevel
    {
        WarningsPlus = 4, //Includes bepinex warnings (4), error (2), and fatal (1)
        Message = 8, // Includes bepinex messages (8) and all of above
        Info = 0x10, // Includes bepinex info (0x10) and all of above
        Debug = 0x20 // Includes bepinex debug (0x20) and all of above
    }

    private static void Log(BepInEx.Logging.LogLevel bepLevel, object data)
    {
        int settingVal = (int)ConstellationsConfig.LogLevel.Value;
        int messageLevel = (int)bepLevel;

        if (messageLevel > settingVal)
            return;

        Plugin.Log.Log(bepLevel, data);

    }

    internal static void LogDebug(object data)
    {
        Log(BepInEx.Logging.LogLevel.Debug, data);
    }

    internal static void LogInfo(object data)
    {
        Log(BepInEx.Logging.LogLevel.Info, data);
    }

    internal static void LogMessage(object data)
    {
        Log(BepInEx.Logging.LogLevel.Message, data);
    }

    internal static void WARNING(object data)
    {
        Log(BepInEx.Logging.LogLevel.Warning, data);
    }

    internal static void ERROR(object data)
    {
        Log(BepInEx.Logging.LogLevel.Error, data);
    }

    internal static void FATAL(object data)
    {
        Log(BepInEx.Logging.LogLevel.Fatal, data);
    }
}
