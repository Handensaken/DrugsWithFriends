#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemDetailData), "Titles", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailDataEvents))]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailTitle : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamWorkshopItemDetailData _inspector;
        private SteamWorkshopItemDetailDataEvents _events;

        private void Awake()
        {
            _inspector = GetComponent<SteamWorkshopItemDetailData>();
            _events = GetComponent<SteamWorkshopItemDetailDataEvents>();

            _events.onChange.AddListener(HandleChanged);
            HandleChanged();
        }

        private void HandleChanged()
        {
            if (label)
            {
                label.text = _inspector.Data != null ? _inspector.Data.Title : string.Empty;
            }
        }
    }
}
#endif