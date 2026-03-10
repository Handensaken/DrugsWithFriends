#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class HtmlJsAlertEvent : UnityEvent<HTML_JSAlert_t> { };
}
#endif