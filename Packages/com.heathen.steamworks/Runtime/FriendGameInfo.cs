#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents information about the game a Steam friend is currently playing.
    /// </summary>
    /// <remarks>
    /// This struct aggregates various pieces of data regarding a friend's current game session,
    /// including the game ID, network connection details, and the associated lobby (if applicable).
    /// </remarks>
    [Serializable]
    public struct FriendGameInfo
    {
        /// <summary>
        /// Contains raw game session data for a Steam friend, represented as a <c>FriendGameInfo_t</c> structure.
        /// </summary>
        /// <remarks>
        /// Provides low-level information such as the game's Steam ID, IP address, ports, and lobby ID.
        /// This field facilitates interaction with Steamworks for querying and parsing game-related
        /// information about a Steam friend currently in-game.
        /// </remarks>
        public FriendGameInfo_t Data;

        /// <summary>
        /// Retrieves the game information associated with a specific friend, represented as a <c>GameData</c> structure.
        /// </summary>
        /// <remarks>
        /// This property provides access to the game that the friend is currently playing. It is derived
        /// from the underlying <c>FriendGameInfo_t</c> structure and can be used to fetch additional details
        /// about the game, such as its App ID and other contextual metadata.
        /// </remarks>
        public readonly GameData Game => Data.m_gameID;

        /// <summary>
        /// Retrieves the IP address of the game server associated with a Steam friend, represented as a formatted string.
        /// </summary>
        /// <remarks>
        /// This property converts the raw IP address stored as an unsigned integer into a human-readable string format.
        /// It is useful when displaying or using the game server's IP address for networking and connection purposes.
        /// </remarks>
        public readonly string IpAddress => API.Utilities.IPUintToString(Data.m_unGameIP);

        /// <summary>
        /// Represents the raw IP address for the game session a Steam friend is currently participating in, stored as an unsigned integer.
        /// </summary>
        /// <remarks>
        /// The <c>IpInt</c> property provides the IP address in its original 32-bit unsigned integer format as retrieved from Steamworks.
        /// This value can be used for low-level networking or converted to a readable IP string using utility functions.
        /// </remarks>
        public readonly uint IpInt => Data.m_unGameIP;

        /// <summary>
        /// Represents the network port number used for the game session that a Steam friend is currently playing.
        /// </summary>
        /// <remarks>
        /// This property retrieves the game port from the underlying <c>FriendGameInfo_t</c> structure.
        /// The game port is used for establishing direct network connections associated with the game session.
        /// </remarks>
        public readonly ushort GamePort => Data.m_usGamePort;

        /// <summary>
        /// Represents the query port used to retrieve additional information about a game server associated with a Steam friend's current game session.
        /// </summary>
        /// <remarks>
        /// This property provides the port number needed for querying detailed server information,
        /// such as player lists, server rules, and other metadata. It is typically used in network communication
        /// with game servers to facilitate server-side queries.
        /// </remarks>
        public readonly ushort QueryPort => Data.m_usQueryPort;

        /// <summary>
        /// Represents the lobby associated with a Steam friend's current game session.
        /// </summary>
        /// <remarks>
        /// Provides access to the Steam lobby ID tied to the game session, allowing further interactions
        /// such as querying lobby details, retrieving members, or managing lobby-related configurations.
        /// </remarks>
        public readonly LobbyData Lobby => Data.m_steamIDLobby;

        public static implicit operator FriendGameInfo(FriendGameInfo_t native) => new() { Data = native };
        public static implicit operator FriendGameInfo_t(FriendGameInfo heathen) => heathen.Data;
    }
}
#endif