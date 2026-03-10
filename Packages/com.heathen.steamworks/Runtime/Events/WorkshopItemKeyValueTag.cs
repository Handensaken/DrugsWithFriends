#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct WorkshopItemKeyValueTag
    {
        public string key;
        public string value;
    }
}
#endif