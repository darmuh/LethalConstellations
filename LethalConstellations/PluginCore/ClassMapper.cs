
using System.Collections.Generic;

namespace LethalConstellations.PluginCore
{
    internal class ClassMapper
    {
        internal string consName;
        internal List<string> constelMoons;
        internal int constelPrice;
        internal string defaultMoon;
        internal string menuText;
        internal string infoText;
        internal bool isHidden;
        internal bool canRouteCompany;
        internal string shortcutList;

        internal ClassMapper(string cName, int cPrice = 0, string defMoon = "", string menuText = "")
        {
            this.consName = cName;
            this.constelPrice = cPrice;
            this.defaultMoon = defMoon;
            this.menuText = menuText;
        }

        internal static void UpdateCNames(List<ClassMapper> constellations, Dictionary<string,string> fixedNames)
        {
            if (constellations.Count < 1 || fixedNames.Count < 1)
                return;
                foreach (KeyValuePair<string, string> fixer in fixedNames)
                {
                    if(CheckForAndUpadteCName(constellations, fixer.Key, fixer.Value))
                    {
                        Plugin.Spam($"CheckForAndUpadteCName success");
                    }
                }
        }

        internal static bool TryGetConstellation(List<ClassMapper> constellations, string query, out ClassMapper outConst)
        {
            Plugin.Spam("TryGetConstellation:");
            Plugin.Spam(query);
            if (constellations.Count < 1 || query.Length < 1)
            {
                Plugin.Spam("Invalid search in TryGetConstellation()");
                outConst = null;
                return false;
            }

            foreach (ClassMapper constellation in constellations)
            {
                if (constellation.consName.ToLower() == query.ToLower() || constellation.shortcutList.ToLower().Contains(query.ToLower()))
                {
                    Plugin.Spam($"constellation found with name: {constellation.consName}");
                    outConst = constellation;
                    return true;
                }
            }

            Plugin.Spam($"Cannot find {query}");
            outConst = null;
            return false;
        }

        internal static bool CheckForAndUpadteCName(List<ClassMapper> constellations, string query, string newValue)
        {
            if (constellations.Count < 1 || query.Length < 1)
            {
                Plugin.Spam("Invalid search in CheckForCName()");
                return false;
            }

            foreach(ClassMapper constellation in constellations)
            {
                if(constellation.consName == query)
                {
                    constellation.consName = newValue;
                    Plugin.Spam($"bad name {query} updated to {constellation.consName}");
                    return true;
                }
            }

            Plugin.Spam($"Unable to find bad name - {query}");
            return false;
        }
    }
}
