#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the data for a friend's rich presence update event in the Steamworks integration.
    /// </summary>
    /// <remarks>
    /// This struct is used to encapsulate information about a friend, including their Steam ID and
    /// the associated application ID for their current rich presence data.
    /// </remarks>
    [Serializable]
    public struct FriendRichPresenceUpdate
    {
        /// <summary>
        /// Encapsulates the data for a friend's rich presence update event.
        /// </summary>
        /// <remarks>
        /// This variable stores the native representation of the friend's rich presence update,
        /// which includes information such as the friend's Steam ID and the application ID
        /// associated with their current rich presence.
        /// </remarks>
        public FriendRichPresenceUpdate_t Data;

        /// <summary>
        /// Represents the friend associated with a rich presence update event.
        /// </summary>
        /// <remarks>
        /// This property provides the <c>UserData</c> object representing the friend whose rich presence data has been updated.
        /// It allows access to the friend's Steam ID and serves as a key for retrieving specific details about the friend within the context of the Steamworks Integration framework.
        /// </remarks>
        public readonly UserData Friend => Data.m_steamIDFriend;

        /// <summary>
        /// Represents the application ID associated with the friend's current rich presence update.
        /// </summary>
        /// <remarks>
        /// This property provides an identifier for the application linked to the friend's activity
        /// within their rich presence data.
        /// </remarks>
        public readonly AppData App => Data.m_nAppID;

        public static implicit operator FriendRichPresenceUpdate(FriendRichPresenceUpdate_t native) => new() { Data = native };
        public static implicit operator FriendRichPresenceUpdate_t(FriendRichPresenceUpdate heathen) => heathen.Data;
    }
}
#endif