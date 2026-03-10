#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    public struct SteamServerConnectFailure
    {
        public SteamServerConnectFailure_t Data;
        public EResult Result => Data.m_eResult;
        public bool Retrying => Data.m_bStillRetrying;

        public static implicit operator SteamServerConnectFailure(SteamServerConnectFailure_t native) => new SteamServerConnectFailure { Data = native };
        public static implicit operator SteamServerConnectFailure_t(SteamServerConnectFailure heathen) => heathen.Data;
    }
    
    
}
#endif