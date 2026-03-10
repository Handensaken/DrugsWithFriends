#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct ItemTag
    {
        public string category;
        public string tag;

        public override string ToString()
        {
            return category + ":" + tag;
        }
    }
}
#endif