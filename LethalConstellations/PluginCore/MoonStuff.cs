using Constellations.ConfigManager;
using LethalLevelLoader;
using System.Collections.Generic;
using static Constellations.PluginCore.Collections;

namespace Constellations.PluginCore
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

        internal static int GetLevelPrice(string levelName)
        {
            if (MoonPrices.Count < 1)
                return 0;

            foreach (KeyValuePair<string, int> item in MoonPrices)
            {
                if (item.Key == levelName)
                {
                    return item.Value;
                }
            }

            return 0;
        }
    }
}
