#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam group chat room and provides methods for interacting with it.
    /// </summary>
    [Serializable]
    public struct ChatRoom : IEquatable<ChatRoom>
    {
        /// <summary>
        /// Represents the associated Steam group (clan) for this chat room.
        /// Provides contextual information about the group, such as its Steam ID and associated properties.
        /// </summary>
        public ClanData clan;

        /// <summary>
        /// Represents the unique identifier for the chat room.
        /// This identifier is used for various operations related to the chat room,
        /// such as sending messages, opening the chat window in Steam, leaving the chat, and more.
        /// </summary>
        public CSteamID id;

        /// <summary>
        /// Specifies the response status when attempting to enter a chat room.
        /// Indicates the success or failure of the operation and provides details
        /// on the reason for failure, if applicable. The response is based on the
        /// <see cref="EChatRoomEnterResponse"/> enumeration.
        /// </summary>
        public EChatRoomEnterResponse enterResponse;

        /// <summary>
        /// Gets the list of members currently present in the chat room.
        /// The data is retrieved from the associated Steam group (clan) using the provided API method.
        /// </summary>
        public readonly UserData[] Members => API.Clans.Client.GetChatMembers(clan);

        /// <summary>
        /// Checks if the Steam Group chat room is open in the Steam UI.
        /// </summary>
        public readonly bool IsOpenInSteam => API.Clans.Client.IsClanChatWindowOpenInSteam(id);

        /// <summary>
        /// Sends a message to a Steam group chat room.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(string message) => SteamFriends.SendClanChatMessage(id, message);

        /// <summary>
        /// Opens this chat in the Steam Overlay
        /// </summary>
        /// <returns></returns>
        public bool OpenChatWindowInSteam() => API.Clans.Client.OpenChatWindowInSteam(id);

        /// <summary>
        /// Leaves the associated Steam group chat room.
        /// </summary>
        public void Leave() => API.Clans.Client.LeaveChatRoom(id);

        #region Boilerplate

        /// <summary>
        /// Determines whether the current chat room instance is equal to another chat room instance.
        /// </summary>
        /// <param name="other">The other chat room to compare with the current instance.</param>
        /// <returns>True if the current instance is equal to the other chat room; otherwise, false.</returns>
        public bool Equals(ChatRoom other)
        {
            return clan == other.clan && id == other.id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current chat room instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="ChatRoom"/> instance.</param>
        /// <returns>
        /// <c>true</c> if the specified object is a <see cref="ChatRoom"/> and is equal to the current instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is ChatRoom other)
            {
                return Equals(other);
            }
            else
                return base.Equals(obj);
        }

        /// <summary>
        /// Serves as the default hash function for the ChatRoom struct.
        /// </summary>
        /// <returns>An integer that represents the hash code for the current ChatRoom instance.</returns>
        public override int GetHashCode() => clan.GetHashCode() ^ id.GetHashCode();

        /// <summary>
        /// Defines a custom operator for use within the class, providing functionality for specific operations.
        /// </summary>
        /// <param name="left">The left-hand operand of the operator.</param>
        /// <param name="right">The right-hand operand of the operator.</param>
        /// <returns>The result of the operation defined by the operator.</returns>
        public static bool operator ==(ChatRoom left, ChatRoom right) => left.Equals(right);

        /// <summary>
        /// Defines the implementation of a custom operator for the class or struct.
        /// </summary>
        /// <param name="left">The left operand of the operator.</param>
        /// <param name="right">The right operand of the operator.</param>
        /// <returns>The result of the operation defined by the operator.</returns>
        public static bool operator !=(ChatRoom left, ChatRoom right) => !left.Equals(right);

        #endregion
    }
}
#endif