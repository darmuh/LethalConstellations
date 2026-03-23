using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using Dawn;
using static LethalConstellations.Plugin;

namespace LethalConstellations;

public class ConstellationsConfig
{
    public static ConfigEntry<Loggers.LoggingLevel> LogLevel { get; set; } = null!;
    public static ConfigEntry<string> UserProvidedConstellations {  get; set; } = null!;
    public static ConfigFile GeneratedConfig = new(Path.Combine(Paths.ConfigPath, $"{Name}_Generated.cfg"), true);

    public static void Init(ConfigFile config)
    {
        LogLevel = MakeGeneric(config, "Debug", "Logging Level", Loggers.LoggingLevel.Info, "Set what logging messages from this mod should attempt to write to the log file.\nNOTE: This does not overwrite the global bepinex logging configuration items.");
        UserProvidedConstellations = MakeGeneric(config, "Setup", "Constellation Names", "", "Enter your custom constellation names here. Each entry is separated by a comma.\nA blank value here will result in LethalConstellations attempting to make it's own constellations");
        
    }

    internal static void MapMoon(KeyValuePair<NamespacedKey<DawnMoonInfo>, DawnMoonInfo> moon)
    {
        string numberless = moon.Value.GetNumberlessPlanetName();
        string safeName = BepinFriendlyString(numberless);

        foreach(var constellation in Constellations)
        {
            string safeCon = BepinFriendlyString(constellation.Name);
            ConfigEntry<bool> inConstellation = MakeGeneric(GeneratedConfig, $"Moons - {safeName}", $"{safeName} - {safeCon}", true, $"Is {safeName} located within {safeCon}");
            if (inConstellation.Value)
                constellation.MoonGroup.Add(moon.Value);    
        }

        ConfigEntry<int> priceOverride = MakeGeneric(GeneratedConfig, $"Moons - {safeName}", $"{safeName} Price Override", moon.Value.DawnPurchaseInfo.Cost.Provide(), "Set this to a new value (from the default) to override this moon's price");
        ConfigEntry<bool> hideMoon = MakeGeneric(GeneratedConfig, $"Moons - {safeName}", $"{safeName} Remains Hidden", moon.Value.DawnPurchaseInfo.PurchasePredicate.CanPurchase() is TerminalPurchaseResult.HiddenPurchaseResult, "Set this to determine if a moon should remain hidden even while inside it can be routed to");
    
    }

    internal static void MapConstellation(string name)
    {
        string safeName = BepinFriendlyString(name);
        ConfigEntry<string> menuText = MakeGeneric(GeneratedConfig, $"{safeName}", $"{safeName} menuText", $"Route to System $[price] [name][~n]Default Moon:[defaultmoon] [currentweather][~n][currentdistance] [optionals]", $"The text displayed for {safeName}'s menu item\n[price] will display price information\n[name] will display the constellation name\n[~n] will create a new line\n[~t] will create a tab indent\n[defaultmoon] will display a constellation's default moon\n[currentweather] will display a moons current weather (retrieved from LLL)\n[currentdistance] will display the current distance value determined by positional data\n[optionals] will allow for other mods to add their own flavor text to this menu item.");

        ConfigEntry<string> shortCuts = MakeGeneric(GeneratedConfig, $"{safeName}", $"{safeName} shortcuts", "", $"Specify a list of shortcuts to use for routing to {safeName}.\nEach shortcut keyword is separated by a ','");

        ConfigEntry<bool> isHiding = MakeGeneric(GeneratedConfig, $"{safeName}", $"{safeName} isHidden", false, $"Enable this to hide {safeName} from the listing");

        ConfigEntry<bool> buyOnce = MakeGeneric(GeneratedConfig, $"{safeName}", $"{safeName} One-Time Purchase", false, $"Enable this to remove the cost requirement for routing to {safeName} after paying for it once");

        ConstellationInfo constellation = new(safeName)
        {
            MenuText = menuText.Value,
            HideConstellation = isHiding,
            OneTimePurchase = buyOnce,
            shortcuts = shortCuts
        };

        /*
        if (Configuration.ConstellationSpecificInfoNodes.Value)
        {
            ConfigEntry<string> infoText = MakeString(Configuration.GeneratedConfig, $"{ConstellationWord} {fixedName}", $"{fixedName} infoText", $"{ConstellationWord} - {fixedName}\n\n\nThis [ConstellationWord] contains moons in it. Route to it and find out which!\r\n\r\n", $"The text that displays with the info command for this {ConstellationWord}'s shortcut keywords");
            if (infoText.Value.Contains("[ConstellationWord]"))
                infoText.Value = infoText.Value.Replace("[ConstellationWord]", ConstellationWord);
            constClass.infoText = infoText.Value;
        }*/
    }

    //Openlib rips
    public static ConfigEntry<T> MakeGeneric<T>(ConfigFile ModConfig, string section, string configItemName, T defaultValue, string ConfigDescription)
    {
        section = BepinFriendlyString(section);
        configItemName = BepinFriendlyString(configItemName);

        return ModConfig.Bind<T>(section, configItemName, defaultValue, ConfigDescription);
    }

    public static ConfigEntry<T> MakeGeneric<T>(ConfigFile ModConfig, string section, string configItemName, T defaultValue, string description, AcceptableValueList<T> acceptableValues = null!) where T : IEquatable<T>
    {
        section = BepinFriendlyString(section);
        configItemName = BepinFriendlyString(configItemName);

        return ModConfig.Bind<T>(section, configItemName, defaultValue, new ConfigDescription(description, acceptableValues));
    }

    public static ConfigEntry<T> MakeGeneric<T>(ConfigFile ModConfig, string section, string configItemName, T defaultValue, string description, T minValue, T maxValue) where T : IComparable
    {
        section = BepinFriendlyString(section);
        configItemName = BepinFriendlyString(configItemName);
        AcceptableValueRange<T> acceptableRange = new(minValue, maxValue);

        return ModConfig.Bind<T>(section, configItemName, defaultValue, new ConfigDescription(description, acceptableRange));
    }

    
}
