#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Provides functionality to manage the chat of a specific clan or group within the Steamworks environment.
    /// This component is designed to be attached to the Clan/Group chat user interface in Unity and facilitates interaction
    /// with the Steamworks API for managing clan chat interactions.
    /// </summary>
    /// <remarks>
    /// After attaching this component to the Clan/Group chat UI, you must call the Join method to connect it to a specific clan's chat.
    /// Once connected, this will handle sending and receiving messages for the chat. Additionally, various events can be utilized for
    /// monitoring activities within the chat, such as when a user joins, receives a message, or leaves the chat.
    /// For smaller clans or groups, you can access member data directly via properties like `Members`, though larger clans may
    /// have limitations on retrieving member details.
    /// </remarks>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/clan-chat-director")]
    public class ClanChatDirector : MonoBehaviour
    {
        [Header("Events")] public GameConnectedChatJoinEvent evtJoin;
        [FormerlySerializedAs("evtReceived")] 
        public UnityEvent<ChatRoom, UserData, string, Steamworks.EChatEntryType> onReceived;
        [FormerlySerializedAs("onLeave")] 
        public UnityEvent<ChatRoom, UserData, bool, bool> onLeave;

        /// <summary>
        /// A collection of user data representing the members currently in the chat room.
        /// If the user is not in a chat room, this will return an empty array.
        /// </summary>
        public UserData[] Members => _chatRoom != null && InRoom ? _chatRoom.Value.Members : Array.Empty<UserData>();

        /// <summary>
        /// Indicates whether the associated clan chat room is currently open in the Steam client.
        /// Returns true if the chat room is open in Steam, and the user is in the chat room;
        /// otherwise, returns false.
        /// </summary>
        public bool IsOpenInSteam => InRoom && _chatRoom is { IsOpenInSteam: true };

        /// <summary>
        /// Indicates whether the user is currently in a chat room.
        /// Returns true if the user successfully entered a chat room, otherwise false.
        /// </summary>
        public bool InRoom => _chatRoom is { enterResponse: EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess };

        /// <summary>
        /// Represents a chat room associated with a clan or group, providing access to its state and functionality.
        /// This property retrieves the current chat room being managed by the ClanChatDirector.
        /// If no chat room is currently associated, it will return a default value.
        /// </summary>
        public ChatRoom ChatRoom => _chatRoom ?? default;

        private ChatRoom? _chatRoom = null;

        private void OnEnable()
        {
            SteamTools.Events.OnGameConnectedClanChatMsg += HandleNewMessage;
            SteamTools.Events.OnGameConnectedChatJoin += HandleJoined;
            SteamTools.Events.OnGameConnectedChatLeave += HandleLeave;
        }

        private void OnDisable()
        {
            SteamTools.Events.OnGameConnectedClanChatMsg -= HandleNewMessage;
            SteamTools.Events.OnGameConnectedChatJoin -= HandleJoined;
            SteamTools.Events.OnGameConnectedChatLeave -= HandleLeave;
        }

        /// <summary>
        /// Attempts to join the chat room of the specified clan. This method interacts with the Steamworks API
        /// to establish a connection to the clan chat and assigns the resulting chat room instance upon success.
        /// </summary>
        /// <param name="clan">
        /// The clan data representing the target clan whose chat room should be joined.
        /// </param>
        public void Join(ClanData clan)
        {
            API.Clans.Client.JoinChatRoom(clan, (result, error) =>
            {
                if (!error)
                    _chatRoom = result;
                else
                    Debug.LogWarning("Steam client responded with an IO error when attempting to join Clan chat for " + clan.ToString());
            });
        }

        /// <summary>
        /// Leaves the current clan chat room if the user is currently connected.
        /// This method interacts with the Steamworks API to terminate the connection
        /// to the chat room and resets the chat room instance.
        /// </summary>
        public void Leave()
        {
            if (InRoom)
            {
                _chatRoom?.Leave();
                _chatRoom = null;
            }
        }

        /// <summary>
        /// Sends a message to the currently connected clan chat room. This method interacts with the Steamworks API
        /// to transmit the specified message to all members of the chat room.
        /// </summary>
        /// <param name="message">
        /// The text of the message to be sent to the chat room. This must be a non-null, non-empty string.
        /// </param>
        public void Send(string message)
        {
            if (InRoom)
            {
                _chatRoom?.SendMessage(message);
            }
        }

        /// <summary>
        /// Opens the Steam chat window for the current chat room. This method allows users
        /// to view and interact with the chat room directly through the Steam client application.
        /// </summary>
        /// <remarks>
        /// The chat room must be successfully joined and connected before using this method.
        /// If the chat room is not currently active or accessible, this method will not perform any action.
        /// </remarks>
        public void OpenInSteam()
        {
            if (InRoom)
            {
                _chatRoom?.OpenChatWindowInSteam();
            }
        }

        private void HandleLeave(ChatRoom room, UserData user, bool wasKicked, bool wasDropped)
        {
            if (InRoom && _chatRoom != null && room.id == _chatRoom.Value.id)
                onLeave.Invoke(room, user, wasKicked, wasDropped);
        }

        private void HandleJoined(ChatRoom arg0, UserData arg1)
        {
            if (InRoom && _chatRoom != null && arg0.id == _chatRoom.Value.id)
                evtJoin.Invoke(arg0, arg1);
        }

        private void HandleNewMessage(ChatRoom room, UserData user, string message, Steamworks.EChatEntryType type)
        {
            if (InRoom && _chatRoom != null && room.id == _chatRoom.Value.id)
                onReceived.Invoke(room, user, message, type);
        }
    }
}
#endif