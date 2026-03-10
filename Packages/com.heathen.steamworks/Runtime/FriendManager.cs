#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Manages interactions with the Steam Friends API, providing functionality
    /// for retrieving friend lists, messages, and presence updates.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks")]
    [DisallowMultipleComponent]
    public class FriendManager : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets a value indicating whether the system is actively listening for
        /// messages from friends within the Steamworks environment.
        /// </summary>
        /// <remarks>
        /// When enabled, the system will monitor and handle incoming chat messages
        /// from friends. Disabling this stops the monitoring process and may be useful
        /// in scenarios where message listening is not required to conserve resources
        /// or manage functionality.
        /// </remarks>
        public bool ListenForFriendsMessages
        {
            get => API.Friends.Client.IsListenForFriendsMessages;
            set => API.Friends.Client.IsListenForFriendsMessages = value;
        }

        /// <summary>
        /// Event triggered when a chat message is received from a friend
        /// while connected within the same game environment on Steam.
        /// </summary>
        /// <remarks>
        /// This event provides information about the sender, the content of the message,
        /// and the type of chat entry. It can be used to handle incoming messages
        /// from friends during online gameplay.
        /// </remarks>
        [FormerlySerializedAs("evtGameConnectedChatMsg")] public GameConnectedFriendChatMsgEvent onGameConnectedChatMsg;

        /// <summary>
        /// Represents the event that is triggered when a friend's rich presence is updated
        /// within the Steamworks environment.
        /// </summary>
        /// <remarks>
        /// This event can be used to monitor and respond to changes in the rich presence
        /// information of a friend. Rich presence typically includes context about the
        /// friend’s current game or activity, and is often displayed on friend lists or
        /// game overlays. Subscribing to this event allows you to handle these updates
        /// in your application or game.
        /// </remarks>
        [FormerlySerializedAs("evtRichPresenceUpdated")] public UnityEvent<UserData, AppData> onRichPresenceUpdated;

        /// <summary>
        /// Event invoked when a friend's persona state changes within the Steamworks environment.
        /// </summary>
        /// <remarks>
        /// This event provides notifications for changes in a friend's persona, such as their name,
        /// online status, or other presence-related updates. It is triggered by the Steam Friends API
        /// and can be used to dynamically update UI elements or other systems dependent on these changes.
        /// </remarks>
        [FormerlySerializedAs("evtPersonaStateChanged")] public UnityEvent<UserData, EPersonaChange> onPersonaStateChanged;

        private void OnEnable()
        {
            SteamTools.Events.OnGameConnectedFriendChatMsg += onGameConnectedChatMsg.Invoke;
            SteamTools.Events.OnFriendRichPresenceUpdate += onRichPresenceUpdated.Invoke;
            SteamTools.Events.OnPersonaStateChange += onPersonaStateChanged.Invoke;
        }

        private void OnDisable()
        {
            SteamTools.Events.OnGameConnectedFriendChatMsg -= onGameConnectedChatMsg.Invoke;
            SteamTools.Events.OnFriendRichPresenceUpdate -= onRichPresenceUpdated.Invoke;
            SteamTools.Events.OnPersonaStateChange -= onPersonaStateChanged.Invoke;
        }

        /// <summary>
        /// Retrieves the list of friends based on the specified friend flags.
        /// </summary>
        /// <param name="flags">The flags specifying the type of friends to retrieve, such as blocked users,
        /// immediate friends, or all users in the player's friends list.</param>
        /// <returns>An array of <c>UserData</c> objects representing the user's friends that match the specified flags.</returns>
        public UserData[] GetFriends(EFriendFlags flags) => API.Friends.Client.GetFriends(flags);

        /// <summary>
        /// Retrieves the list of users the local player has recently played with via Steam.
        /// </summary>
        /// <returns>An array of <c>UserData</c> objects representing the users the local player has co-played with.</returns>
        public UserData[] GetCoplayFriends() => API.Friends.Client.GetCoplayFriends();

        /// <summary>
        /// Retrieves a message from a friend's chat history based on the specified parameters.
        /// </summary>
        /// <param name="userId">The identifier of the friend from whom the message should be retrieved.</param>
        /// <param name="index">The zero-based index of the message to retrieve within the friend's chat history.</param>
        /// <param name="type">Outputs the type of the chat entry retrieved, such as text message, typing indicator, or game invite.</param>
        /// <returns>A string representing the message content retrieved from the friend's chat history.</returns>
        public string GetFriendMessage(UserData userId, int index, out EChatEntryType type) => API.Friends.Client.GetFriendMessage(userId, index, out type);

        /// <summary>
        /// Sends a message to a specified friend using the Steamworks API.
        /// </summary>
        /// <param name="friend">The <c>UserData</c> object representing the friend to whom the message will be sent.</param>
        /// <param name="message">The message text to send to the specified friend.</param>
        /// <returns><c>true</c> if the message was successfully sent; otherwise, <c>false</c>.</returns>
        public bool SendMessage(UserData friend, string message) => API.Friends.Client.ReplyToFriendMessage(friend, message);
    }
}
#endif