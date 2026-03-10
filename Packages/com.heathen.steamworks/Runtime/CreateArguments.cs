#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents arguments used to create a Steamworks lobby.
    /// This includes settings such as the lobby's usage type, name, slot count, privacy type, metadata,
    /// and rich presence fields. These options control how the lobby is created and what metadata
    /// or presence information is set at creation.
    /// </summary>
    [Serializable]
    public class CreateArguments
    {
        /// <summary>
        /// Defines how the lobby will be utilized. This is an optional property.
        /// When set to specific modes such as Group or Session, features of the LobbyData object
        /// can be leveraged to query or fetch the created lobby, enabling functions such as LobbyData.GetGroup(...).
        /// </summary>
        [UnityEngine.Tooltip(
            "How will this lobby be used? This is an optional feature. If set to Group or Session then features of the LobbyData object can be used in code to fetch the created lobby such as LobbyData.GetGroup(...)")]
        public SteamLobbyModeType usageHint = SteamLobbyModeType.Session;

        /// <summary>
        /// Represents the name to assign to the lobby during its creation.
        /// This property helps identify the lobby and may be displayed to users
        /// or used in conjunction with other lobby features for organizational purposes.
        /// </summary>
        [UnityEngine.Tooltip("The name to assign to the lobby when it is created")]
        public string name;

        /// <summary>
        /// Specifies the maximum number of participants the newly created lobby can accommodate.
        /// This value determines the size of the lobby and enforces a limit on how many members can join.
        /// </summary>
        [UnityEngine.Tooltip("The number of slots the newly created lobby should have")]
        public int slots;

        /// <summary>
        /// Represents the type of the lobby to be created.
        /// This property determines the access level and visibility of the lobby,
        /// allowing the lobby to be configured as Private, Friends-Only, Public, Invisible, or Private-Unique.
        /// </summary>
        [UnityEngine.Tooltip("The type of lobby to create")]
        public ELobbyType type;

        /// <summary>
        /// Represents a collection of metadata to be associated with the lobby upon creation.
        /// Each metadata entry is defined as a <c>MetadataTemplate</c> and stored in a list.
        /// These metadata fields are not duplicated and can be used to store additional information
        /// about the lobby, such as custom tags or settings, that may be relevant for querying or managing lobbies.
        /// </summary>
        [UnityEngine.Tooltip(
            "The metadata to add to the lobby after creation. This is a dictionary and fields will not be repeated")]
        public List<MetadataTemplate> metadata = new List<MetadataTemplate>();

        /// <summary>
        /// Defines the Rich Presence fields to be set when a lobby is created.
        /// This allows additional metadata to be associated with the lobby,
        /// enhancing its visibility and discoverability in Rich Presence-enabled systems.
        /// </summary>
        [UnityEngine.Tooltip("The Rich Presence fields to be set when a lobby is created.")]
        public List<StringKeyValuePair> richPresenceFields = new List<StringKeyValuePair>();
    }
}
#endif