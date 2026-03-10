#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Provides events related to Steam Game Server.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamGameServerData))]
    public class SteamGameServerEvents : MonoBehaviour
    {
        /// <summary>
        /// Occurs when the server's data has changed.
        /// </summary>
        [EventField]
        public UnityEvent onChange;
    }
}
#endif