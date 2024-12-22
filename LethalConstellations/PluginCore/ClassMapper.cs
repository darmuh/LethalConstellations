using LethalConstellations.Compat;
using LethalConstellations.ConfigManager;
using LethalLevelLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LethalConstellations.PluginCore
{

    public class ClassMapper
    {
        public string consName;
        public List<string> constelMoons = [];
        public List<string> stayHiddenMoons = [];
        public bool buyOnce;
        public int constelPrice;
        public Vector3 RelativePosition;
        internal string positionalPriceMode;
        public float originalDistance; //original currentDistance to starter constellation
        public string defaultMoon;
        public ExtendedLevel defaultMoonLevel;
        internal string menuText;
        internal string infoText;
        public bool isHidden;
        public string optionalParams = ""; //for use in external mods. Can be used to add information to the main menu
        public bool isLocked; //for use in other mods that will modify constellations display
        public bool oneTimePurchase;
        public bool canRouteCompany;
        internal List<string> shortcutList;


        internal ClassMapper(string cName, int cPrice = 0, string defMoon = "", string mText = "")
        {
            consName = cName;
            constelPrice = cPrice;
            defaultMoon = defMoon;
            menuText = mText;
            oneTimePurchase = false;
            isLocked = false;
        }

        internal void SetPosition(string value)
        {
            List<string> coords = [.. value.Split(',')];
            coords.RemoveAll(x => x.Length == 0);

            RelativePosition = Vector3.zero;
            int x = 0;
            int y = 0;
            int z = 0;

            if (coords.Count == 3)
            {
                bool canUse = true;
                for (int i = 0; i < coords.Count; i++)
                {
                    if (int.TryParse(coords[i], out int pos))
                    {
                        if (i == 0)
                            x = pos;
                        else if (i == 1)
                            y = pos;
                        else
                            z = pos;
                    }
                    else
                        canUse = false;
                }

                if (canUse)
                    RelativePosition = new(x, y, z);
            }
        }

        internal void SetOriginalDistance()
        {
            originalDistance = Vector3.Distance(RelativePosition, Collections.StarterConstellation.RelativePosition);

            if (positionalPriceMode == "SetPriceByDistance")
                constelPrice = Configuration.CostPerDistanceUnit.Value * (int)originalDistance;

            Plugin.Spam($"Price for constellation set to {constelPrice} ({Configuration.CostPerDistanceUnit.Value} x (int)originalDistance)");
        }

        internal void OneTimePurchase()
        {
            if (oneTimePurchase)
                return;

            if (!buyOnce || !Plugin.instance.LethalNetworkAPI)
                return;
            else
            {
                Plugin.Spam("Updating oneTimePurchase to true");
                oneTimePurchase = true;
                constelPrice = 0;
                if (!Collections.ConstellationsOTP.Contains(consName))
                {
                    Collections.ConstellationsOTP.Add(consName);
                    SaveManager.SaveUnlocks(Collections.ConstellationsOTP);
                    NetworkThings.SyncUnlockSet(Collections.ConstellationsOTP);
                }
                else
                    Plugin.WARNING($"--- Error with oneTimePurchaseCheck, already in list ---");
            }
        }

        internal int GetDistance()
        {
            if (!Configuration.AddConstellationPositionData.Value)
                return -1;

            if (Collections.CurrentConstellationCM == null)
                return -1;

            return (int)Vector3.Distance(Collections.CurrentConstellationCM.RelativePosition, RelativePosition);
        }


        internal static void UpdateCNames(List<ClassMapper> constellations, Dictionary<string, string> fixedNames)
        {
            if (constellations.Count < 1 || fixedNames.Count < 1)
                return;
            foreach (KeyValuePair<string, string> fixer in fixedNames)
            {
                if (CheckForAndUpdateCName(constellations, fixer.Key, fixer.Value))
                {
                    Plugin.Spam($"CheckForAndUpadteCName success");
                }
            }
        }

        //for mods like darmuhsTerminalStuff that have a random moon command and need to check if level is in current constellation
        //added optional constellation string param if wanting to check a specific constellation
        public static bool IsLevelInConstellation(SelectableLevel level, string constellation = "")
        {
            if (level == null)
                return false;

            if (constellation.Length < 1)
                constellation = Collections.CurrentConstellation;
            string numberlessName = ExtendedLevel.GetNumberlessPlanetName(level);
            if (TryGetConstellation(Collections.ConstellationStuff, constellation, out ClassMapper currentConst))
            {
                if (currentConst.constelMoons.Contains(numberlessName))
                    return true;
                else
                    return false;
            }
            return false;
        }

        public static bool TryGetConstellation(List<ClassMapper> constellations, string query, out ClassMapper outConst)
        {
            Plugin.Spam("TryGetConstellation:");
            Plugin.Spam(query);
            if (constellations.Count < 1 || query.Length < 1)
            {
                Plugin.Spam("Invalid search in TryGetConstellation()");
                outConst = null!;
                return false;
            }

            foreach (ClassMapper constellation in constellations)
            {
                if (constellation.consName.ToLowerInvariant() == query.ToLowerInvariant() || constellation.shortcutList.Any(x => x.ToLowerInvariant() == query.ToLowerInvariant()))
                {
                    Plugin.Spam($"constellation found with name: {constellation.consName}");
                    outConst = constellation;
                    return true;
                }
            }

            Plugin.Spam($"Cannot find {query}");
            outConst = null!;
            return false;
        }

        internal static bool CheckForAndUpdateCName(List<ClassMapper> constellations, string query, string newValue)
        {
            if (constellations.Count < 1 || query.Length < 1)
            {
                Plugin.Spam("Invalid search in CheckForCName()");
                return false;
            }

            foreach (ClassMapper constellation in constellations)
            {
                if (constellation.consName == query)
                {
                    constellation.consName = newValue;
                    Plugin.Spam($"bad name {query} updated to {constellation.consName}");
                    return true;
                }
            }

            Plugin.Spam($"Unable to find bad name - {query}");
            return false;
        }


        internal static void UpdateConstellationUnlocks()
        {
            if (Collections.ConstellationsOTP.Count == 0)
                return;

            foreach (string item in Collections.ConstellationsOTP)
            {
                if (TryGetConstellation(Collections.ConstellationStuff, item, out ClassMapper outConst))
                {
                    if (outConst == null)
                        continue;

                    if (outConst.buyOnce && !outConst.oneTimePurchase)
                        outConst.oneTimePurchase = true;

                    Plugin.Spam($"{outConst.consName} detected as unlocked from save file");

                }
            }
        }

        internal static void ResetUnlockedConstellations(List<ClassMapper> constellations)
        {
            if (constellations.Count == 0)
                return;

            foreach (ClassMapper item in constellations)
            {
                item.oneTimePurchase = false;
                Plugin.Spam($"{item.consName} OTP has been reset");
            }
        }
    }
}
