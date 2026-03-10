#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a struct for managing and interacting with Steam inventory items,
    /// providing various methods for querying item data, handling pricing information,
    /// and performing operations such as consumption, exchange, generation, purchase,
    /// and item drops.
    /// </summary>
    [Serializable]
    public struct ItemData : IEquatable<ItemData>, IEquatable<int>, IEquatable<SteamItemDef_t>, IComparable<ItemData>, IComparable<int>, IComparable<SteamItemDef_t>
    {
        /// <summary>
        /// Represents the unique identifier for an item within the Steam Inventory system.
        /// </summary>
        public int id;

        /// <summary>
        /// Retrieves the display name of the item associated with the Steam Inventory system.
        /// </summary>
        public readonly string Name => API.Inventory.Client.GetItemDefinitionProperty(new SteamItemDef_t(id), "name");

        /// <summary>
        /// Determines whether the item has a price associated with it in the Steam Inventory system.
        /// </summary>
        public readonly bool HasPrice => API.Inventory.Client.GetItemPrice(new SteamItemDef_t(id), out ulong _, out ulong _);

        /// <summary>
        /// Represents the current currency code associated with the local user's Steam account.
        /// </summary>
        public static Currency.Code CurrencyCode => API.Inventory.Client.LocalCurrencyCode;

        /// <summary>
        /// Provides the symbol of the local currency based on the player's regional settings
        /// as determined by the Steam client's inventory API.
        /// </summary>
        public static string CurrencySymbol => API.Inventory.Client.LocalCurrencySymbol;

        /// <summary>
        /// Retrieves the current price of the item in the smallest currency unit for the user's local currency.
        /// If the price cannot be determined, this returns 0.
        /// </summary>
        public readonly ulong CurrentPrice
        {
            get
            {
                if (API.Inventory.Client.GetItemPrice(new SteamItemDef_t(id), out ulong current, out ulong _))
                {
                    return current;
                }
                else
                    return 0;
            }
        }

        /// <summary>
        /// Represents the base price of the inventory item in its default currency.
        /// </summary>
        public readonly ulong BasePrice
        {
            get
            {
                if (API.Inventory.Client.GetItemPrice(new SteamItemDef_t(id), out ulong _, out ulong basePrice))
                {
                    return basePrice;
                }
                else
                    return 0;
            }
        }

        /// <summary>
        /// Retrieves the list of <see cref="ItemDetail"/> objects that represent instances or "stacks" of this item.
        /// <para>Each <see cref="ItemDetail"/> may have a quantity of zero or more, conceptualised as a "stack" of this item type. This method provides detailed information about the specific instances of the item.</para>
        /// </summary>
        /// <returns>A list of <see cref="ItemDetail"/> associated with this item.</returns>
        public readonly List<ItemDetail> GetDetails() => API.Inventory.Client.Details(this);

        /// <summary>
        /// Retrieves the total quantity of this item owned by the user.
        /// <para>The total quantity is calculated as the sum of all instances of <see cref="ItemData"/> associated with this item.</para>
        /// </summary>
        /// <returns>The total quantity of the item owned by the user as a <see cref="long"/>.</returns>
        public readonly long GetTotalQuantity() => API.Inventory.Client.ItemTotalQuantity(this);

        /// <summary>
        /// Attempts to add a promotional item to the user's inventory. This action is only successful if the item qualifies as a promotional item and the user meets the eligibility criteria.
        /// </summary>
        /// <param name="callback">A delegate of type <see cref="Action{InventoryResult}"/> that is invoked with the operation result when the process completes.</param>
        /// <returns>A boolean value indicating whether the process was successfully initiated.</returns>
        public readonly bool AddPromoItem(Action<InventoryResult> callback) =>
            API.Inventory.Client.AddPromoItem(new SteamItemDef_t(id), callback);

        /// <summary>
        /// Creates a collection of <see cref="ConsumeOrder"/> objects that specify the quantities of <see cref="ItemDetail"/> to be consumed for a given consumption process.
        /// </summary>
        /// <param name="quantity">The total quantity of the item to be consumed in this process.</param>
        /// <returns>An array of <see cref="ConsumeOrder"/> defining the consumption process. Returns null if the desired quantity exceeds the available quantity.</returns>
        public readonly ConsumeOrder[] GetConsumeOrders(uint quantity)
        {
            var details = GetDetails();

            if (details.Sum(p => (long)p.Quantity) < quantity)
                return null;

            var results = new List<ConsumeOrder>();
            uint count = 0;
            var index = 0;
            while (count < quantity)
            {
                uint cCount = (uint)Mathf.Min(details[index].Quantity, quantity - count);
                count += cCount;

                results.Add(new ConsumeOrder
                {
                    Detail = details[index].ItemDetails,
                    Quantity = cCount
                });
            }

            return results.ToArray();
        }

        /// <summary>
        /// Attempts to consume a single instance of this item.
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes.</param>
        /// <returns>Returns <c>true</c> if an instance of the item was successfully consumed; otherwise, <c>false</c> if no instances of the item are available to consume.</returns>
        public readonly bool Consume(Action<InventoryResult> callback)
        {
            var details = GetDetails();

            if (details.Sum(p => (long)p.Quantity) < 1)
                return false;

            var instance = details.First(p => p.Quantity > 0);
            API.Inventory.Client.ConsumeItem(instance.ItemDetails.m_itemId, 1, callback);
            return true;
        }
        /// <summary>
        /// Attempts to consume the order provided. You can get an order for more than one item to be consumed at once via the <see cref="GetConsumeOrders(uint)"/> method.
        /// </summary>
        /// <param name="order">The order to consume.</param>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes.</param>
        public readonly void Consume(ConsumeOrder order, Action<InventoryResult> callback) => API.Inventory.Client.ConsumeItem(order.Detail.m_itemId, order.Quantity, callback);
        /// <summary>
        /// Attempts to consume a given quantity of the item; this will use <see cref="GetConsumeOrders(uint)"/> to get orders and then will consume each if possible.
        /// </summary>
        /// <param name="quantity">The number of items to be consumed.</param>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes.</param>
        /// <returns>Returns <c>true</c> if a sufficient quantity of the item was found and the consumption process was started; otherwise, <c>false</c>.</returns>
        public readonly bool Consume(uint quantity, Action<InventoryResult> callback)
        {
            var orders = GetConsumeOrders(quantity);
            if(orders == null || orders.Length < 1)
            {
                return false;
            }
            else
            {
                List<ItemDetail> details = new List<ItemDetail>();
                EResult eResult = EResult.k_EResultOK;
                var worker = new BackgroundWorker();
                worker.DoWork += (_, eventArgs) =>
                {
                    foreach(var order in orders)
                    {
                        bool wait = true;
                        
                        API.Inventory.Client.ConsumeItem(order.Detail.m_itemId, order.Quantity, (r) =>
                        {
                            eResult = r.result;

                            if (eResult == EResult.k_EResultOK)
                                details.AddRange(r.items);
                                
                            wait = false;
                        });

                        while(wait)
                            Thread.Sleep(50);

                        if (eResult != EResult.k_EResultOK)
                            break;
                    }

                    var final = new InventoryResult
                    {
                        result = eResult,
                        items = details.ToArray()
                    };

                    eventArgs.Result = final;
                };
                worker.RunWorkerCompleted += (_, eventArgs) =>
                {
                    var final = (InventoryResult)eventArgs.Result;
                    callback?.Invoke(final);
                    worker.Dispose();
                };
                return true;
            }
        }
        /// <summary>
        /// Gets a collection of <see cref="ExchangeEntry"/> for use in exchange processes.
        /// </summary>
        /// <param name="quantity">The number of items that should be included in the exchange.</param>
        /// <param name="entries">The resulting <see cref="ExchangeEntry"/> that represent the requested items.</param>
        /// <returns>True if sufficient items were found, false if the exchange request is not possible.</returns>
        public readonly bool GetExchangeEntry(uint quantity, out ExchangeEntry[] entries)
        {
            var details = GetDetails();

            if (details.Sum(p => (long)p.Quantity) < quantity)
            {
                entries = Array.Empty<ExchangeEntry>();
                return false;
            }
            else
            {
                var list = new List<ExchangeEntry>();
                uint count = 0;
                var index = 0;
                while (count < quantity)
                {
                    if (details[index].Quantity <= quantity - count)
                    {
                        if (details[index].Quantity > 0)
                        {
                            list.Add(new ExchangeEntry
                            {
                                Instance = details[index].ItemId,
                                Quantity = details[index].Quantity
                            });
                            count += details[index].Quantity;
                        }
                    }
                    else
                    {
                        if (details[index].Quantity > 0)
                        {
                            var remainder = quantity - count;
                            list.Add(new ExchangeEntry
                            {
                                Instance = details[index].ItemId,
                                Quantity = remainder,
                            });
                            count += remainder;
                        }
                    }

                    index++;
                }

                entries = list.ToArray();
                return true;
            }
        }
        /// <summary>
        /// Exchange the items represented by the <see cref="ExchangeEntry"/> collection you pass in to produce one of this item.
        /// <para>For this to work, this item must define an exchange recipe that takes the items defined in the <see cref="ExchangeEntry"/> collection you provided. This is a secure way to convert one or more items into a specific other item, e.g. "crafting".</para>
        /// </summary>
        /// <param name="recipeEntries">A collection of <see cref="ExchangeEntry"/> to be consumed.</param>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes.</param>
        public readonly void Exchange(IEnumerable<ExchangeEntry> recipeEntries, Action<InventoryResult> callback)
        {
            var list = recipeEntries.ToArray();

            var instances = new SteamItemInstanceID_t[list.Length];
            var counts = new uint[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                instances[i] = list[i].Instance;
                counts[i] = list[i].Quantity;
            }
            API.Inventory.Client.ExchangeItems(new SteamItemDef_t(id), instances, counts, callback);
        }
        /// <summary>
        /// This is only available to developers of this app and should only be used prior to launch of the game for testing purposes.
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes.</param>
        public readonly void GenerateItem(Action<InventoryResult> callback)
        {
            API.Inventory.Client.GenerateItems(new[] { new SteamItemDef_t(id) }, new uint[] { 1 }, callback);
        }
        /// <summary>
        /// This is only available to developers of this app and should only be used prior to launch of the game for testing purposes.
        /// </summary>
        /// <param name="quantity">The quantity of items to generate.</param>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes.</param>
        public readonly void GenerateItem(uint quantity, Action<InventoryResult> callback)
        {
            API.Inventory.Client.GenerateItems(new[] { new SteamItemDef_t(id) }, new[] { quantity }, callback);
        }
        /// <summary>
        /// Starts the purchase process.
        /// <para>Assuming the item has a valid price configured, this will open the Steam overlay with this item in the shopping cart. The <see cref="SteamInventoryStartPurchaseResult_t"/> provided in the callback will indicate the transaction ID which can be used with <see cref="API.Inventory.Client.OnSteamMicroTransactionAuthorizationResponse"/> to know the purchase is completed if it is completed.</para>
        /// <para>Generally speaking, you do not need to track when the purchase is completed, and in most games, the game will simply monitor for any changes to inventory. This is advisable as it is possible the player outside your game uses the Steam store or other mechanism to purchase an item, or was gifted an item from another player, or in some other way acquired an item from a process outside your game's control. As such, it is important that your game handles the possibility of inventory changing at any time, which would of course catch any purchases made from within the game as well.</para>
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="SteamInventoryStartPurchaseResult_t"/> result, <see cref="bool"/> ioError) that is invoked when the process completes.</param>
        public readonly void StartPurchase(Action<SteamInventoryStartPurchaseResult_t, bool> callback)
        {
            API.Inventory.Client.StartPurchase(new[] { new SteamItemDef_t(id) }, new uint[] { 1 }, callback);
        }
        /// <summary>
        /// Starts the purchase process.
        /// <para>Assuming the item has a valid price configured, this will open the Steam overlay with this item in the shopping cart. The <see cref="SteamInventoryStartPurchaseResult_t"/> provided in the callback will indicate the transaction ID which can be used with <see cref="API.Inventory.Client.OnSteamMicroTransactionAuthorizationResponse"/> to know the purchase is completed if it is completed.</para>
        /// <para>Generally speaking, you do not need to track when the purchase is completed, and in most games, the game will simply monitor for any changes to inventory. This is advisable as it is possible the player outside your game uses the Steam store or other mechanism to purchase an item, or was gifted an item from another player, or in some other way acquired an item from a process outside your game's control. As such, it is important that your game handles the possibility of inventory changing at any time, which would of course catch any purchases made from within the game as well.</para>
        /// </summary>
        /// <param name="count">The number of items to add to the shopping cart.</param>
        /// <param name="callback">A delegate of the form (<see cref="SteamInventoryStartPurchaseResult_t"/> result, <see cref="bool"/> ioError) that is invoked when the process completes.</param>
        public readonly void StartPurchase(uint count, Action<SteamInventoryStartPurchaseResult_t, bool> callback)
        {
            API.Inventory.Client.StartPurchase(new[] { new SteamItemDef_t(id) }, new[] { count }, callback);
        }
        /// <summary>
        /// Gets the price values for the item.
        /// </summary>
        /// <param name="currentPrice">The current price in base 100, e.g. 150 = $1.50.</param>
        /// <param name="basePrice">The base price in base 100, e.g. 150 = $1.50.</param>
        /// <returns>True if the item has a price, false otherwise.</returns>
        public readonly bool GetPrice(out ulong currentPrice, out ulong basePrice) => API.Inventory.Client.GetItemPrice(new SteamItemDef_t(id), out currentPrice, out basePrice);
        /// <summary>
        /// Request a drop of this item; this will cause the item to be dropped if the item has a valid promo rule configured and the user is eligible for it.
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes.</param>
        public readonly void TriggerDrop(Action<InventoryResult> callback) => API.Inventory.Client.TriggerItemDrop(new SteamItemDef_t(id), callback);
        /// <summary>
        /// Returns the culturally formatted string representing the price as seen by the user.
        /// <para>For example, a price of 150 would be seen by an Irish user as €1.50 and by a German user as €1,50.</para>
        /// </summary>
        /// <returns>The culturally formatted string representation of the price including its currency symbol.</returns>
        public readonly string CurrentPriceString()
        {
            System.Globalization.NumberFormatInfo cultureInfo = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.CurrentCulture.NumberFormat.Clone();
            cultureInfo.CurrencySymbol = CurrencySymbol;

            return ((double)CurrentPrice / 100).ToString("c", cultureInfo);
        }
        /// <summary>
        /// Returns the culturally formatted string representing the price as seen by the user.
        /// <para>For example, a price of 150 would be seen by an Irish user as €1.50 and by a German user as €1,50.</para>
        /// </summary>
        /// <returns>The culturally formatted string representation of the price including its currency symbol.</returns>
        public readonly string BasePriceString()
        {
            System.Globalization.NumberFormatInfo cultureInfo = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.CurrentCulture.NumberFormat.Clone();
            cultureInfo.CurrencySymbol = CurrencySymbol;

            return ((double)BasePrice / 100).ToString("c", cultureInfo);
        }
        /// <summary>
        /// Request the inventory item prices be loaded from the Steam backend.
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="SteamInventoryRequestPricesResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when the process completes.</param>
        public static void RequestPrices(Action<SteamInventoryRequestPricesResult_t, bool> callback) => API.Inventory.Client.RequestPrices(callback);
        /// <summary>
        /// Request all items be updated in local memory.
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes.</param>
        public static void Update(Action<InventoryResult> callback) => API.Inventory.Client.GetAllItems(callback);
        /// <summary>
        /// Gets the <see cref="ItemData"/> representing the indicated item.
        /// </summary>
        /// <param name="id">The item ID to get.</param>
        /// <returns>The <see cref="ItemData"/> result.</returns>
        public static ItemData Get(int id) => id;
        /// <summary>
        /// Get the <see cref="ItemData"/> based on the native <see cref="SteamItemDef_t"/>.
        /// </summary>
        /// <param name="id">The native <see cref="SteamItemDef_t"/> of the item you want to get.</param>
        /// <returns>The <see cref="ItemData"/> result.</returns>
        public static ItemData Get(SteamItemDef_t id) => id;
        /// <summary>
        /// Get the <see cref="ItemData"/> from the <see cref="ItemDefinitionSettings"/>.
        /// </summary>
        /// <param name="item">The object instance of the item you want to get.</param>
        /// <returns>The <see cref="ItemData"/> result.</returns>
        public static ItemData Get(ItemDefinitionSettings item) => item.id;

        #region Boilerplate
        /// <summary>
        /// Compares the current instance with another <see cref="ItemData"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ItemData"/> to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public readonly int CompareTo(ItemData other)
        {
            return id.CompareTo(other.id);
        }

        /// <summary>
        /// Compares the current instance with an integer representing an item ID.
        /// </summary>
        /// <param name="other">The item ID to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public readonly int CompareTo(int other)
        {
            return id.CompareTo(other);
        }

        /// <summary>
        /// Compares the current instance with a <see cref="SteamItemDef_t"/>.
        /// </summary>
        /// <param name="other">The <see cref="SteamItemDef_t"/> to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public readonly int CompareTo(SteamItemDef_t other)
        {
            return id.CompareTo(other.m_SteamItemDef);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another <see cref="ItemData"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ItemData"/> to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.</returns>
        public readonly bool Equals(ItemData other)
        {
            return id.Equals(other.id);
        }

        /// <summary>
        /// Indicates whether the current object is equal to an integer representing an item ID.
        /// </summary>
        /// <param name="other">The item ID to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.</returns>
        public readonly bool Equals(int other)
        {
            return id.Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to a <see cref="SteamItemDef_t"/>.
        /// </summary>
        /// <param name="other">The <see cref="SteamItemDef_t"/> to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.</returns>
        public readonly bool Equals(SteamItemDef_t other)
        {
            return id.Equals(other.m_SteamItemDef);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public readonly override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(ItemData l, ItemData r) => l.id == r.id;
        public static bool operator ==(ItemData l, int r) => l.id == r;
        public static bool operator ==(ItemData l, SteamItemDef_t r) => l.id == r.m_SteamItemDef;
        public static bool operator !=(ItemData l, ItemData r) => l.id != r.id;
        public static bool operator !=(ItemData l, int r) => l.id != r;
        public static bool operator !=(ItemData l, SteamItemDef_t r) => l.id != r.m_SteamItemDef;

        public static implicit operator int(ItemData c) => c.id;
        public static implicit operator ItemData(int id) => new ItemData { id = id };
        public static implicit operator SteamItemDef_t(ItemData c) => new SteamItemDef_t(c.id);
        public static implicit operator ItemData(SteamItemDef_t id) => new ItemData { id = id.m_SteamItemDef };
        #endregion
    }
}
#endif