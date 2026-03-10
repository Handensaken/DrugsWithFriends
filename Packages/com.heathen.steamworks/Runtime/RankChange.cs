#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    public struct RankChange
    {
        public string LeaderboardName;
        public SteamLeaderboard_t LeaderboardId;
        public LeaderboardEntry OldEntry;
        public LeaderboardEntry NewEntry;
        public int RankDelta
        {
            get
            {
                if (OldEntry != null)
                    return NewEntry.Entry.m_nGlobalRank - OldEntry.Entry.m_nGlobalRank;
                else
                    return NewEntry.Entry.m_nGlobalRank;
            }
        }

        public int ScoreDeta
        {
            get
            {
                if (OldEntry != null)
                    return NewEntry.Entry.m_nScore - OldEntry.Entry.m_nScore;
                else
                    return NewEntry.Entry.m_nScore;
            }
        }
    }
}
#endif