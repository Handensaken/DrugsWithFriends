#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Updates a <see cref="TMPro.TextMeshProUGUI"/> label with the base price of a <see cref="SteamInventoryItemData"/>.
    /// </summary>
    [ModularComponent(typeof(SteamInventoryItemData), "Base Prices", nameof(label))]
    [RequireComponent(typeof(SteamInventoryItemDataEvents))]
    [RequireComponent(typeof(SteamInventoryItemData))]
    [AddComponentMenu("")]
    public class SteamInventoryItemBasePrice : MonoBehaviour
    {
        /// <summary>
        /// The UI label used to display the base price string.
        /// </summary>
        public TMPro.TextMeshProUGUI label;

        private SteamInventoryItemData _inspector;
        private SteamInventoryItemDataEvents _events;

        private void Awake()
        {
            _inspector = GetComponent<SteamInventoryItemData>();
            _events = GetComponent<SteamInventoryItemDataEvents>();

            _events.onChange?.AddListener(HandleChange);
        }

        private void HandleChange()
        {
            if (label != null)
                label.text = _inspector.Data.BasePriceString();
        }
    }
}
#endif