#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemDetailData), "Up Votes", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailDataEvents))]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailUpVoteLabel : MonoBehaviour
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
                label.text = _inspector.Data != null ? _inspector.Data.UpVotes.ToString() : "0";
            }
        }
    }
}
#endif