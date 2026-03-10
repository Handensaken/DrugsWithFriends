#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    public interface ISteamInputControllerData
    {
        public InputHandle_t? Data { get; set; }
    }
}
#endif