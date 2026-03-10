#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents an entry used in an inventory exchange process.
    /// </summary>
    /// <remarks>
    /// This structure is primarily used in integration with the Steamworks Inventory system and encapsulates
    /// information about a specific item instance and the quantity involved in a transaction or exchange.
    /// </remarks>
    public struct ExchangeEntry
    {
        /// <summary>
        /// Represents the unique identifier for a specific item instance within the inventory exchange process.
        /// </summary>
        /// <remarks>
        /// This field is used to reference the specific item instance involved in a transaction or exchange.
        /// It plays a crucial role in identifying and performing operations on the correct instance of an item
        /// when interacting with the Steamworks Inventory system.
        /// </remarks>
        public SteamItemInstanceID_t Instance;

        /// <summary>
        /// Represents the count or amount of a specific item involved in an inventory exchange process.
        /// </summary>
        /// <remarks>
        /// This field indicates the quantity of an item instance that is being transacted or exchanged
        /// in the Steamworks Inventory system. It is crucial for defining the scale of the operation
        /// associated with the inventory exchange entry.
        /// </remarks>
        public uint Quantity;
    }
}
#endif