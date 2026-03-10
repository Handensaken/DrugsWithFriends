#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a favorite game entry retrieved from the Steamworks matchmaking API.
    /// </summary>
    [Serializable]
    public struct FavoriteGame
    {
        /// <summary>
        /// Represents the unique identifier of a Steam application or game.
        /// This identifier is used within the Steamworks API to reference specific applications.
        /// </summary>
        public AppId_t appId;

        /// <summary>
        /// Represents the IP address of a favorite game server.
        /// The value is stored as an uint internally and converted to a string
        /// for external usage using the provided utilities.
        /// </summary>
        public string IpAddress
        {
            get => API.Utilities.IPUintToString(ipAddress);
            set => ipAddress = API.Utilities.IPStringToUint(value);
        }

        /// <summary>
        /// Stores the IP address of a server associated with a favorite game in unsigned integer format.
        /// The value can be converted to a string representation using relevant utility methods.
        /// </summary>
        public uint ipAddress;

        /// <summary>
        /// Represents the connection port associated with a favorite game server.
        /// The client uses this port to establish a connection to the server during matchmaking.
        /// </summary>
        public ushort connectionPort;

        /// <summary>
        /// Represents the query port of a server for a favorite game entry.
        /// This port is used by the Steamworks API to query server information.
        /// </summary>
        public ushort queryPort;

        /// <summary>
        /// Represents the date and time when the game was last played on the associated server.
        /// This value is retrieved from the Steamworks matchmaking API and is based on Unix time converted to a <see cref="DateTime"/> value.
        /// </summary>
        public DateTime LastPlayedOnServer;

        /// <summary>
        /// Indicates whether the favorite game is marked as a historical entry in the Steamworks matchmaking API.
        /// This flag is set based on the game's status within the user's favorite games list.
        /// </summary>
        public bool isHistory;
    }
}
#endif