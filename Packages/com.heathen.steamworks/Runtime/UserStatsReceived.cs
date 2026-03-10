#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct UserStatsReceived
    {
        public UserStatsReceived_t Data;
        public GameData Id => Data.m_nGameID;
        public EResult Result => Data.m_eResult;
        public UserData User => Data.m_steamIDUser;

        public static implicit operator UserStatsReceived(UserStatsReceived_t native) => new UserStatsReceived { Data = native };
        public static implicit operator UserStatsReceived_t(UserStatsReceived heathen) => heathen.Data;
    }
}
#endif