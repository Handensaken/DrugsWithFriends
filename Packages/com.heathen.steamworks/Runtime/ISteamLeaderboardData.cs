#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
namespace Heathen.SteamworksIntegration
{
    public interface ISteamLeaderboardData
    {
        public LeaderboardData Data { get; set; }
    }
}
#endif