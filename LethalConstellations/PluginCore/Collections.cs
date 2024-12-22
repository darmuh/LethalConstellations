using LethalLevelLoader;
using System.Collections.Generic;

namespace LethalConstellations.PluginCore
{
    public class Collections
    {
        internal static List<string> ConstellationsList = [];
        internal static Dictionary<string, string> ManualSetupList = [];
        internal static System.Random Rand = new();

        public static List<ClassMapper> ConstellationStuff = []; //public for access from other mods (complete list)

        public static List<ClassMapper> DisplayConstellations = []; //used for displaying in the constellations menu (for filtering complete list)

        internal static Dictionary<string, string> ConstellationCats = [];
        //ConstellationCats.Add(item, $"Route to ConstellationWord {item}\t${price}");

        internal static Dictionary<ExtendedLevel, int> MoonPrices = [];
        //MoonPrices.Add(extendedLevel.NumberlessPlanetName, levelPrice.Value);

        internal static Dictionary<string, string> CNameFix = [];
        //CNameFix.Add(item, newWord);

        internal static string CompanyMoon = "Gordion";

        public static string CurrentConstellation = ""; //easy way to get current constellation name

        public static ClassMapper CurrentConstellationCM;

        internal static string ConstellationsWord;

        internal static string ConstellationWord;

        internal static TerminalNode ConstellationsNode;
        internal static TerminalNode RouteShortcutNode;

        internal static List<string> ConstellationsOTP = [];

        internal static ClassMapper StarterConstellation;

        internal static void Start()
        {
            MoonPrices.Clear();
        }

        internal static bool TryGetConstellationFromScreenText(out ClassMapper outConst, out string failText)
        {
            failText = "General Error, unexpected screen text...\r\n\r\n";
            Plugin.Spam("Getting screen text");
            string screen = Plugin.instance.Terminal.screenText.text.Substring(Plugin.instance.Terminal.screenText.text.Length - Plugin.instance.Terminal.textAdded);
            Plugin.Spam(screen);
            Plugin.Spam($"{ConstellationStuff.Count}");
            if (ClassMapper.TryGetConstellation(ConstellationStuff, screen, out outConst))
            {
                Plugin.Spam($"Current Constellation: {CurrentConstellation}");
                if (CurrentConstellation == outConst.consName)
                {
                    failText = $"You are already located at {ConstellationWord} - {CurrentConstellation}...\r\n\r\n";
                    return false;
                }

                if (outConst.isLocked)
                {
                    failText = $"Unable to travel to {outConst.consName}. {ConstellationWord} is locked!\r\n\r\n";
                    return false;
                }

                return true;
            }
            else
            {
                failText = $"Unable to travel to {ConstellationWord} provided in command [ {screen} ]\r\n\r\n";
            }

            return false;
        }
    }
}
