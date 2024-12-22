using LethalConstellations.ConfigManager;
using LethalLevelLoader;
using System.Collections.Generic;
using UnityEngine;
using static LethalConstellations.PluginCore.Collections;

namespace LethalConstellations.PluginCore
{
    internal class LevelStuff
    {
        internal static bool gotConstellation = false;
        internal static bool cancelConfirmation = false;
        internal static string constellationName = "";
        internal static ClassMapper nextConstellation;

        internal static void ResetConstVars()
        {
            cancelConfirmation = true;
            gotConstellation = false;
            constellationName = "";
        }

        internal static void UpdateLevelList(ClassMapper thisConstellation, bool enableMoons)
        {
            if (!enableMoons)
            {
                Plugin.Spam($"Disabling all moons in: {thisConstellation.consName}");
                foreach (string name in thisConstellation.constelMoons)
                {
                    AdjustExtendedLevel(name, thisConstellation, false);
                }
            }
            else
            {
                Plugin.Spam($"Enabling all moons in: {thisConstellation.consName}");
                foreach (string name in thisConstellation.constelMoons)
                {
                    AdjustExtendedLevel(name, thisConstellation, true);
                }
            }
        }

        internal static void AdjustExtendedLevel(string levelName, ClassMapper myConst, bool thisConstellation)
        {
            if (Plugin.instance.LethalMoonUnlocks)
                return;

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                if (extendedLevel.NumberlessPlanetName.ToLower() == levelName.ToLower())
                {
                    if (!thisConstellation)
                    {
                        Plugin.Spam($"{extendedLevel.NumberlessPlanetName} should be DISABLED");
                        extendedLevel.IsRouteHidden = true;
                        extendedLevel.IsRouteLocked = true;
                        extendedLevel.LockedRouteNodeText = $"{extendedLevel.NumberlessPlanetName} is not located in this {ConstellationWord}.\n\n\tType \"{ConstellationsWord}\" to see a listing of LethalConstellations.\r\n\r\n";
                    }
                    else
                    {
                        Plugin.Spam($"{extendedLevel.NumberlessPlanetName} should be ENABLED");
                        extendedLevel.IsRouteLocked = false;
                        if (!myConst.stayHiddenMoons.Contains(extendedLevel.NumberlessPlanetName) && extendedLevel.NumberlessPlanetName.ToLower() != CompanyMoon.ToLower())
                            extendedLevel.IsRouteHidden = false;
                        extendedLevel.LockedRouteNodeText = "";
                    }
                }
            }
        }

        internal static void AdjustToNewConstellation(string levelName, string constellationName)
        {
            if (constellationName.Length == 0)
            {
                Plugin.WARNING("Invalid constellationName at AdjustToNewConstellation");
                return;
            }

            Plugin.Spam($"Adjusting to {constellationName} from {levelName}");

            if (levelName.ToLower() == CompanyMoon.ToLower() && ClassMapper.TryGetConstellation(ConstellationStuff, constellationName, out ClassMapper conClass))
            {
                AdjustExtendedLevel(conClass.defaultMoon, conClass, conClass.canRouteCompany);
                return;
            }

            //ConstellationsToMoons.Add(extendedLevel.NumberlessPlanetName, levelToConstellation.Value);
            foreach (ClassMapper item in ConstellationStuff)
            {
                if (item.consName == constellationName)
                {
                    UpdateLevelList(item, true);

                    if (item.canRouteCompany)
                    {
                        AdjustExtendedLevel(CompanyMoon, item, true);
                    }
                    else
                    {
                        AdjustExtendedLevel(CompanyMoon, item, false);
                    }
                }
                else
                    UpdateLevelList(item, false);
            }

            CurrentConstellation = constellationName;
            if (GameNetworkManager.Instance.isHostingGame)
                SaveManager.SaveCurrentConstellation(CurrentConstellation);
        }


        internal static void GetCurrentConstellation(string levelName, bool mustUpdate)
        {
            if (levelName.Length == 0)
            {
                Plugin.WARNING("Invalid levelName at GetCurrentConstellation");
                return;
            }

            if (levelName.ToLower() == CompanyMoon.ToLower() && mustUpdate)
            {
                DefaultConstellation();
                return;
            }

            //ConstellationsToMoons.Add(extendedLevel.NumberlessPlanetName, levelToConstellation.Value);
            foreach (ClassMapper item in ConstellationStuff)
            {
                List<string> lowerCaseMoons = item.constelMoons.ConvertAll(x => x.ToLower());
                if (lowerCaseMoons.Contains(levelName.ToLower()))
                {
                    CurrentConstellation = item.consName;
                    CurrentConstellationCM = item;
                    Plugin.Spam($"DefaultConstellation set to {CurrentConstellation}");
                    AdjustToNewConstellation(levelName, CurrentConstellation);
                }
            }
        }

        internal static int GetConstPrice(ClassMapper item)
        {
            int price = item.constelPrice;
            if (!Configuration.AddConstellationPositionData.Value)
                return price;

            if (item.positionalPriceMode == "None")
                return price;

            //Distance from current constellation
            float currentDistance = Vector3.Distance(CurrentConstellationCM.RelativePosition, item.RelativePosition);

            if (item.positionalPriceMode == "SetPriceByDistance")
            {
                price = Configuration.CostPerDistanceUnit.Value * (int)currentDistance;
                return price;
            }


            if (item.originalDistance == 0)
                item.originalDistance = 0.1f;

            //get difference between current distance value and original distance value
            //multiply the price by the current distance value divided by the original distance value
            //closer diff is to 0, the lower the price should be
            //if original currentDistance and current currentDistance are relatively the same, price does not get modified
            //if original currentDistance is higher than current currentDistance, price should go down
            //if original currentDistance is lower than current disance, price should go up
            Plugin.Spam($"Getting Dynamic Price from PositionalData:\n\nPrice ({item.constelPrice}) * currentDistance ({currentDistance}) / originalDistance {item.originalDistance}");

            float newValue = item.constelPrice * (currentDistance / item.originalDistance);

            if (currentDistance > 0)
                price = (int)newValue;
            else
                price = 0;

            price = Mathf.Clamp(price, 0, 99999); //clamped for sanity

            Plugin.Spam($"result - {price}");

            return price;
        }

        internal static void DefaultConstellation()
        {
            string currentLevel;
            if (LevelManager.CurrentExtendedLevel == null)
            {
                currentLevel = ExtendedLevel.GetNumberlessPlanetName(StartOfRound.Instance.currentLevel);
            }
            else
                currentLevel = LevelManager.CurrentExtendedLevel.NumberlessPlanetName;

            if (currentLevel.ToLower() == CompanyMoon.ToLower() && (CurrentConstellation.Length < 1 || !Configuration.ReturnToLastConstellationFromCompany.Value))
            {
                Plugin.Log.LogInfo("Currently located at the Company! Setting to default constellation!");
                if (TryGetDefaultConstellation(out ClassMapper theConst))
                {
                    if (theConst.canRouteCompany)
                    {
                        AdjustToNewConstellation(theConst.defaultMoon, theConst.consName);
                        CurrentConstellationCM = theConst;
                        return;
                    }
                }

                Plugin.WARNING("Unable to load default constellation from config item. Setting to first contellation in list.");
                AdjustToNewConstellation(ConstellationStuff[0].defaultMoon, ConstellationStuff[0].consName);
                CurrentConstellationCM = ConstellationStuff[0];
            }
        }

        internal static bool TryGetDefaultConstellation(out ClassMapper theConst)
        {
            if (ConstellationStuff.Count == 0)
            {
                Plugin.ERROR("Unable to detect ConstellationStuff listing!!");
                theConst = null;
                return false;
            }

            if (ClassMapper.TryGetConstellation(ConstellationStuff, Configuration.CompanyDefaultConstellation.Value, out theConst))
            {
                Plugin.Spam($"Returning {theConst.consName}");
                return true;
            }
            else
            {
                Plugin.Spam($"Config item not matching anything, returning first constellation: {ConstellationStuff[0].consName}");
                theConst = ConstellationStuff[0];
                return true;
            }
        }
    }
}
