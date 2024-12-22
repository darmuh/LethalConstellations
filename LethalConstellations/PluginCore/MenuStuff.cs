using LethalConstellations.ConfigManager;
using LethalConstellations.EventStuff;
using LethalLevelLoader;
using OpenLib.Common;
using OpenLib.Compat;
using OpenLib.CoreMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using static LethalConstellations.PluginCore.Collections;
using static OpenLib.ConfigManager.ConfigSetup;

namespace LethalConstellations.PluginCore
{
    internal class MenuStuff
    {
        internal static InteractiveMenu ConstellationsMenu = new("Constellations_Menu", LoadPage, MenuSelect, ExitToTerminal);
        internal static List<Key> KeysBound = [];
        internal static bool ConfirmationCheck = false;

        internal static Color transparent = new(0, 0, 0, 0);

        internal static void PreInit()
        {
            //update constellation category names here
            if (FixBadConfig())
                Plugin.Spam($"ConstellationWord custom words updated to {ConstellationsWord}");

            Init();
        }

        internal static void LoadSpecial(string message)
        {
            Plugin.instance.Terminal.StartCoroutine(DelayUpdateText(message));
        }

        internal static void LoadPage()
        {
            Plugin.instance.Terminal.StartCoroutine(DelayUpdateText());
        }

        internal static IEnumerator DelayUpdateText(string newText = "")
        {
            yield return new WaitForEndOfFrame();
            if (newText.Length < 1)
                ConstellationsNode.displayText = MainMenuText(ConstellationsMenu.activeSelection, ref ConstellationsMenu.currentPage);
            else
                ConstellationsNode.displayText = newText;

            yield return new WaitForEndOfFrame();
            if (OpenLib.Plugin.instance.TerminalStuff)
                TerminalStuffMod.LoadAndSync(ConstellationsNode);
            else
                Plugin.instance.Terminal.LoadNewNode(ConstellationsNode);

            yield return new WaitForEndOfFrame();

        }

        internal static void AnyKeyEvent()
        {
            if (ConfirmationCheck)
            {
                Plugin.Spam($"Confirmation check for {DisplayConstellations[ConstellationsMenu.activeSelection].consName}!");

                if (Keyboard.current[Key.Enter].isPressed)
                {
                    int getPrice = LevelStuff.GetConstPrice(DisplayConstellations[ConstellationsMenu.activeSelection]);
                    TravelToNewConstellation(getPrice, ConstellationsMenu.activeSelection);
                    ConfirmationCheck = false;
                }

                if (Keyboard.current[Key.Backspace].isPressed)
                {
                    LoadSpecial($"Route to {ConstellationWord} {DisplayConstellations[ConstellationsMenu.activeSelection].consName.ToUpper()} has been canceled.\r\n\r\n\tPress any key to continue...\r\n");
                    ConfirmationCheck = false;
                }

                return;
            }

            Plugin.Spam("Accepting any key to continue!");
            ConstellationsMenu.acceptAnything = false;
            LoadPage();
        }

        internal static void MenuSelect()
        {
            int getPrice = LevelStuff.GetConstPrice(DisplayConstellations[ConstellationsMenu.activeSelection]);
            Plugin.Spam($"Price of {DisplayConstellations[ConstellationsMenu.activeSelection].consName} is {getPrice}!");
            if (CannotRoute(getPrice, ConstellationsMenu.activeSelection, out string message))
            {
                ConstellationsMenu.acceptAnything = true;
                LoadSpecial(message);
                return;
            }

            if (Configuration.RequireConfirmation.Value)
            {
                ConstellationsMenu.acceptAnything = true;
                ConfirmationCheck = true;
                LoadSpecial($"Travel to {ConstellationWord} - {DisplayConstellations[ConstellationsMenu.activeSelection].consName.ToUpper()}?\n\n\tPress [ ENTER ] to CONFIRM\n\tPress [ BACKSPACE ] to DENY\n");
                return;
            }

            TravelToNewConstellation(getPrice, ConstellationsMenu.activeSelection);
        }

        internal static string RouteShortcut()
        {
            DisplayConstellations = ConstellationStuff;

            if (TryGetConstellationFromScreenText(out ClassMapper newcon, out string message))
            {
                int indexNum = DisplayConstellations.IndexOf(newcon);
                int getPrice = LevelStuff.GetConstPrice(DisplayConstellations[indexNum]);
                if (CannotRoute(getPrice, DisplayConstellations.IndexOf(newcon), out message))
                {
                    ConfirmationCheck = true;
                    Plugin.instance.Terminal.StartCoroutine(MenuStart(true));
                    return message;
                }


                if (Configuration.RequireConfirmation.Value)
                {
                    ConfirmationCheck = true;
                    Plugin.instance.Terminal.StartCoroutine(MenuStart(true));
                    ConstellationsMenu.activeSelection = indexNum;
                    return $"Travel to {ConstellationWord} - {DisplayConstellations[indexNum].consName.ToUpper()}?\n\n\tPress [ ENTER ] to CONFIRM\n\tPress [ BACKSPACE ] to DENY\n";
                }

                TravelToNewConstellation(getPrice, indexNum);
                return "";

            }

            return message;
        }

        internal static void TravelToNewConstellation(int getPrice, int indexNum)
        {
            CurrentConstellation = DisplayConstellations[indexNum].consName;
            CurrentConstellationCM = DisplayConstellations[indexNum];
            Plugin.Spam($"oldcreds: {Plugin.instance.Terminal.groupCredits}");
            int newCreds = Plugin.instance.Terminal.groupCredits - getPrice;
            Plugin.Spam($"newCreds amount = {Plugin.instance.Terminal.groupCredits}");
            StartOfRound.Instance.ChangeLevelServerRpc(CurrentConstellationCM.defaultMoonLevel.SelectableLevel.levelID, newCreds);

            if (getPrice > 0)
                Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);

            NewEvents.RouteConstellationSuccess.Invoke(); //for other mods to subscribe to successful route

            DisplayConstellations[indexNum].OneTimePurchase();
            //OneTimePurchaseCheck(constellationName);
            ConstellationsMenu.acceptAnything = true;
            LoadSpecial($"Travelling to {ConstellationWord} - {CurrentConstellation.ToUpper()}\nYour new credits balance: ${newCreds}\r\n\r\nPress any key to continue...\r\n");
        }

        internal static void ExitToTerminal()
        {
            ExitMenu(true);
        }

        internal static void ExitMenu(bool enableInput)
        {
            Plugin.instance.Terminal.StartCoroutine(MenuClose(enableInput));
        }

        internal static IEnumerator MenuClose(bool enableInput)
        {
            yield return new WaitForEndOfFrame();
            ConstellationsMenu.inMenu = false;
            ConfirmationCheck = false;
            yield return new WaitForEndOfFrame();

            if (OpenLib.Plugin.instance.TerminalStuff)
                TerminalStuffMod.LoadAndSync(Plugin.instance.Terminal.terminalNodes.specialNodes[13]);
            else
                Plugin.instance.Terminal.LoadNewNode(Plugin.instance.Terminal.terminalNodes.specialNodes[13]);

            yield return new WaitForEndOfFrame();
            CommonTerminal.ChangeCaretColor(CommonTerminal.CaretOriginal, false);

            if (enableInput)
            {
                Plugin.instance.Terminal.screenText.ActivateInputField();
                Plugin.instance.Terminal.screenText.interactable = true;
            }

            yield break;
        }

        internal static IEnumerator MenuStart(bool acceptAnything)
        {
            if (ConstellationsMenu.inMenu)
                yield break;

            yield return new WaitForEndOfFrame();
            CommonTerminal.ChangeCaretColor(transparent, true);
            ConstellationsMenu.inMenu = true;
            ConstellationsMenu.acceptAnything = acceptAnything;
            yield return new WaitForEndOfFrame();
            Plugin.instance.Terminal.screenText.DeactivateInputField();
            Plugin.instance.Terminal.screenText.interactable = false;
            yield break;
        }

        internal static void Init()
        {
            Plugin.Spam("ConstellationsKeywords()");
            ConstellationsKeywords();
            MoonStuff.ModifyMoonPrices();
            InteractiveMenuStuff();
        }

        internal static void InteractiveMenuStuff()
        {
            ConstellationsMenu.isMenuEnabled = true;
            ConstellationsMenu.activeSelection = 0;
            ConstellationsMenu.currentPage = 1;
            ConstellationsMenu.AcceptAnyKeyEvent.AddListener(AnyKeyEvent);
            KeysBound.Clear();
            TrySetKey(ref ConstellationsMenu.upMenu, Configuration.UpMenuKey.Value);
            TrySetKey(ref ConstellationsMenu.downMenu, Configuration.DownMenuKey.Value);
            TrySetKey(ref ConstellationsMenu.rightMenu, Configuration.RightMenuKey.Value);
            TrySetKey(ref ConstellationsMenu.leftMenu, Configuration.LeftMenuKey.Value);
            TrySetKey(ref ConstellationsMenu.selectMenu, Configuration.SelectMenuKey.Value);
            TrySetKey(ref ConstellationsMenu.leaveMenu, Configuration.ExitMenuKey.Value);

        }

        //will probably move this to openlib at some point
        internal static void TrySetKey(ref Key original, string config)
        {
            if (CommonStringStuff.TryGetKey(config, out Key newKey))
            {
                if (KeysBound.Contains(newKey))
                {
                    Plugin.WARNING($"Unable to set to key {newKey}, it is already in use!");
                    return;
                }

                if (original != newKey)
                {
                    Plugin.Log.LogInfo($"Changing default binding of {original} to {newKey}");
                    KeysBound.Add(newKey);
                    Key find = original;
                    Action action = ConstellationsMenu.MainActions.FirstOrDefault(x => x.Key == find).Value;
                    if (action == null)
                    {
                        Plugin.WARNING($"UNABLE TO REPLACE KEY {original}! Action undefined!");
                        return;
                    }
                    ConstellationsMenu.MainActions.Remove(original);
                    ConstellationsMenu.MainActions.Add(newKey, action);
                    original = newKey;
                    return;
                }

                Plugin.Log.LogInfo($"Default binding of {original} is already set.");
                KeysBound.Add(original);
            }
        }

        internal static bool FixBadConfig()
        {
            if (DynamicBools.TryGetKeyword(ConstellationsWord))
            {
                ConstellationsWord = GetCleanKeyWord();
                ConstellationWord = ConstellationsWord;
                return true;
            }
            return false;
        }

        internal static string GetCleanKeyWord()
        {
            for (int i = 0; i < StartOfRound.Instance.randomNames.Length - 1; i++)
            {
                int randomIndex = Rand.Next(0, StartOfRound.Instance.randomNames.Length);
                string newName = StartOfRound.Instance.randomNames[randomIndex];

                if (!DynamicBools.TryGetKeyword(newName))
                {
                    return newName;
                }
            }
            return "invalidWord";
        }

        internal static void ConstellationsKeywords()
        {
            Plugin.Spam($"{ConstellationCats.Count}");

            ConstellationsNode = AddingThings.AddNodeManual("Constellations_Menu", ConstellationsWord, EnterMenu, true, 0, defaultListing);
            TerminalNode consInfo = BasicTerminal.CreateNewTerminalNode();
            consInfo.displayText = $"{Configuration.ConstellationsInfoText.Value}\r\n";
            consInfo.clearPreviousText = true;

            if (DynamicBools.TryGetKeyword("info", out TerminalKeyword infokeyword))
            {
                AddingThings.AddCompatibleNoun(ref infokeyword, ConstellationsWord.ToLower(), consInfo);
                Plugin.Spam("Adding info stuff");
            }

            if (Configuration.ConstellationsShortcuts.Value.Length > 0)
            {

                List<string> shortcuts = CommonStringStuff.GetKeywordsPerConfigItem(Configuration.ConstellationsShortcuts.Value, ',');
                foreach (string shortcut in shortcuts)
                {
                    AddingThings.AddKeywordToExistingNode(shortcut, ConstellationsNode);
                    Plugin.Spam($"{shortcut} added to ConstellationsNode");

                    if (infokeyword != null)
                    {
                        AddingThings.AddCompatibleNoun(ref infokeyword, shortcut.ToLower(), consInfo);
                        Plugin.Spam($"{shortcut} info command added!");
                    }

                }
            }

            foreach (ClassMapper cons in ConstellationStuff)
                RouteShortcuts(cons, infokeyword);

            AddHintsToNodes();

        }


        internal static void RouteShortcuts(ClassMapper cons, TerminalKeyword infokeyword)
        {
            Plugin.Spam($"Setting shortcuts for:");
            Plugin.Spam(cons.consName);

            if (cons.shortcutList.Count == 0)
                return;
            TerminalNode consInfo = null!;

            bool addInfo = cons.infoText.Length > 0;

            if (addInfo)
            {
                consInfo = BasicTerminal.CreateNewTerminalNode();
                consInfo.displayText = $"{Configuration.ConstellationsInfoText.Value}\r\n";
                consInfo.clearPreviousText = true;
            }

            RouteShortcutNode = AddingThings.AddNodeManual("Constellations_Menu", $"{cons.consName} shortcutword", RouteShortcut, true, 0, defaultListing);
            foreach (string shortcut in cons.shortcutList)
            {
                AddingThings.AddKeywordToExistingNode(shortcut, RouteShortcutNode);

                if (!addInfo)
                    continue;

                if (infokeyword != null)
                {
                    AddingThings.AddCompatibleNoun(ref infokeyword, shortcut.ToLower(), consInfo);
                    Plugin.Spam($"{shortcut} info command added!");
                }
            }
        }

        internal static void AddHintsToNodes()
        {
            string hintText = Configuration.ConstellationsHintText.Value.Replace("[keyword]", $"{ConstellationsWord.ToUpper()}");
            if (LogicHandling.TryGetFromAllNodes("OtherCommands", out TerminalNode otherNode))
            {
                if (Configuration.AddHintTo.Value == "both" || Configuration.AddHintTo.Value == "help")
                    AddingThings.AddToHelpCommand($"{hintText}");

                if (Configuration.AddHintTo.Value == "both" || Configuration.AddHintTo.Value == "other")
                    AddingThings.AddToExistingNodeText($"\n{hintText}", ref otherNode);
            }

        }

        internal static string EnterMenu()
        {
            ConstellationsMenu.currentPage = 1;
            Plugin.instance.Terminal.StartCoroutine(MenuStart(false));
            return MainMenuText(0, ref ConstellationsMenu.currentPage);
        }

        internal static string MainMenuText(int activeIndex, ref int currentPage)
        {
            if (ConstellationStuff.Count == 0)
                return $"Unable to route to any {ConstellationsWord} at this time!\r\n\r\n\t{ConstellationWord} count is 0!!!\r\n\r\n";

            DisplayConstellations = ConstellationStuff;

            if (Configuration.HideUnaffordableConstellations.Value)
                DisplayConstellations = DisplayConstellations.FindAll(x => LevelStuff.GetConstPrice(x) <= Plugin.instance.Terminal.groupCredits);

            if (DisplayConstellations.Count == 0)
                return $"Unable to route to any {ConstellationsWord}! All constellations cannot be afforded!";

            // Ensure currentPage is within valid range
            currentPage = Mathf.Clamp(currentPage, 1, Mathf.CeilToInt((float)DisplayConstellations.Count / Configuration.ConstellationsPerPage.Value));

            // Calculate the start and end indexes for the current page
            int startIndex = (currentPage - 1) * Configuration.ConstellationsPerPage.Value;
            int endIndex;
            if (DisplayConstellations.Count > Configuration.ConstellationsPerPage.Value)
                endIndex = Mathf.Min(startIndex + Configuration.ConstellationsPerPage.Value, DisplayConstellations.Count);
            else
                endIndex = DisplayConstellations.Count;

            // Recalculate activeIndex based on the current page
            // Ensure activeIndex is within the range of items on the current page
            activeIndex = Mathf.Clamp(activeIndex, startIndex, endIndex - 1);

            ConstellationsMenu.activeSelection = activeIndex;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"========== {ConstellationsWord} Routing Matrix ==========\n");

            for (int i = startIndex; i < endIndex; i++)
            {
                ClassMapper item = DisplayConstellations[i];
                string menuText = item.menuText;
                string defaultMoon = item.defaultMoon;
                string defaultWeather = "";
                if (item.defaultMoonLevel == null)
                {
                    Plugin.WARNING($"{item.consName} defaultMoonLevel is NULL. This will affect performance of the mod");
                }
                else
                    defaultWeather = TerminalManager.GetWeatherConditions(item.defaultMoonLevel);

                int getPrice = LevelStuff.GetConstPrice(DisplayConstellations[i]);
                int getDistance = item.GetDistance();
                string distanceDisplay = (getDistance != -1)
                ? getDistance.ToString() : "[Unknown]";

                menuText = menuText.Replace("[~t]", "\t").Replace("[~n]", "\n").Replace("[name]", item.consName).Replace("[price]", $"{getPrice}").Replace("[defaultmoon]", $"{defaultMoon}").Replace("[currentweather]", defaultWeather).Replace("[currentdistance]", $"Distance: {distanceDisplay}").Replace("[optionals]", item.optionalParams);

                if (menuText.Length < 1)
                    menuText = item.consName.ToUpperInvariant();

                if (i == activeIndex)
                    stringBuilder.Append("> " + menuText + "\n\n");
                else
                    stringBuilder.Append("" + menuText + "\n\n");
            }


            stringBuilder.Append($"Current {ConstellationWord}: {CurrentConstellation}\r\n\r\n");
            stringBuilder.Append($"Page [{ConstellationsMenu.leftMenu}] < {currentPage}/{Mathf.CeilToInt((float)DisplayConstellations.Count / Configuration.ConstellationsPerPage.Value)} > [{ConstellationsMenu.rightMenu}]\r\n");

            if (Configuration.ConstellationsPerPage.Value > 1)
                stringBuilder.Append($"Next [{ConstellationsMenu.downMenu}] / Previous [{ConstellationsMenu.upMenu}]\r\n");
            stringBuilder.Append($"Select [{ConstellationsMenu.selectMenu}] / Exit [{ConstellationsMenu.leaveMenu}]");


            return stringBuilder.ToString();
        }

        internal static bool CannotRoute(int getPrice, int selection, out string message)
        {
            if (DisplayConstellations.Count < 1)
            {
                message = "Configuration failure detected!\r\n\r\nPress any key to continue...\r\n\r\n";
                return true;
            }

            if (StartOfRound.Instance.travellingToNewLevel)
            {
                message = "Ship is currently in motion, unable to change routing at this time!\r\n\r\nPress any key to continue...\r\n\r\n";
                return true;
            }

            if (!StartOfRound.Instance.inShipPhase)
            {
                message = $"Ship needs to be in orbit in order to travel to new {ConstellationWord}!\r\n\r\nPress any key to continue...\r\n\r\n";
                return true;
            }

            if (Plugin.instance.Terminal.groupCredits < getPrice)
            {
                message = $"Unable to afford to travel to {ConstellationWord} - {DisplayConstellations[selection].consName.ToUpper()}\r\n\r\nPress any key to continue...\r\n\r\n";
                return true;
            }

            if (DisplayConstellations[selection].isLocked)
            {
                message = $"Unable to travel to {DisplayConstellations[selection].consName}. {ConstellationWord} is locked!\r\n\r\nPress any key to continue...\r\n\r\n";
                return true;
            }

            message = "";
            return false;
        }

    }
}
