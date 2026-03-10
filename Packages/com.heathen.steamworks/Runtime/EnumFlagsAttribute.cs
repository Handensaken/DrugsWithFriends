#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A custom attribute that enables the use of bitmask flags for enum fields in the Unity inspector.
    /// </summary>
    /// <remarks>
    /// This attribute is applied to an enum field to enable multi-selection of values in the Unity editor,
    /// visualising the enum as a flags-based selection instead of a single-option dropdown.
    /// </remarks>
    public class EnumFlagsAttribute : PropertyAttribute
    {
        /// <summary>
        /// A custom attribute that enables the use of bitmask flags for enum fields in the Unity inspector.
        /// </summary>
        /// <remarks>
        /// When applied to an enum field, this attribute allows Unity's editor to display a multi-selectable bitmask interface
        /// instead of the default single-option dropdown. Suitable for use with enums marked with the [Flags] attribute.
        /// </remarks>
        public EnumFlagsAttribute() { }
    }
}
#endif