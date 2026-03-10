#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class SteamInventoryDefinitionUpdateEvent : UnityEvent { }
}
#endif