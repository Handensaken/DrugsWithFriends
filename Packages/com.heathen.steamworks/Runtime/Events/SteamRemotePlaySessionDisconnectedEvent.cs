#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class SteamRemotePlaySessionDisconnectedEvent : UnityEvent<SteamRemotePlaySessionDisconnected_t> { }
}
#endif