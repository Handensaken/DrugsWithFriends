#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
#endif

namespace Heathen.SteamworksIntegration
{
    public interface ISteamInputActionData
    {
        public InputActionSetData Set { get; set; }
        public InputActionSetLayerData Layer { get; set; }
        public InputActionData Action { get; set; }
    }
}
#endif