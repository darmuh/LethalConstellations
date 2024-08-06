using System.Collections.Generic;
using System.Text;
using static LethalConstellations.PluginCore.Collections;

namespace LethalConstellations.PluginCore
{
    internal class MainMenu
    {
        internal static string MainMenuText(string MainMenuText, Dictionary<string, string> Categories)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(MainMenuText + "\r\n");
            stringBuilder.Append($"Current {ConstellationWord}: {CurrentConstellation}\r\n\r\n");
            if (Categories.Count > 0)
            {
                foreach (KeyValuePair<string, string> Category in Categories)
                {
                    stringBuilder.Append("[" + Category.Key.ToUpper() + "]\r\n" + Category.Value + "\r\n\r\n");
                }
            }
            else
                stringBuilder.Append($"Unable to route to any {ConstellationsWord} at this time!\r\n\r\n");

            return stringBuilder.ToString();
        }

        internal static string ReturnMainMenu()
        {
            MenuStuff.CreateConstellationCategories();
            return MainMenuText($"========== {ConstellationsWord} Routing Matrix ==========\r\n", ConstellationCats);
        }
    }
}
