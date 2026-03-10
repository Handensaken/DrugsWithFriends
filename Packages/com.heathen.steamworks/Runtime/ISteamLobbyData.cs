#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
namespace Heathen.SteamworksIntegration
{
    public interface ISteamLobbyData
    {
        public LobbyData Data { get; set; }
    }
}
#endif