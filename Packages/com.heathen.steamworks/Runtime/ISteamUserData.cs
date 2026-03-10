#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
namespace Heathen.SteamworksIntegration
{
    public interface ISteamUserData
    {
        public UserData Data { get; set; }
    }
}
#endif