
using System;
using LethalLevelLoader;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace LethalConstellations.PluginCore
{
    public class ClassMapper
    {
        static public int PriceOfOneLightYear = 50;
        public static int LightYears = 20;
        public static int TotalCostForRouteUsingLightYears = PriceOfOneLightYear * LightYears;
        internal static void UpdatePricesBasedOnCurrent(List<ClassMapper> constellations)
        {
            if (string.IsNullOrEmpty(Collections.CurrentConstellation))
            {
                Plugin.Spam("No current constellation is set.");
                return;
            }

            var currentConstellation = constellations.Find(c => c.consName == Collections.CurrentConstellation);
            if (currentConstellation == null)
            {
                Plugin.Spam($"Current constellation '{Collections.CurrentConstellation}' not found in the list.");
                return;
            }

            int currentID = currentConstellation.ConstellationID;

            foreach (var constellation in constellations)
            {
                int distance = Math.Abs(constellation.ConstellationID - currentID);
                constellation.constelPrice = TotalCostForRouteUsingLightYears * distance;
                constellation.LightYearsToTravel = LightYears * distance;

                Plugin.Spam($"Updated price for {constellation.consName} (ID: {constellation.ConstellationID}): {constellation.constelPrice}");
            }
        }

        private static int _counter = 0;
        public int ConstellationID { get; private set; }

        public string consName;
        public List<string> constelMoons = [];
        public List<string> stayHiddenMoons = [];
        public bool buyOnce;
        public int constelPrice;
        public int LightYearsToTravel;
        public string defaultMoon;
        public ExtendedLevel defaultMoonLevel;
        internal string menuText;
        internal string infoText;
        public bool isHidden;
        public string optionalParams = ""; //for use in external mods. Can be used to add information to the main menu
        public bool isLocked; //for use in other mods that will modify constellations display
        public bool oneTimePurchase;
        public bool canRouteCompany;
        internal string shortcutList;

        internal ClassMapper(string cName, int cPrice = 0, string defMoon = "", string menuText = "", int clightyears = 0)
        {
            this.LightYearsToTravel = clightyears;
            this.ConstellationID = ++_counter;
            this.consName = cName;
            this.constelPrice = cPrice;
            this.defaultMoon = defMoon;
            this.menuText = menuText;
            this.oneTimePurchase = false;
            this.isLocked = false;

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
                if (constellation.consName.ToLower() == query.ToLower() || constellation.shortcutList.ToLower().Contains(query.ToLower()))
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
