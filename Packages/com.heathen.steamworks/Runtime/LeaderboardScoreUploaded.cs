#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct LeaderboardScoreUploaded
    {
        public LeaderboardScoreUploaded_t Data;
        public readonly bool Success => Data.m_bSuccess != 0;
        public readonly bool ScoreChanged => Data.m_bScoreChanged != 0;
        public readonly LeaderboardData Leaderboard => Data.m_hSteamLeaderboard;
        public readonly int Score => Data.m_nScore;
        public readonly int GlobalRankNew => Data.m_nGlobalRankNew;
        public readonly int GlobalRankPrevious => Data.m_nGlobalRankPrevious;

        public static implicit operator LeaderboardScoreUploaded(LeaderboardScoreUploaded_t native) => new LeaderboardScoreUploaded { Data = native };
        public static implicit operator LeaderboardScoreUploaded_t(LeaderboardScoreUploaded heathen) => heathen.Data;
    }
}
#endif