#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
#endif
using UnityEngine;
using UnityEngine.Events;
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Event handler for Steam Authentication events.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAuthenticationData))]
    public class SteamAuthenticationEvents : MonoBehaviour
    {
        /// <summary>
        /// Occurs when the ticket data changes.
        /// </summary>
        [EventField]
        public UnityEvent<AuthenticationTicket> onChange;
        /// <summary>
        /// Occurs when an RPC is invoked.
        /// </summary>
        [EventField]
        public UnityEvent<ulong, byte[]> onRpcInvoke;
        /// <summary>
        /// Occurs when a ticket request error happens.
        /// </summary>
        [EventField]
        public UnityEvent<EResult> onError;
        /// <summary>
        /// Occurs when an invalid ticket is received.
        /// </summary>
        [EventField]
        public UnityEvent<EBeginAuthSessionResult> onInvalidTicket;
        /// <summary>
        /// Occurs when an invalid session is requested.
        /// </summary>
        [EventField]
        public UnityEvent<EAuthSessionResponse> onInvalidSession;
        /// <summary>
        /// Occurs when a session starts.
        /// </summary>
        [EventField]
        public UnityEvent<EAuthSessionResponse> onSessionStart;
    }
}
#endif