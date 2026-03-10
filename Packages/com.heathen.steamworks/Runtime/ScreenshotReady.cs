#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct ScreenshotReady
    {
        public ScreenshotReady_t Data;

        public readonly ScreenshotHandle Handle => Data.m_hLocal;
        public readonly EResult Result => Data.m_eResult;

        public static implicit operator ScreenshotReady(ScreenshotReady_t native) => new ScreenshotReady { Data = native };
        public static implicit operator ScreenshotReady_t(ScreenshotReady heathen) => heathen.Data;
    }
}
#endif