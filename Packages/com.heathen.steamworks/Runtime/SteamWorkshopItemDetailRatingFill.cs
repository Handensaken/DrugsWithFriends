#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;
using UnityEngine.UI;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemDetailData), "Ratings Fill", nameof(image))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailDataEvents))]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailRatingFill : MonoBehaviour
    {
        public Image image;

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
            if (image != null)
            {
                if (_mInspector.Data != null)
                    image.fillAmount = _mInspector.Data.VoteScore;
                else
                    image.fillAmount = 0;
            }
        }
    }
}
#endif