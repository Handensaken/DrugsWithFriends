#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Proxies events from a <see cref="SteamInventoryItemData"/> component.
    /// </summary>
    [ModularEvents(typeof(SteamInventoryItemData))]
    [RequireComponent(typeof(SteamInventoryItemData))]
    [AddComponentMenu("")]
    public class SteamInventoryItemDataEvents : MonoBehaviour
    {
        /// <summary>
        /// Occurs when the item data has changed.
        /// </summary>
        [EventField]
        public UnityEvent onChange = new();
        /// <summary>
        /// Occurs when the item state has changed, such as after an inventory operation.
        /// </summary>
        [EventField]
        public UnityEvent onStateChanged = new();
        /// <summary>
        /// Occurs when a consume request has successfully completed.
        /// </summary>
        [EventField]
        public UnityEvent<ItemDetail[]> onConsumeRequestComplete = new();
        /// <summary>
        /// Occurs when a purchase process has started.
        /// </summary>
        [EventField]
        public UnityEvent<SteamInventoryStartPurchaseResult_t> onPurchaseStarted = new();
        /// <summary>
        /// Occurs when an order has been authorised by the user.
        /// </summary>
        [EventField]
        public UnityEvent<ulong> onOrderAuthorized = new();
        /// <summary>
        /// Occurs when an order has not been authorised by the user.
        /// </summary>
        [EventField]
        public UnityEvent<ulong> onOrderNotAuthorized = new();
        /// <summary>
        /// Occurs when a promotional item has been successfully added to the inventory.
        /// </summary>
        [EventField]
        public UnityEvent<ItemDetail[]> onAddPromoComplete = new();
        /// <summary>
        /// Occurs when the possibility of exchanging items has changed.
        /// </summary>
        [EventField]
        public UnityEvent<bool> onCanExchangeChange = new();
        /// <summary>
        /// Occurs when an exchange operation has successfully completed.
        /// </summary>
        [EventField]
        public UnityEvent<ItemDetail[]> onExchangeComplete = new();

        /// <summary>
        /// Occurs when a consume request was rejected by the system before being sent to Steam.
        /// </summary>
        [EventField]
        public UnityEvent onConsumeRequestRejected = new();
        /// <summary>
        /// Occurs when a consume request failed to complete on the Steam backend.
        /// </summary>
        [EventField]
        public UnityEvent<EResult> onConsumeRequestFailed = new();
        /// <summary>
        /// Occurs when a purchase start request failed.
        /// </summary>
        [EventField]
        public UnityEvent<EResult> onPurchaseStartFailed = new();
        /// <summary>
        /// Occurs when an add-promo request was rejected by the system before being sent to Steam.
        /// </summary>
        [EventField]
        public UnityEvent onAddPromoRejected = new();
        /// <summary>
        /// Occurs when an add-promo request failed to complete on the Steam backend.
        /// </summary>
        [EventField]
        public UnityEvent<EResult> onAddPromoFailed = new();
        /// <summary>
        /// Occurs when an exchange request was rejected by the system before being sent to Steam.
        /// </summary>
        [EventField]
        public UnityEvent onExchangeRejected = new();
        /// <summary>
        /// Occurs when an exchange request failed to complete on the Steam backend.
        /// </summary>
        [EventField]
        public UnityEvent<EResult> onExchangeFailed = new();

        private SteamInventoryItemData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamInventoryItemData>();
            SteamTools.Events.OnMicroTxnAuthorisationResponse += HandleTransactionAuth;
            SteamTools.Events.OnInventoryResultReady += HandleInvResultReady;
        }

        private void OnDestroy()
        {
            SteamTools.Events.OnMicroTxnAuthorisationResponse -= HandleTransactionAuth;
            SteamTools.Events.OnInventoryResultReady -= HandleInvResultReady;
        }

        private void HandleTransactionAuth(AppData arg0, ulong arg1, bool arg2)
        {
            if(arg0 == AppData.Me)
            {
                if (arg2)
                    onOrderAuthorized?.Invoke(arg1);
                else
                    onOrderNotAuthorized?.Invoke(arg1);
            }
        }

        private void HandleInvResultReady(InventoryResult arg0)
        {
            if(arg0.result == EResult.k_EResultOK)
            {
                bool found = false;
                foreach(var item in arg0.items)
                {
                    if(item.Definition == _mInspector.Data)
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                    onStateChanged?.Invoke();
            }
        }
    }
}
#endif