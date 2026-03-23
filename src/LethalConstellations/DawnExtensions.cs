using BepInEx.Configuration;
using Dawn;

namespace LethalConstellations;

internal class DawnExtensions
{
    private static TerminalNode _routeLocked = null!;
    internal static TerminalNode RouteLocked
    {
        get
        {
            if (_routeLocked == null)
            {
                _routeLocked = TerminalNode.CreateInstance<TerminalNode>();
                _routeLocked.displayText = "This route is locked!\n\n\n";
            }

            return _routeLocked;
        }
    }

    internal static void UpdateMoonInfo(DawnMoonInfo moon, bool locked, bool hidden)
    {
        if (hidden)
        {
            if (locked)
                moon.DawnPurchaseInfo.PurchasePredicate = ITerminalPurchasePredicate.AlwaysFail(RouteLocked);
            else
                moon.DawnPurchaseInfo.PurchasePredicate = ITerminalPurchasePredicate.AlwaysHide();
        }
        else
            moon.DawnPurchaseInfo.PurchasePredicate = ITerminalPurchasePredicate.AlwaysSuccess();
    }

    internal static void UpdateMoonPrice(DawnMoonInfo moon, ConfigEntry<int> price)
    {
        moon.DawnPurchaseInfo.Cost = new SimpleProvider<int>(price.Value);
    }
}
