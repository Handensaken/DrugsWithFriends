#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    public struct SteamServersDisconnected
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public SteamServersDisconnected_t Data;
        public EResult Result => Data.m_eResult;

        public static implicit operator SteamServersDisconnected(SteamServersDisconnected_t native) => new SteamServersDisconnected { Data = native };
        public static implicit operator SteamServersDisconnected_t(SteamServersDisconnected heathen) => heathen.Data;
    }
}
#endif