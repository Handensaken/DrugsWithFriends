#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    public struct LeaderboardUgcSet
    {
        public LeaderboardUGCSet_t Data;
        public EResult Result => Data.m_eResult;
        public LeaderboardData Leaderboard => Data.m_hSteamLeaderboard;

        public static implicit operator LeaderboardUgcSet(LeaderboardUGCSet_t native) => new() { Data = native };
        public static implicit operator LeaderboardUGCSet_t(LeaderboardUgcSet heathen) => heathen.Data;
    }
}
#endif