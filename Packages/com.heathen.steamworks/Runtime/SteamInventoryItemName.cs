#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Updates a <see cref="TMPro.TextMeshProUGUI"/> label with the name of a <see cref="SteamInventoryItemData"/>.
    /// </summary>
    [ModularComponent(typeof(SteamInventoryItemData), "Names", nameof(label))]
    [RequireComponent(typeof(SteamInventoryItemDataEvents))]
    [RequireComponent(typeof(SteamInventoryItemData))]
    [AddComponentMenu("")]
    public class SteamInventoryItemName : MonoBehaviour
    {
        /// <summary>
        /// The UI label used to display the name of the item.
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
                label.text = _mInspector.Data.Name;
        }
    }
}
#endif