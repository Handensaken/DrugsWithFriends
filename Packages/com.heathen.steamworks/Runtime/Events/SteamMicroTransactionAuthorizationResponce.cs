#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class SteamMicroTransactionAuthorizationResponce : UnityEvent<AppId_t, ulong, bool>
    { }
}
#endif