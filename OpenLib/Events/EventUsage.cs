﻿using OpenLib.ConfigManager;
using OpenLib.CoreMethods;
using OpenLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenLib.Menus;

namespace OpenLib.Events
{
    public class EventUsage
    {
        public static void Subscribers()
        {
            EventManager.TerminalAwake.AddListener(OnTerminalAwake);
            EventManager.TerminalStart.AddListener(OnTerminalStart);
            EventManager.TerminalDisable.AddListener(OnTerminalDisable);
            EventManager.TerminalLoadNewNode.AddListener(OnLoadNewNode);
            EventManager.TerminalParseSent.AddListener(OnParseSent);
            EventManager.TerminalBeginUsing.AddListener(OnUsingTerminal);
            EventManager.GameNetworkManagerStart.AddListener(StartGame.OnGameStart);
            EventManager.TeleporterAwake.AddListener(Teleporter.CheckTeleporterTypeAndAssign);
        }

        public static void OnTerminalAwake(Terminal instance)
        {
            Plugin.instance.Terminal = instance;
            Plugin.MoreLogs($"Setting Plugin.instance.Terminal");
            CommandRegistry.GetCommandsToAdd(ConfigSetup.defaultManagedBools, ConfigSetup.defaultListing);

        }

        public static void OnTerminalDisable()
        {
            RemoveThings.OnTerminalDisable();
            TerminalStart.delayStartEnum = false;
            ListManagement.ClearLists();
        }

        public static void OnTerminalStart()
        {
            TerminalStart.TerminalStartGroupDelay();
        }

        public static void OnUsingTerminal()
        {
            Plugin.MoreLogs("Start Using Terminal Postfix");
        }

        public static TerminalNode OnParseSent(ref TerminalNode node)
        {
            Plugin.Spam("parse event");
            if (node == null)
            {
                Plugin.ERROR("node detected as NULL");
                return node;
            }

            string cleanedText = CommonStringStuff.GetCleanedScreenText(Plugin.instance.Terminal);
            string[] words = cleanedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (ConfigSetup.defaultListing.specialListString.ContainsKey(words[0]))
            {
                TerminalNode retrieveNode = CommonTerminal.GetNodeFromList(words[0], ConfigSetup.defaultListing.specialListString);
                node = retrieveNode;
            } //listnumtostring should be patched in by terminalstuff, outside the scope of this mod other than holding the lists

            if (LogicHandling.GetNewDisplayText(ConfigSetup.defaultListing, ref node))
            {
                Plugin.MoreLogs($"node found: {node.name}");
            }

            return node;

        }

        public static void OnLoadNewNode(TerminalNode node)
        {
            Plugin.Spam($"listing count: {ConfigSetup.defaultListing.count}");

            if (node != null)
                Plugin.Spam($"{node.name} has been loaded");
        }

    }
}