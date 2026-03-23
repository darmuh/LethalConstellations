using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using Dawn;

namespace LethalConstellations;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    internal static System.Random Rand = new();
    internal static List<ConstellationInfo> Constellations = [];

    private void Awake()
    {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loaded!");

        LethalContent.Moons.OnFreeze += RegisterMoons;
    }

    private void RegisterMoons()
    {
        List<string> constellations = [];

        if(!string.IsNullOrEmpty(ConstellationsConfig.UserProvidedConstellations.Value))
            constellations = [.. ConstellationsConfig.UserProvidedConstellations.Value.Split(',')];

        bool customConstellations = false;

        if (constellations.Count > 0)
        {
            foreach (string constellation in constellations)
                ConstellationsConfig.MapConstellation(constellation);

            customConstellations = true;
        }

        foreach (var moon in LethalContent.Moons)
        {
            if (!customConstellations)
            {
                string newConstellation = BepinFriendlyString(moon.Key.Namespace);
                if (!Constellations.Any(x => x.Name == newConstellation))
                {
                    ConstellationsConfig.MapConstellation(newConstellation);
                }
            }

            ConstellationsConfig.MapMoon(moon);
        }

    }

    public static string BepinFriendlyString(string input)
    {
        char[] invalidChars = ['\'', '\n', '\t', '\\', '"', '[', ']'];
        string result = "";

        input = input.Trim();

        foreach (char c in input)
        {
            if (!invalidChars.Contains(c))
                result += c;
            else
                continue;
        }

        return result;
    }
}
