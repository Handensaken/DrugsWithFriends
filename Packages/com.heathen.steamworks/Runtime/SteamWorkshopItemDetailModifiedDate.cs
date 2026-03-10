#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemDetailData), "Date Updated", nameof(settings))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailDataEvents))]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailModifiedDate : MonoBehaviour
    {
        [Serializable]
        public class Settings
        {
            public string format = "yyyy-MMM-dd HH:mm";
            public TMPro.TextMeshProUGUI label;
        }

        public Settings settings;

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
            if (settings.label != null)
            {
                if (_mInspector.Data != null)
                    settings.label.text = _mInspector.Data.TimeUpdated.ToString(settings.format);
                else
                    settings.label.text = string.Empty;
            }
        }
    }
}
#endif