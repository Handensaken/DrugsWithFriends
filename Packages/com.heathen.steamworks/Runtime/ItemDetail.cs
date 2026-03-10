#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct ItemDetail
    {
        public SteamItemDetails_t ItemDetails;
        public ItemProperty[] properties;
        public string dynamicProperties;
        public ItemTag[] tags;

        public SteamItemInstanceID_t ItemId => ItemDetails.m_itemId;
        public ItemData Definition => ItemDetails.m_iDefinition;
        public ushort Quantity => ItemDetails.m_unQuantity;
        public ESteamItemFlags Flags => (ESteamItemFlags)ItemDetails.m_unFlags;
    }
}
#endif