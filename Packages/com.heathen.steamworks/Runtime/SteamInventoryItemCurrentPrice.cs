#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Updates a <see cref="TMPro.TextMeshProUGUI"/> label with the current price of a <see cref="SteamInventoryItemData"/>.
    /// </summary>
    [ModularComponent(typeof(SteamInventoryItemData), "Current Prices", nameof(label))]
    [RequireComponent(typeof(SteamInventoryItemDataEvents))]
    [RequireComponent(typeof(SteamInventoryItemData))]
    [AddComponentMenu("")]
    public class SteamInventoryItemCurrentPrice : MonoBehaviour
    {
        /// <summary>
        /// The UI label used to display the current price string.
        /// </summary>
        public TMPro.TextMeshProUGUI label;

        private SteamInventoryItemData _mInspector;
        private SteamInventoryItemDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamInventoryItemData>();
            _mEvents = GetComponent<SteamInventoryItemDataEvents>();

            _mEvents.onChange?.AddListener(HandleChange);
        }

        private void HandleChange()
        {
            if (label != null)
                label.text = _mInspector.Data.CurrentPriceString();
        }
    }
}
#endif