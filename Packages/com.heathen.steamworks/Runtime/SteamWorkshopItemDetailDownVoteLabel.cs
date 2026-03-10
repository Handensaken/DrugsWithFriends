#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemDetailData), "Down Votes", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailDataEvents))]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailDownVoteLabel : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamWorkshopItemDetailData _mInspector;
        private SteamWorkshopItemDetailDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamWorkshopItemDetailData>();
            _mEvents = GetComponent<SteamWorkshopItemDetailDataEvents>();

            _mEvents.onChange.AddListener(HandleChanged);
            HandleChanged();
        }

        private void HandleChanged()
        {
            if (label != null)
            {
                if (_mInspector.Data != null)
                    label.text = _mInspector.Data.DownVotes.ToString();
                else
                    label.text = "0";
            }
        }
    }
}
#endif