#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class HtmlJsConfirmEvent : UnityEvent<HTML_JSConfirm_t> { };
}
#endif