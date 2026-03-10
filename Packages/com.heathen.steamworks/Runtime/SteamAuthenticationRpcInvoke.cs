#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
#endif
using UnityEngine;
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Automatically invokes the onRpcInvoke event when a valid authentication ticket is received.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAuthenticationEvents))]
    public class SteamAuthenticationRpcInvoke : MonoBehaviour
    {
        private SteamAuthenticationEvents _mEvents;

        private void Awake()
        {
            _mEvents = GetComponent<SteamAuthenticationEvents>();
            _mEvents.onChange.AddListener(HandleTicketChanged);
        }

        private void HandleTicketChanged(AuthenticationTicket arg0)
        {
            if (arg0.Verified && arg0.Result == EResult.k_EResultOK)
                _mEvents.onRpcInvoke?.Invoke(UserData.Me, arg0.Data);
            else
                _mEvents.onError?.Invoke(arg0.Result);
        }
    }
}
#endif