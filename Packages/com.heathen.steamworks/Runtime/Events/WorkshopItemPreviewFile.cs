#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct WorkshopItemPreviewFile
    {
        public string source;
        /// <summary>
        /// YouTubeVideo and Sketchfab are not currently supported
        /// </summary>
        public EItemPreviewType type;
    }
}
#endif