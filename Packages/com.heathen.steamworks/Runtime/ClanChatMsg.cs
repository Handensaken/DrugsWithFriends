#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a chat message exchanged within a clan chat room in Steamworks integration.
    /// </summary>
    /// <remarks>
    /// This structure is used to encapsulate details regarding a single message sent or received
    /// in a clan chat. It includes information about the chat room, message type, content, and the user
    /// involved in the message.
    /// </remarks>
    [Serializable]
    public struct ClanChatMsg
    {
        /// <summary>
        /// Represents the chat room associated with this message.
        /// </summary>
        /// <remarks>
        /// The `room.id` is guaranteed to be populated. However, in certain scenarios,
        /// such as receiving a clan chat room message from an unknown room within
        /// the system, the `clan.id` may be invalid, and `room.enterResponse` could indicate failure.
        /// </remarks>
        public ChatRoom room;

        /// <summary>
        /// Represents the type of chat entry in a clan chat message.
        /// </summary>
        /// <remarks>
        /// This indicates the nature of the message or event in a clan chat,
        /// such as a standard chat message, a user typing notification,
        /// or a user being kicked from the conversation. The value is derived
        /// from the <see cref="Steamworks.EChatEntryType"/> enumeration.
        /// </remarks>
        public EChatEntryType type;

        /// <summary>
        /// Represents the content of a chat message sent within a clan chat room.
        /// </summary>
        /// <remarks>
        /// The `message` is the text or data sent by the user in the associated chat room.
        /// It may contain plain text or formatted content depending on the chat system's implementation.
        /// Ensure that `message` is appropriately validated or processed before display or use
        /// to avoid potential issues with untrusted input.
        /// </remarks>
        public string message;

        /// <summary>
        /// Represents the user associated with the clan chat message.
        /// </summary>
        /// <remarks>
        /// This field contains the user data of the individual who sent the chat message.
        /// The `user` object provides identity information, such as the user's unique Steam ID and validity status.
        /// It is used to trace or interact with the sender within the context of a clan chat.
        /// </remarks>
        public UserData user;
    }
}
#endif