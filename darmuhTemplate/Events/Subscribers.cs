using OpenLib.Events;


namespace Template.Events
{
    public class Subscribers
    {
        public static void Subscribe()
        {
            EventManager.TerminalAwake.AddListener(OnTerminalAwake);
            EventManager.TerminalStart.AddListener(OnTerminalStart);
            EventManager.TerminalDisable.AddListener(OnTerminalDisable);
        }

        public static void OnTerminalAwake(Terminal instance)
        {
            Plugin.instance.Terminal = instance;
            Plugin.MoreLogs($"Setting Plugin.instance.Terminal");

        }

        public static void OnTerminalDisable()
        {

        }

        public static void OnTerminalStart()
        {

        }

    }
}
