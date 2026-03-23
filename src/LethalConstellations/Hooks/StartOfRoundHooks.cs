using System.Linq;
using Dawn;
using HarmonyLib;

namespace LethalConstellations.Hooks;

internal class StartOfRoundHooks
{
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ChangeLevel))]
    internal class LevelChangePatch
    {
        internal static void Postfix(StartOfRound __instance, int levelID)
        {
            DawnMoonInfo moon = __instance.levels[levelID].GetDawnInfo();

            if (moon == null)
                return;

            var constellation = Plugin.Constellations.FirstOrDefault(x => x.DefaultMoon == moon);
            if (constellation == null)
                return;

            foreach(ConstellationInfo con in Plugin.Constellations)
            {
                if (con == constellation)
                    con.IsActive(true);
                else
                    con.IsActive(false);
            }
        }
    }
}
