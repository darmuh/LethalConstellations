using LethalConstellations.ConfigManager;
using LethalLevelLoader;
using System.Collections.Generic;
using System.Linq;
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

            foreach (KeyValuePair<ExtendedLevel, int> moon in MoonPrices)
            {
                Plugin.Spam($"Modifying {moon.Key.NumberlessPlanetName} price from {moon.Key.RoutePrice} to {moon.Value}");
                moon.Key.RoutePrice = moon.Value;
            }
        }

        internal static ExtendedLevel GetExtendedLevel(string levelName)
        {
            return PatchedContent.ExtendedLevels.FirstOrDefault(e => e.NumberlessPlanetName.ToLower() == levelName.ToLower());
        }

        internal static bool TryGetMoon(string levelName, Dictionary<string, int> moonPrices, out int price)
        {
            price = -1;

            if (moonPrices.Count == 0)
                return false;

            if (moonPrices.Any(pair => pair.Key.ToLower() == levelName.ToLower()))
            {
                price = moonPrices.FirstOrDefault(pair => pair.Key.ToLower() == levelName.ToLower()).Value;
                return true;
            }
            else
                return false;
        }
    }
}
