#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// The type applied to a Steam Inventory Item
    /// </summary>
    public enum InventoryItemType
    {
        Item,
        Bundle,
        Generator,
        Playtimegenerator,
        TagGenerator,
        TagTool
    }
}
#endif