#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class HtmlFileOpenDialogEvent : UnityEvent<HTML_FileOpenDialog_t> { };
}
#endif