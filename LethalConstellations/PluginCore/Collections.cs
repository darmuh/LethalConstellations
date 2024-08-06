using System.Collections.Generic;

namespace LethalConstellations.PluginCore
{
    internal class Collections
    {
        internal static List<string> ConstellationsList = [];

        internal static List<ClassMapper> ConstellationStuff = [];

        internal static Dictionary<string,string> ConstellationCats = [];
        //ConstellationCats.Add(item, $"Route to ConstellationWord {item}\t${price}");

        internal static Dictionary<string, int> MoonPrices = [];
        //MoonPrices.Add(extendedLevel.NumberlessPlanetName, levelPrice.Value);

        internal static Dictionary<string, string> CNameFix = [];
        //CNameFix.Add(item, newWord);

        internal static string CompanyMoon = "Gordion";

        internal static string CurrentConstellation;

        internal static string ConstellationsWord;

        internal static string ConstellationWord;

        internal static TerminalNode ConstellationsNode;

        internal static void Start()
        {
            MoonPrices.Clear();
        }
    }
}
