using LethalConstellations.ConfigManager;
using LethalLevelLoader;
using System.Collections.Generic;
using static LethalConstellations.PluginCore.Collections;

namespace LethalConstellations.PluginCore
{
    internal class MoonStuff
    {
        internal static void ModifyMoonPrices()
        {
            Plugin.Spam("MODIFYMOONPRICES");
            if (MoonPrices.Count < 1 || !Configuration.ModifyMoonPrices.Value)
                return;

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                if (TryGetMoon(extendedLevel.NumberlessPlanetName, MoonPrices, out int newPrice))
                {
                    extendedLevel.RoutePrice = newPrice;
                }
            }
        }

        internal static ExtendedLevel GetExtendedLevel(string levelName)
        {
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                if (extendedLevel.NumberlessPlanetName.ToLower() == levelName.ToLower())
                {
                    return extendedLevel;
                }
            }

            Plugin.WARNING($"Unable to get extendedLevel from {levelName}");
            return null;
        }

        internal static bool TryGetMoon(string levelName, Dictionary<string, int> moonPrices, out int price)
        {
            price = -1;

            if(moonPrices.Count == 0)
                return false;

            foreach(KeyValuePair<string, int> pair in moonPrices)
            {
                if (pair.Key == levelName)
                {
                    price = pair.Value;
                    return true;
                }   
            }

            return false;
        }
    }
}
