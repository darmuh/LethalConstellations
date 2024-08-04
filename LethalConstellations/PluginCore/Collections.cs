using System.Collections.Generic;

namespace LethalConstellations.PluginCore
{
    internal class Collections
    {
        internal static List<string> ConstellationsList = [];

        internal static Dictionary<string,string> ConstellationCats = [];
        //ConstellationCats.Add(item, $"Route to Constellation {item}\t${price}");
        
        internal static Dictionary<string, string> ConstellationsToMoons = [];
        //ConstellationsToMoons.Add(extendedLevel.NumberlessPlanetName, levelToConstellation.Value);

        internal static Dictionary<string, int> MoonPrices = [];
        //MoonPrices.Add(extendedLevel.NumberlessPlanetName, levelPrice.Value);

        internal static Dictionary<string, int> ConstellationPrices = [];
        //ConstellationPrices.Add(name, constellationPrice.Value);

        internal static Dictionary<string, string> DefaultMoons = [];
        //DefaultMoons.Add(name, defaultMoon.Value);

        internal static Dictionary<string, string> ConstellationMenuText = [];

        internal static Dictionary<string, string> ConstellationInfoNodes = [];
        //ConstellationInfoNodes.Add(name, infoText.Value);

        internal static Dictionary<string, string> CNameFix = [];
        //CNameFix.Add(item, newWord);

        internal static string CurrentConstellation;

        internal static string Constellations;

        internal static string Constellation;


        internal static void UpdateDictionaryKey(ref Dictionary<string, string> dict, Dictionary<string,string> badNames)
        {
            //CNameFix.Add(item, newWord);
            Dictionary<string, string> newDictionary = [];
            foreach (KeyValuePair<string,string> bad in badNames)
            {
                foreach (KeyValuePair<string, string> pair in dict)
                {
                    if (pair.Key == bad.Key)
                    {
                        Plugin.Spam($"Match found for badName: {bad.Key}");
                        newDictionary.Add(bad.Value, pair.Value);
                    }
                    else
                        newDictionary.Add(pair.Key, pair.Value);
                }
                if (newDictionary.Count > 0)
                    break;
            }

            dict = newDictionary;
        }

        internal static void UpdateDictionaryKey(ref Dictionary<string, int> dict, Dictionary<string, string> badNames)
        {
            //CNameFix.Add(item, newWord);
            Dictionary<string, int> newDictionary = [];
            foreach (KeyValuePair<string, string> bad in badNames)
            {
                foreach (KeyValuePair<string, int> pair in dict)
                {
                    if (pair.Key == bad.Key)
                    {
                        Plugin.Spam($"Match found for badName: {bad.Key}");
                        newDictionary.Add(bad.Value, pair.Value);
                    }
                    else
                        newDictionary.Add(pair.Key, pair.Value);
                }
                if (newDictionary.Count > 0)
                    break;
            }

            dict = newDictionary;
        }
    }
}
