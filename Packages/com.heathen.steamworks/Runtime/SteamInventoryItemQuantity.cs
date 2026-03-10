#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Updates <see cref="label"/> with the <see cref="ItemData.GetTotalQuantity()"/> value of the related <see cref="ItemData"/>
    /// </summary>
    [ModularComponent(typeof(SteamInventoryItemData), "Quantities", nameof(label))]
    [RequireComponent(typeof(SteamInventoryItemDataEvents))]
    [RequireComponent(typeof(SteamInventoryItemData))]
    [AddComponentMenu("")]
    public class SteamInventoryItemQuantity : MonoBehaviour
    {
        /// <summary>
        /// The label where the <see cref="ItemData.GetTotalQuantity()"/> will be written.
        /// </summary>
        public TMPro.TextMeshProUGUI label;

        private SteamInventoryItemData _mInspector;
        private SteamInventoryItemDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamInventoryItemData>();
            _mEvents = GetComponent<SteamInventoryItemDataEvents>();

            _mEvents.onStateChanged?.AddListener(HandleStateChange);
        }

        private void HandleStateChange()
        {
            if (label != null)
                label.text = _mInspector.Data.GetTotalQuantity().ToString();
        }
    }
}
#endif