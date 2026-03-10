#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents an order for consuming a specific quantity of items in the Steam Inventory system.
    /// </summary>
    /// <remarks>
    /// This struct is commonly used in inventory management processes where items are consumed
    /// (e.g. using up consumable items or crafting materials). Each order details the specific item to be consumed
    /// and the quantity requested.
    /// </remarks>
    public struct ConsumeOrder
    {
        /// <summary>
        /// Represents the details of a Steam inventory item in the context of a consumption order.
        /// </summary>
        /// <remarks>
        /// The <c>Detail</c> field is of type <c>Steamworks.SteamItemDetails_t</c>, which provides
        /// the necessary metadata and identifier for a Steam inventory item.
        /// This field is primarily used in operations related to inventory management,
        /// such as consuming or inspecting items.
        /// </remarks>
        public Steamworks.SteamItemDetails_t Detail;

        /// <summary>
        /// Represents the quantity of items to be consumed or manipulated in a Steam inventory operation.
        /// </summary>
        /// <remarks>
        /// The <c>Quantity</c> field specifies the number of items to process within the context
        /// of a consumption order or similar inventory operation. It is an unsigned 32-bit integer
        /// and should always represent a non-negative value.
        /// </remarks>
        public uint Quantity;
    }
}
#endif