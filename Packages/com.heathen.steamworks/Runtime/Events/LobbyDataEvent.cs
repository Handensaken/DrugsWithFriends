#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class LobbyDataEvent : UnityEvent<LobbyData> { }

    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }
}
#endif