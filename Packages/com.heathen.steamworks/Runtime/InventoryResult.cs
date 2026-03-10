#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct InventoryResult
    {
        public ItemDetail[] items;
        public EResult result;
        public DateTime Timestamp;
    }
}
#endif