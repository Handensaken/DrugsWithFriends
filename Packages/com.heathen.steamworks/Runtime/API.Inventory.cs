#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides a set of tools and operations for managing and interacting with Steam inventory,
    /// including functionalities for handling item definitions, managing promo items, processing inventory results,
    /// and working with in-game currency and inventory events.
    /// </summary>
    public static class Inventory
    {
        /// <summary>
        /// Provides methods and events for interacting with the Steam Inventory service,
        /// enabling functionalities such as item management, promo item handling, result processing,
        /// currency details, and inventory item properties.
        /// </summary>
        public static class Client
        {
            private class SerializationPointer
            {
                public UserData ExpectedUser;
                public Action<InventoryResult> Callback;
            }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                _itemIndex = new Dictionary<ItemData, List<ItemDetail>>();
                _mOnSteamInventoryDefinitionUpdate = new SteamInventoryDefinitionUpdateEvent();
                _mOnSteamInventoryResultReady = new SteamInventoryResultReadyEvent();
                _mOnSteamMtxAuthResponse = new SteamMicroTransactionAuthorizationResponce();
                _mResultHandles = new Dictionary<SteamInventoryResult_t, Action<InventoryResult>>();
                _mSerializationResults = new Dictionary<SteamInventoryResult_t, Action<byte[]>>();
                _mDeserializationResults = new Dictionary<SteamInventoryResult_t, SerializationPointer>();
                _mSteamInventoryEligiblePromoItemDefIDsT = null;
                _mSteamInventoryStartPurchaseResultT = null;
                _mSteamInventoryRequestPricesResultT = null;
            }

            private static Dictionary<ItemData, List<ItemDetail>> _itemIndex = new Dictionary<ItemData, List<ItemDetail>>();

            /// <summary>
            /// Represents the local currency code used for transactions within the Steamworks inventory system.
            /// The value is resolved from the current user's regional currency settings and updated when relevant data is fetched.
            /// </summary>
            public static Currency.Code LocalCurrencyCode
            {
                get;
                private set;
            }
            public static string LocalCurrencySymbol => Currency.GetSymbol(LocalCurrencyCode);

            private static SteamInventoryDefinitionUpdateEvent _mOnSteamInventoryDefinitionUpdate = new();
            private static SteamInventoryResultReadyEvent _mOnSteamInventoryResultReady = new();
            private static SteamMicroTransactionAuthorizationResponce _mOnSteamMtxAuthResponse = new();

            private static Dictionary<SteamInventoryResult_t, Action<InventoryResult>> _mResultHandles = new();
            private static Dictionary<SteamInventoryResult_t, Action<byte[]>> _mSerializationResults = new();
            private static Dictionary<SteamInventoryResult_t, SerializationPointer> _mDeserializationResults = new();

            private static CallResult<SteamInventoryEligiblePromoItemDefIDs_t> _mSteamInventoryEligiblePromoItemDefIDsT;
            private static CallResult<SteamInventoryStartPurchaseResult_t> _mSteamInventoryStartPurchaseResultT;
            private static CallResult<SteamInventoryRequestPricesResult_t> _mSteamInventoryRequestPricesResultT;

            /// <summary>
            /// Retrieves the list of detailed information associated with the specified item.
            /// </summary>
            /// <param name="item">The item data object for which details are to be retrieved.</param>
            /// <returns>A list of <see cref="ItemDetail"/> that represents the detailed information about the specified item.</returns>
            public static List<ItemDetail> Details(ItemData item)
            {
                if (!_itemIndex.ContainsKey(item))
                    _itemIndex.Add(item, new List<ItemDetail>());

                return _itemIndex[item];
            }

            /// <summary>
            /// Calculates the total quantity of items for the specified item data by summing the quantities of all its associated details.
            /// </summary>
            /// <param name="item">The item data object whose total quantity is to be calculated.</param>
            /// <returns>The total quantity of the specified item as a <see cref="long"/>. Returns 0 if the item has no associated details.</returns>
            public static long ItemTotalQuantity(ItemData item)
            {
                if (!_itemIndex.TryGetValue(item, out var value))
                    return 0;
                else
                    return value.Sum(p => Convert.ToInt64(p.Quantity));
            }

            /// <summary>
            /// Adds a promotional item to the inventory using the specified item definition and assigns the result to a callback.
            /// </summary>
            /// <param name="itemDef">The item definition representing the promotional item to be added.</param>
            /// <param name="callback">The callback function to handle the inventory result upon completion.</param>
            /// <returns>A boolean value indicating whether the operation was successfully initiated.</returns>
            public static bool AddPromoItem(SteamItemDef_t itemDef, Action<InventoryResult> callback)
            {
                if (callback == null)
                    return false;
                
                if (SteamInventory.AddPromoItem(out SteamInventoryResult_t resultHandle, itemDef))
                {
                    _mResultHandles.Add(resultHandle, callback);
                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Adds promotional items to the user's inventory.
            /// </summary>
            /// <param name="items">An array of item definitions representing the promotional items to be added.</param>
            /// <param name="callback">A callback function invoked after the operation is completed, providing the result of the inventory update.</param>
            /// <returns>A boolean indicating whether the operation was successfully initiated.</returns>
            public static bool AddPromoItems(ItemDefinitionSettings item, Action<InventoryResult> callback) =>
                AddPromoItem(item.Id, callback);

            /// <summary>
            /// Grant a specific one-time promotional item to the current user.
            /// <para>
            /// This can be safely called from the client because the items it can grant can be locked down via policies in the itemdefs. One of the primary scenarios for this call is to grant an item to users who also own a specific other game. If you want to grant a single promotional item then use AddPromoItem. If you want to grant all possible promo items then use GrantPromoItems.
            /// </para>
            /// <para>
            /// Any items that can be granted MUST have a "promo" attribute in their itemdef. That promo item list a set of APPIDs that the user must own to be granted this given item. This version will grant all items that have promo attributes specified for them in the configured item definitions. This allows adding additional promotional items without having to update the game client. For example the following will allow the item to be granted if the user owns either TF2 or SpaceWar.
            /// </para>
            /// </summary>
            /// <param name="itemDefs">The items to test for promo drop</param>
            /// <param name="callback">Invoked when the process completes</param>
            /// <returns></returns>
            public static bool AddPromoItems(SteamItemDef_t[] itemDefs, Action<InventoryResult> callback)
            {
                if (callback == null)
                    return false;

                if (SteamInventory.AddPromoItems(out SteamInventoryResult_t resultHandle, itemDefs, (uint)itemDefs.Length))
                {
                    _mResultHandles.Add(resultHandle, callback);
                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Adds a collection of promotional items to the user's inventory.
            /// </summary>
            /// <param name="items">An array of <see cref="ItemDefinitionSettings"/> representing the promotional items to be added.</param>
            /// <param name="callback">The callback method to execute when the operation is complete, providing the result of the inventory operation.</param>
            /// <returns>A boolean value indicating whether the request to add promotional items was successfully initiated.</returns>
            public static bool AddPromoItems(ItemDefinitionSettings[] items, Action<InventoryResult> callback) =>
                AddPromoItems(Array.ConvertAll<ItemDefinitionSettings, SteamItemDef_t>(items, p => p.Id), callback);
            /// <summary>
            /// Grant a specific one-time promotional item to the current user.
            /// <para>
            /// This can be safely called from the client because the items it can grant can be locked down via policies in the itemdefs. One of the primary scenarios for this call is to grant an item to users who also own a specific other game. If you want to grant a single promotional item then use AddPromoItem. If you want to grant all possible promo items then use GrantPromoItems.
            /// </para>
            /// <para>
            /// Any items that can be granted MUST have a "promo" attribute in their itemdef. That promo item list a set of APPIDs that the user must own to be granted this given item. This version will grant all items that have promo attributes specified for them in the configured item definitions. This allows adding additional promotional items without having to update the game client. For example the following will allow the item to be granted if the user owns either TF2 or SpaceWar.
            /// </para>
            /// </summary>
            /// <param name="itemDefs">The items to test for promo drop</param>
            /// <param name="callback">Invoked when the process completes</param>
            /// <returns></returns>
            public static bool AddPromoItems(IEnumerable<SteamItemDef_t> itemDefs, Action<InventoryResult> callback)
            {
                return AddPromoItems(Enumerable.ToArray(itemDefs), callback);
            }
            /// <summary>
            /// Checks whether an inventory result handle belongs to the specified Steam ID.
            /// </summary>
            /// <remarks>
            /// This is important when using DeserializeResult, to verify that a remote player is not pretending to have a different user's inventory.
            /// </remarks>
            /// <param name="resultHandle">The inventory result handle to check the Steam ID on.</param>
            /// <param name="steamIDExpected">The Steam ID to verify.</param>
            /// <returns></returns>
            public static bool CheckResultSteamID(SteamInventoryResult_t resultHandle, CSteamID steamIDExpected) => SteamInventory.CheckResultSteamID(resultHandle, steamIDExpected);
            /// <summary>
            /// Consumes items from a user's inventory. If the quantity of the given item goes to zero, it is permanently removed.
            /// </summary>
            /// <param name="resultHandle">Returns a new inventory result handle.</param>
            /// <param name="itemConsume">The item instance id to consume.</param>
            /// <param name="quantity">The number of items in that stack to consume.</param>
            /// <returns></returns>
            public static void ConsumeItem(SteamItemInstanceID_t itemConsume, uint quantity, Action<InventoryResult> callback)
            {
                if (callback == null)
                    return;

                SteamInventory.ConsumeItem(out SteamInventoryResult_t resultHandle, itemConsume, quantity);
                _mResultHandles.Add(resultHandle, callback);
            }
            /// <summary>
            /// Deserializes a result set and verifies the signature bytes.
            /// </summary>
            /// <remarks>
            /// This call has a potential soft-failure mode where the handle status is set to k_EResultExpired. GetResultItems will still succeed in this mode. The "expired" result could indicate that the data may be out of date - not just due to timed expiration (one hour), but also because one of the items in the result set may have been traded or consumed since the result set was generated. You could compare the timestamp from GetResultTimestamp to ISteamUtils::GetServerRealTime to determine how old the data is. You could simply ignore the "expired" result code and continue as normal, or you could request the player with expired data to send an updated result set.
            /// </remarks>
            /// <param name="resultHandle">Returns a new inventory result handle.</param>
            /// <param name="buffer">The buffer to deserialize.</param>
            /// <returns></returns>
            public static void DeserializeResult(UserData expectedUser, byte[] buffer, Action<InventoryResult> callback)
            {
                if (callback == null)
                    return;

                SteamInventory.DeserializeResult(out SteamInventoryResult_t resultHandle, buffer, (uint)buffer.Length);
                _mDeserializationResults.Add(resultHandle, new SerializationPointer
                {
                    Callback = callback,
                    ExpectedUser = expectedUser,
                });
            }
            /// <summary>
            /// Destroys a result handle and frees all associated memory.
            /// </summary>
            /// <param name="resultHandle">The inventory result handle to destroy.</param>
            public static void DestroyResult(SteamInventoryResult_t resultHandle) => SteamInventory.DestroyResult(resultHandle);
            /// <summary>
            /// Grant one item in exchange for a set of other items.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This can be used to implement crafting recipes or transmutations, or items which unpack themselves into other items (e.g., a chest).
            /// </para>
            /// <para>
            /// The caller of this API passes in the requested item and an array of existing items and quantities to exchange for it. The API currently takes an array of items to generate but at this time the size of that array must be 1 and the quantity of the new item must be 1.
            /// </para>
            /// <para>
            /// Any items that can be granted MUST have an exchange attribute in their itemdef. The exchange attribute specifies a set of recipes that are valid exchanges for this item. Exchange recipes are evaluated atomically by the Inventory Service; if the supplied components do not match the recipe, or do not contain sufficient quantity, the exchange will fail.
            /// </para>
            /// </remarks>
            /// <param name="resultHandle">Returns a new inventory result handle.</param>
            /// <param name="generate">The list of items that will be created by this call. Currently can only be 1 item!</param>
            /// <param name="generateQuantity">The quantity of each item in pArrayGenerate to create. Currently can only be 1 item and it must be set to 1!</param>
            /// <param name="destroy">The list of items that will be destroyed by this call.</param>
            /// <param name="destroyQuantity">The quantity of each item in pArrayDestroy to destroy.</param>
            /// <returns></returns>
            public static void ExchangeItems(SteamItemDef_t generate, SteamItemInstanceID_t[] destroy, uint[] destroyQuantity, Action<InventoryResult> callback)
            {
                if (callback == null)
                    return;

                SteamInventory.ExchangeItems(out SteamInventoryResult_t resultHandle, 
                    new SteamItemDef_t[] { generate }, 
                    new uint[]{ 1 }, 
                    1, 
                    destroy, 
                    destroyQuantity,
                    (uint)destroy.Length);
                _mResultHandles.Add(resultHandle, callback);
            }
            /// <summary>
            /// Grants specific items to the current user, for developers only.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This API is only intended for prototyping - it is only usable by Steam accounts that belong to the publisher group for your game.
            /// </para>
            /// <para>
            /// You can pass in an array of items, identified by their SteamItemDef_t and optionally a second array of corresponding quantities for each item. The length of these arrays MUST match!
            /// </para>
            /// </remarks>
            /// <param name="resultHandle">Returns a new inventory result handle.</param>
            /// <param name="itemDefs">The list of items to grant the user.</param>
            /// <param name="quantity">The quantity of each item in pArrayItemDefs to grant. This is optional, pass in NULL to specify 1 of each item.</param>
            public static void GenerateItems(SteamItemDef_t[] itemDefs, uint[] quantity, Action<InventoryResult> callback)
            {
                if (callback == null)
                    return;

                SteamInventory.GenerateItems(out SteamInventoryResult_t resultHandle, itemDefs, quantity, (uint)itemDefs.Length);
                _mResultHandles.Add(resultHandle, callback);
            }
            /// <summary>
            /// Start retrieving all items in the current users inventory.
            /// </summary>
            /// <remarks>
            /// <para>
            /// NOTE: Calls to this function are subject to rate limits and may return cached results if called too frequently. It is suggested that you call this function only when you are about to display the user's full inventory, or if you expect that the inventory may have changed.
            /// </para>
            /// </remarks>
            /// <param name="resultHandle"></param>
            public static void GetAllItems(Action<InventoryResult> callback = null)
            {
                _itemIndex.Clear();
                SteamInventory.GetAllItems(out SteamInventoryResult_t resultHandle);

                if (callback != null)
                {
                    if (_mResultHandles.ContainsKey(resultHandle))
                        _mResultHandles[resultHandle] = callback;
                    else
                        _mResultHandles.Add(resultHandle, callback);
                }
            }
            /// <summary>
            /// Returns the list of items eligible for Add Promo calls for the indicated user
            /// </summary>
            /// <param name="user">The user to test for</param>
            /// <param name="callback"></param>
            public static void GetEligiblePromoItems(UserData user, Action<EResult, ItemData[], bool> callback)
            {
                if (callback == null)
                    return;

                _mSteamInventoryEligiblePromoItemDefIDsT ??= CallResult<SteamInventoryEligiblePromoItemDefIDs_t>.Create();

                var handle = SteamInventory.RequestEligiblePromoItemDefinitionsIDs(user);
                _mSteamInventoryEligiblePromoItemDefIDsT.Set(handle, (result, e) =>
                {
                    if (e || result.m_result != EResult.k_EResultOK)
                    {
                        callback?.Invoke(result.m_result, new ItemData[0], e);
                    }
                    else
                    {
                        var buffer = new SteamItemDef_t[result.m_numEligiblePromoItemDefs];
                        uint count = (uint)result.m_numEligiblePromoItemDefs;
                        if (SteamInventory.GetEligiblePromoItemDefinitionIDs(user, buffer, ref count))
                        {
                            var data = new ItemData[count];
                            for (int i = 0; i < count; i++)
                            {
                                data[i] = buffer[i];
                            }
                            callback?.Invoke(result.m_result, data, e);
                        }
                        else
                            callback.Invoke(EResult.k_EResultFail, null, e);
                    }
                });
            }

            /// <summary>
            /// Returns the set of all item definition IDs which are defined in the App Admin panel of the Steamworks website.
            /// </summary>
            /// <remarks>
            /// This should be called in response to a SteamInventoryDefinitionUpdate_t callback. There is no reason to call this function if your game hardcodes the numeric definition IDs (eg, purple face mask = 20, blue weapon mod = 55) and does not allow for adding new item types without a client patch.
            /// </remarks>
            /// <param name="results"></param>
            /// <returns></returns>
            public static bool GetItemDefinitionIDs(out SteamItemDef_t[] results)
            {
                uint count = 0;
                if (SteamInventory.GetItemDefinitionIDs(null, ref count))
                {
                    results = new SteamItemDef_t[count];
                    return SteamInventory.GetItemDefinitionIDs(results, ref count);
                }
                else
                {
                    results = new SteamItemDef_t[0];
                    return false;
                }
            }
            /// <summary>
            /// Gets a property value for a specific item definition.
            /// </summary>
            /// <remarks>
            /// Note that some properties (for example, "name") may be localised and will depend on the current Steam language settings (see ISteamApps::GetCurrentGameLanguage). Property names are always ASCII alphanumeric and underscores.
            /// </remarks>
            /// <param name="item">The item definition to get the properties for.</param>
            /// <param name="propertyName">The property name to get the value for</param>
            /// <returns></returns>
            public static string GetItemDefinitionProperty(SteamItemDef_t item, string propertyName)
            {
                uint count = 0;
                if (SteamInventory.GetItemDefinitionProperty(item, propertyName, out _, ref count))
                {
                    SteamInventory.GetItemDefinitionProperty(item, propertyName, out string value, ref count);
                    return value;
                }
                else
                {
                    Debug.LogWarning($"Steam has reported that the item definitions have not been loaded from the server, or no item definitions exist for the current application, or `{propertyName}` is not a valid property.");
                    return string.Empty;
                }
            }
            /// <summary>
            /// Returns a list of the available properties on a given item
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public static string[] GetItemDefinitionProperties(SteamItemDef_t item)
            {
                uint count = 0;
                if (SteamInventory.GetItemDefinitionProperty(item, null, out string value, ref count))
                {
                    SteamInventory.GetItemDefinitionProperty(item, null, out value, ref count);
                    if (value != null)
                        return value.Split(',');
                    else
                    {
                        Debug.LogWarning($"Steam has reported that the item definitions have not been loaded from the server, or no item definitions exist for the current application.");
                        return new string[0];
                    }
                }
                else
                {
                    Debug.LogWarning($"Steam has reported that the item definitions have not been loaded from the server, or no item definitions exist for the current application.");
                    return new string[0];
                }
            }
            /// <summary>
            /// Gets the state of a subset of the current user's inventory.
            /// </summary>
            /// <param name="instanceIds">A list of the item instance ids to update the state of.</param>
            /// <param name="callback"></param>
            public static void GetItemsByID(SteamItemInstanceID_t[] instanceIds, Action<InventoryResult> callback = null)
            {
                SteamInventory.GetItemsByID(out SteamInventoryResult_t resultHandle, instanceIds, (uint)instanceIds.Length);

                if (callback != null)
                    _mResultHandles.Add(resultHandle, callback);
            }
            /// <summary>
            /// After a successful call to RequestPrices, you can call this method to get the pricing for a specific item definition.
            /// </summary>
            /// <param name="item"></param>
            /// <param name="currentPrice"></param>
            /// <param name="basePrice"></param>
            /// <returns></returns>
            public static bool GetItemPrice(SteamItemDef_t item, out ulong currentPrice, out ulong basePrice) => SteamInventory.GetItemPrice(item, out currentPrice, out basePrice);
            /// <summary>
            /// After a successful call to RequestPrices, you can call this method to get all the pricing for applicable item definitions. 
            /// </summary>
            /// <param name="items">The array of item definition ids to populate</param>
            /// <param name="currentPrices">The array of prices for each corresponding item definition id in pArrayItemDefs. Prices are rendered in the user's local currency.</param>
            /// <param name="basePrices">The array of prices for each corresponding item definition id in pArrayItemDefs. Prices are rendered in the user's local currency.</param>
            /// <returns></returns>
            public static bool GetItemsWithPrices(out SteamItemDef_t[] items, out ulong[] currentPrices, out ulong[] basePrices)
            {
                var count = SteamInventory.GetNumItemsWithPrices();
                items = new SteamItemDef_t[count];
                currentPrices = new ulong[count];
                basePrices = new ulong[count];
                return SteamInventory.GetItemsWithPrices(items, currentPrices, basePrices, count);
            }
            /// <summary>
            /// Gets the dynamic properties from an item in an inventory result set.
            /// </summary>
            /// <param name="resultHandle">The result handle containing the item to get the properties of.</param>
            /// <param name="itemIndex"></param>
            /// <param name="propertyName">The property name to get the value for. If you pass in NULL then pchValueBuffer will contain a comma-separated list of all the available names.</param>
            /// <param name="valueBuffer">Returns the value associated with propertyName.</param>
            /// <param name="bufferSize">This should be set to the size of pchValueBuffer, and returns the number of bytes required to hold the value.</param>
            /// <returns></returns>
            public static bool GetResultItemProperty(SteamInventoryResult_t resultHandle, uint itemIndex, string propertyName, out string valueBuffer, ref uint bufferSize) => SteamInventory.GetResultItemProperty(resultHandle, itemIndex, propertyName, out valueBuffer, ref bufferSize);
            /// <summary>
            /// Get the items associated with an inventory result handle.
            /// </summary>
            /// <param name="resultHandle">The inventory result handle to get the items for.</param>
            /// <param name="items">The details are returned by copying them into this array.</param>
            /// <param name="count">This should be set to the length of pOutItemsArray. If pOutItemsArray is NULL then this will return the number of elements the array needs to hold.</param>
            /// <returns></returns>
            public static bool GetResultItems(SteamInventoryResult_t resultHandle, SteamItemDetails_t[] items, ref uint count) => SteamInventory.GetResultItems(resultHandle, items, ref count);
            /// <summary>
            /// Gets the server time at which the result was generated.
            /// </summary>
            /// <param name="resultHandle">The inventory result handle to get the timestamp for.</param>
            /// <returns></returns>
            public static DateTime GetResultTimestamp(SteamInventoryResult_t resultHandle) => new DateTime(1970, 1, 1).AddSeconds(SteamInventory.GetResultTimestamp(resultHandle));
            /// <summary>
            /// Grant all potential one-time promotional items to the current user.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This can be safely called from the client because the items it can grant can be locked down via policies in the itemdefs. One of the primary scenarios for this call is to grant an item to users who also own a specific other game. If you want to grant specific promotional items rather than all of them see: AddPromoItem and AddPromoItems.
            /// </para>
            /// <para>
            /// Any items that can be granted MUST have a "promo" attribute in their itemdef. That promo item list a set of APPIDs that the user must own to be granted this given item. This version will grant all items that have promo attributes specified for them in the configured item definitions. This allows adding additional promotional items without having to update the game client. For example the following will allow the item to be granted if the user owns either TF2 or SpaceWar.
            /// </para>
            /// </remarks>
            /// <param name="callback"></param>
            public static bool GrantPromoItems(Action<InventoryResult> callback = null)
            {
                if (callback == null)
                    return false;

                if (SteamInventory.GrantPromoItems(out SteamInventoryResult_t resultHandle))
                {
                    if (callback != null)
                        _mResultHandles.Add(resultHandle, callback);

                    return true;
                }
                else
                    return false;
            }
            /// <summary>
            /// Triggers an asynchronous load and refresh of item definitions.
            /// </summary>
            /// <remarks>
            /// Triggers a SteamInventoryDefinitionUpdate_t callback.
            /// </remarks>
            public static bool LoadItemDefinitions() => SteamInventory.LoadItemDefinitions();
            /// <summary>
            /// Request prices for all item definitions that can be purchased in the user's local currency. A SteamInventoryRequestPricesResult_t call result will be returned with the user's local currency code. After that, you can call GetNumItemsWithPrices and GetItemsWithPrices to get prices for all the known item definitions, or GetItemPrice for a specific item definition.
            /// </summary>
            /// <param name="callback"></param>
            public static void RequestPrices(Action<SteamInventoryRequestPricesResult_t, bool> callback)
            {
                _mSteamInventoryRequestPricesResultT ??= CallResult<SteamInventoryRequestPricesResult_t>.Create();

                var handle = SteamInventory.RequestPrices();
                _mSteamInventoryRequestPricesResultT.Set(handle, (response, ioError) =>
                {
                    if (ioError || response.m_result != EResult.k_EResultOK)
                    {
                        LocalCurrencyCode = Currency.Code.Unknown;
                        Debug.LogWarning("Failed to fetch current prices for the list of available inventory items.\nSteam Response: " + response.m_result.ToString());
                    }
                    else
                    {
                        LocalCurrencyCode = (Currency.Code)Enum.Parse(typeof(Currency.Code), response.m_rgchCurrency.ToUpper());
                    }

                    callback?.Invoke(response, ioError);
                });
            }
            /// <summary>
            /// Gets the state of a subset of the current user's inventory and serializes the data.
            /// <para>
            /// Serialized result sets contain a short signature which can't be forged or replayed across different game sessions.
            /// </para>
            /// </summary>
            /// <param name="instanceIds">The IDs of items to fetch a result set for to serialize</param>
            /// <param name="callback"></param>
            public static void SerializeItemResultsByID(SteamItemInstanceID_t[] instanceIds, Action<byte[]> callback)
            {
                if (callback == null)
                    return;

                SteamInventory.GetItemsByID(out SteamInventoryResult_t resultHandle, instanceIds, (uint)instanceIds.Length);
                _mSerializationResults.Add(resultHandle, callback);
            }
            /// <summary>
            /// Start retrieving all items in the current users inventory and serializes the data.
            /// <para>
            /// Serialized result sets contain a short signature which can't be forged or replayed across different game sessions.
            /// </para>
            /// </summary>
            /// <param name="callback"></param>
            public static void SerializeAllItemResults(Action<byte[]> callback)
            {
                if (callback == null)
                    return;

                SteamInventory.GetAllItems(out SteamInventoryResult_t resultHandle);
                _mSerializationResults.Add(resultHandle, callback);
            }
            /// <summary>
            /// Starts the purchase process for the user, given a "shopping cart" of item definitions that the user would like to buy. The user will be prompted in the Steam Overlay to complete the purchase in their local currency, funding their Steam Wallet if necessary, etc.
            /// </summary>
            /// <remarks>
            /// <para>
            /// If the purchase process was started successfully, then m_ulOrderID and m_ulTransID will be valid in the SteamInventoryStartPurchaseResult_t call result.
            /// </para>
            /// <para>
            /// If the user authorizes the transaction and completes the purchase, then the callback SteamInventoryResultReady_t will be triggered and you can then retrieve what new items the user has acquired. NOTE: You must call DestroyResult on the inventory result for when you are done with it.
            /// </para>
            /// </remarks>
            /// <param name="items">The array of item definition ids that the user wants to purchase.</param>
            /// <param name="quantities">The array of quantities of each item definition that the user wants to purchase.</param>
            /// <param name="callback"></param>
            public static void StartPurchase(SteamItemDef_t[] items, uint[] quantities, Action<SteamInventoryStartPurchaseResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _mSteamInventoryStartPurchaseResultT ??= CallResult<SteamInventoryStartPurchaseResult_t>.Create();

                var handle = SteamInventory.StartPurchase(items, quantities, (uint)items.Length);
                _mSteamInventoryStartPurchaseResultT.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Transfer items between stacks within a user's inventory.
            /// </summary>
            /// <param name="source">The source item to transfer.</param>
            /// <param name="quantity">The quantity of the item that will be transferred from itemIdSource to itemIdDest.</param>
            /// <param name="destination">The destination item. You can pass k_SteamItemInstanceIDInvalid to split the source stack into a new item stack with the requested quantity.</param>
            /// <param name="callback"></param>
            public static void TransferItemQuantity(SteamItemInstanceID_t source, uint quantity, SteamItemInstanceID_t destination, Action<InventoryResult> callback)
            {
                if (callback == null)
                    return;

                SteamInventory.TransferItemQuantity(out SteamInventoryResult_t resultHandle, source, quantity, destination);
                _mResultHandles.Add(resultHandle, callback);
            }
            /// <summary>
            /// Trigger an item drop if the user has played a long enough period of time.
            /// </summary>
            /// <param name="item">This must refer to an itemdefid of the type "playtimegenerator". See the inventory schema for more details.</param>
            /// <param name="callback"></param>
            public static void TriggerItemDrop(SteamItemDef_t item, Action<InventoryResult> callback)
            {
                if (callback == null)
                    return;

                SteamInventory.TriggerItemDrop(out SteamInventoryResult_t resultHandle, item);
                _mResultHandles.Add(resultHandle, callback);
            }
            /// <summary>
            /// Starts a transaction request to update dynamic properties on items for the current user. This call is rate-limited by user, so property modifications should be batched as much as possible (e.g. at the end of a map or game session). After calling SetProperty or RemoveProperty for all the items that you want to modify, you will need to call SubmitUpdateProperties to send the request to the Steam servers. A SteamInventoryResultReady_t callback will be fired with the results of the operation.
            /// </summary>
            /// <returns></returns>
            public static SteamInventoryUpdateHandle_t StartUpdateProperties() => SteamInventory.StartUpdateProperties();
            /// <summary>
            /// Submits the transaction request to modify dynamic properties on items for the current user. See StartUpdateProperties.
            /// </summary>
            /// <param name="handle">The update handle corresponding to the transaction request, returned from StartUpdateProperties.</param>
            /// <param name="callback"></param>
            public static void SubmitUpdateProperties(SteamInventoryUpdateHandle_t handle, Action<InventoryResult> callback)
            {
                if (callback == null)
                    return;

                SteamInventory.SubmitUpdateProperties(handle, out SteamInventoryResult_t resultHandle);
                _mResultHandles.Add(resultHandle, callback);
            }
            /// <summary>
            /// Removes a dynamic property for the given item.
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="item"></param>
            /// <param name="propertyName"></param>
            public static void RemoveProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t item, string propertyName) => SteamInventory.RemoveProperty(handle, item, propertyName);
            /// <summary>
            /// Sets a dynamic property for the given item. Supported value types are strings, boolean, 64 bit integers, and 32 bit floats.
            /// </summary>
            /// <param name="handle">The update handle corresponding to the transaction request, returned from StartUpdateProperties.</param>
            /// <param name="item">ID of the item being modified.</param>
            /// <param name="propertyName">The dynamic property being added or updated.</param>
            /// <param name="data">The value to be set</param>
            public static void SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t item, string propertyName, string data) => SteamInventory.SetProperty(handle, item, propertyName, data);
            /// <summary>
            /// Sets a dynamic property for the given item. Supported value types are strings, boolean, 64 bit integers, and 32 bit floats.
            /// </summary>
            /// <param name="handle">The update handle corresponding to the transaction request, returned from StartUpdateProperties.</param>
            /// <param name="item">ID of the item being modified.</param>
            /// <param name="propertyName">The dynamic property being added or updated.</param>
            /// <param name="data">The value to be set</param>
            public static void SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t item, string propertyName, bool data) => SteamInventory.SetProperty(handle, item, propertyName, data);
            /// <summary>
            /// Sets a dynamic property for the given item. Supported value types are strings, boolean, 64 bit integers, and 32 bit floats.
            /// </summary>
            /// <param name="handle">The update handle corresponding to the transaction request, returned from StartUpdateProperties.</param>
            /// <param name="item">ID of the item being modified.</param>
            /// <param name="propertyName">The dynamic property being added or updated.</param>
            /// <param name="data">The value to be set</param>
            public static void SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t item, string propertyName, long data) => SteamInventory.SetProperty(handle, item, propertyName, data);
            /// <summary>
            /// Sets a dynamic property for the given item. Supported value types are strings, boolean, 64 bit integers, and 32 bit floats.
            /// </summary>
            /// <param name="handle">The update handle corresponding to the transaction request, returned from StartUpdateProperties.</param>
            /// <param name="item">ID of the item being modified.</param>
            /// <param name="propertyName">The dynamic property being added or updated.</param>
            /// <param name="data">The value to be set</param>
            public static void SetProperty(SteamInventoryUpdateHandle_t handle, SteamItemInstanceID_t item, string propertyName, float data) => SteamInventory.SetProperty(handle, item, propertyName, data);
            /// <summary>
            /// Constructs an <see cref="ItemDetail"/> object based on a native <see cref="SteamItemDetails_t"/> object and its source result list
            /// </summary>
            /// <param name="result">The result list to query</param>
            /// <param name="index">The index in this list the details belong to</param>
            /// <param name="detail">The detail we should extend</param>
            /// <returns></returns>
            private static ItemDetail GetExtendedItemDetail(SteamInventoryResult_t result, uint index, SteamItemDetails_t detail)
            {
                uint count = 0;
                SteamInventory.GetResultItemProperty(result, index, null, out string value, ref count);
                SteamInventory.GetResultItemProperty(result, index, null, out value, ref count);
                var properties = value.Split(',');
                var values = new List<ItemProperty>();
                var tags = new ItemTag[0];
                var dynProp = string.Empty;

                if (properties.Length > 0)
                {
                    for (int ii = 0; ii < properties.Length; ii++)
                    {
                        count = 0;
                        SteamInventory.GetResultItemProperty(result, index, properties[ii], out _, ref count);
                        SteamInventory.GetResultItemProperty(result, index, properties[ii], out value, ref count);

                        if (properties[ii] == "tags")
                        {
                            var tagArray = value.Split(';');
                            tags = new ItemTag[tagArray.Length];
                            for (int iii = 0; iii < tagArray.Length; iii++)
                            {
                                if (tagArray[iii].Contains(":"))
                                {
                                    var tVal = tagArray[iii].Split(':');
                                    if (tVal.Length >= 2)
                                    {
                                        tags[iii] = new ItemTag
                                        {
                                            category = tVal[0],
                                            tag = tVal[1]
                                        };
                                    }
                                    else
                                    {
                                        tags[iii] = new ItemTag
                                        {
                                            category = tVal[0]
                                        };
                                    }
                                }
                                else
                                {
                                    tags[iii] = new ItemTag
                                    {
                                        category = tagArray[index]
                                    };
                                }
                            }
                        }
                        else if (properties[ii] == "dynamic_props")
                        {
                            dynProp = value;
                        }
                        else
                        {
                            values.Add(new ItemProperty
                            {
                                key = properties[ii],
                                value = value
                            });
                        }
                    }
                }

                var nDet = new ItemDetail
                {
                    ItemDetails = detail,
                    properties = values.ToArray(),
                    dynamicProperties = dynProp,
                    tags = tags,
                };

                ItemData data = nDet.Definition;
                if(_itemIndex.ContainsKey(data))
                {
                    var list = _itemIndex[data];
                    list.RemoveAll(p => p.ItemId == nDet.ItemId);
                    list.Add(nDet);
                    _itemIndex[data] = list;
                }
                else
                {
                    _itemIndex.Add(data, new List<ItemDetail> { nDet });
                }
                return nDet;
            }

            internal static void HandleInventoryResults(SteamInventoryResultReady_t results)
            {
                if (_mSerializationResults.ContainsKey(results.m_handle))
                {
                    //Serialization request so we don't need to process the results
                    SteamInventory.SerializeResult(results.m_handle, null, out uint size);
                    var buffer = new byte[size];
                    SteamInventory.SerializeResult(results.m_handle, buffer, out size);
                    _mSerializationResults[results.m_handle]?.Invoke(buffer);
                    _mSerializationResults.Remove(results.m_handle);
                    SteamInventory.DestroyResult(results.m_handle);
                }
                else
                {
                    //We should process the results
                    uint count = 0;
                    var inventoryResult = new InventoryResult
                    {
                        items = Array.Empty<ItemDetail>(),
                        result = results.m_result,
                        Timestamp = new DateTime(1970, 1, 1).AddSeconds(SteamInventory.GetResultTimestamp(results.m_handle))
                    };

                    SteamInventory.GetResultItems(results.m_handle, null, ref count);
                    if (count > 0)
                    {
                        var buffer = new SteamItemDetails_t[count];
                        var extendedResults = new ItemDetail[count];
                        SteamInventory.GetResultItems(results.m_handle, buffer, ref count);
                        for (uint i = 0; i < count; i++)
                        {
                            extendedResults[i] = GetExtendedItemDetail(results.m_handle, i, buffer[i]);
                        }

                        inventoryResult = new InventoryResult
                        {
                            //Waiting handle found so let them know we have it
                            items = extendedResults,
                            result = results.m_result,
                            Timestamp = new DateTime(1970, 1, 1).AddSeconds(SteamInventory.GetResultTimestamp(results.m_handle))
                        };
                    }

                    if (_mDeserializationResults.ContainsKey(results.m_handle))
                    {
                        var record = _mDeserializationResults[results.m_handle];

                        if (!SteamInventory.CheckResultSteamID(results.m_handle, record.ExpectedUser))
                        {
                            inventoryResult.result = EResult.k_EResultFail;
                        }

                        record.Callback?.Invoke(inventoryResult);
                        _mDeserializationResults.Remove(results.m_handle);
                    }
                    else
                    {
                        SteamTools.Events.InvokeOnInventoryResultReady(inventoryResult);

                        if (_mResultHandles.ContainsKey(results.m_handle))
                        {
                            _mResultHandles[results.m_handle]?.Invoke(inventoryResult);
                            _mResultHandles.Remove(results.m_handle);
                        }
                    }

                    SteamInventory.DestroyResult(results.m_handle);
                }
            }
        }
    }
}
#endif