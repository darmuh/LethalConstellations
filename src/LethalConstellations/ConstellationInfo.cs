using System.Collections.Generic;
using BepInEx.Configuration;
using Dawn;
using UnityEngine;

namespace LethalConstellations;

public class ConstellationInfo
{
    public static List<DawnMoonInfo> HiddenMoons = []; //list of moons to not ever display on routing page
    public string Name = string.Empty;

    public List<DawnMoonInfo> MoonGroup = []; //container for dawnlib's moon listing
    public DawnMoonInfo DefaultMoon = null!; //where to route this constellation to by default
    public int ConstellationPrice = -1;
    public ConfigEntry<bool> HideConstellation = null!; //should constellation be shown in menus
    public bool IsConstellationLocked = false; //should constellation be locked for any reason
    public ConfigEntry<bool> OneTimePurchase = null!; //should constellation cost be removed after initial purchase
    //public bool CanRouteCompany = true; // ------ This can probably be removed since there are multiple company moons now and we can allow for moons to be part of multiple constellations

    //keep?
    internal string MenuText = string.Empty;
    internal string InfoText = string.Empty;
    public string OptionalParams = ""; //for use in external mods. Can be used to add information to the main menu
    internal ConfigEntry<string> shortcuts = null!;

    //Distance modifiers
    public Vector3 RelativePosition = Vector3.zero;
    public float OriginalDistance = 0f;

    public ConstellationInfo(string constellationName)
    {
        Name = constellationName;
        Plugin.Constellations.Add(this);
    }

    public void IsActive(bool active)
    {
        foreach(var moon in MoonGroup)
        {
            DawnExtensions.UpdateMoonInfo(moon, !active, HiddenMoons.Contains(moon));
        }
    }
}
