#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides functionalities and utilities related to clans in the Heathen Steamworks Integration API.
    /// This class contains methods and events necessary to manage and interact with Steam clans,
    /// including chat room management, membership information, and activity tracking.
    /// </summary>
    public static class Clans
    {
        /// <summary>
        /// Provides functionalities to interact with and manage Steam clan-related chat rooms
        /// in the Heathen Steamworks Integration API. This class includes methods for joining
        /// and leaving chat rooms, retrieving chat messages, fetching member information, and
        /// managing activity and roles within clans.
        /// </summary>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                _downloadClanActivityCountsResultT = null;
                _clanOfficerListResponseT = null;
                _joinClanChatRoomCompletionResultT = null;
                JoinedRooms.Clear();
            }

            /// <summary>
            /// This is provided for debugging purposes and generally shouldn't be used
            /// </summary>
            /// <remarks>
            /// The JoinChatRoom callback provides the ClanChatRoom you just entered you should cache and use that as opposed to reading from this list.
            /// The chat-related events also send the ClanChatRoom meaning there is little reason to read the list of rooms save for debugging.
            /// </remarks>
            public static readonly List<ChatRoom> JoinedRooms = new();

            private static CallResult<DownloadClanActivityCountsResult_t> _downloadClanActivityCountsResultT;
            private static CallResult<ClanOfficerListResponse_t> _clanOfficerListResponseT;
            private static CallResult<JoinClanChatRoomCompletionResult_t> _joinClanChatRoomCompletionResultT;

            /// <summary>
            /// Allows the user to join Steam group (clan) chats right within the game.
            /// </summary>
            /// <remarks>
            /// The behaviour is somewhat complicated because the user may or may not be already in the group chat from outside the game or in the overlay. You can use ActivateGameOverlayToUser to open the in-game overlay version of the chat.
            /// </remarks>
            /// <param name="clan">The Steam group or clan the user wants to join the chat of.</param>
            /// <param name="callback">The callback to handle the result of the join operation, returning the chat room and success state.</param>
            public static void JoinChatRoom(ClanData clan, Action<ChatRoom, bool> callback)
            {
                if (callback == null)
                    return;

                _joinClanChatRoomCompletionResultT ??= CallResult<JoinClanChatRoomCompletionResult_t>.Create();

                var handle = SteamFriends.JoinClanChatRoom(clan);
                _joinClanChatRoomCompletionResultT.Set(handle, (r, e) =>
                {
                    if (!e)
                    {
                        var cc = new ChatRoom
                        {
                            clan = clan,
                            id = r.m_steamIDClanChat,
                            enterResponse = r.m_eChatRoomEnterResponse
                        };

                        if (r.m_eChatRoomEnterResponse == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                            JoinedRooms.Add(cc);

                        callback.Invoke(cc, false);
                    }
                    else
                        callback.Invoke(default, true);
                    
                });
            }

            /// <summary>
            /// Leaves a Steam group chat that the user has previously entered using JoinChatRoom.
            /// </summary>
            /// <remarks>
            /// This method removes the chat room from the internal list of joined rooms and notifies the Steamworks API to leave the specified clan chat room.
            /// </remarks>
            /// <param name="clanChatId">The unique identifier of the chat room or Steam group (clan) to leave. This can be a ChatRoom ID or a Clan ID.</param>
            /// <returns>Returns true if the operation to leave the chat room was successfully initiated, otherwise false.</returns>
            public static bool LeaveChatRoom(CSteamID clanChatId)
            {
                JoinedRooms.RemoveAll(p => p.id == clanChatId);

                return SteamFriends.LeaveClanChatRoom(clanChatId);
            }

            /// <summary>
            /// Leaves a Steam group chat that the user has previously joined.
            /// </summary>
            /// <remarks>
            /// This method removes the specified chat room from the list of joined rooms and
            /// signals the Steam API to leave the corresponding clan chat room.
            /// </remarks>
            /// <param name="clanChat">The chat room the user wishes to leave.</param>
            /// <returns>
            /// Returns true if the leave operation was successfully initiated, otherwise false.
            /// </returns>
            public static bool LeaveChatRoom(ChatRoom clanChat)
            {
                JoinedRooms.Remove(clanChat);

                return SteamFriends.LeaveClanChatRoom(clanChat.id);
            }

            /// <summary>
            /// Retrieves the user data of a chat member at a specified index within a Steam group chat.
            /// </summary>
            /// <param name="clan">The Steam group chat's unique identifier. This must match the group used in a prior call to GetClanChatMemberCount.</param>
            /// <param name="index">The zero-based index of the chat member, which must be less than the total number of chat members as returned by GetClanChatMemberCount.</param>
            /// <returns>
            /// Returns the user data for the chat member at the specified index.
            /// </returns>
            public static UserData GetChatMemberByIndex(CSteamID clan, int index) =>
                SteamFriends.GetChatMemberByIndex(clan, index);

            /// <summary>
            /// Retrieves the most recent activity statistics of members in a specified Steam group.
            /// </summary>
            /// <param name="clan">The Steam group to retrieve activity data for.</param>
            /// <param name="online">Outputs the count of group members currently online.</param>
            /// <param name="inGame">Outputs the count of group members currently in a game.</param>
            /// <param name="chatting">Outputs the count of group members currently in the group chat room.</param>
            /// <returns>Returns true if the activity counts were successfully retrieved; otherwise, false.</returns>
            public static bool GetActivityCounts(CSteamID clan, out int online, out int inGame, out int chatting) =>
                SteamFriends.GetClanActivityCounts(clan, out online, out inGame, out chatting);

            /// <summary>
            /// Retrieves the Steam group's data (ClanData) based on the specified index.
            /// </summary>
            /// <param name="clanIndex">The zero-based index representing the specific Steam group to retrieve.</param>
            /// <returns>The ClanData associated with the provided index.</returns>
            public static ClanData GetClanByIndex(int clanIndex) => SteamFriends.GetClanByIndex(clanIndex);

            /// <summary>
            /// Retrieves an array of clans that the current user is a member of.
            /// </summary>
            /// <returns>An array containing the clan data for all clans the user is associated with.</returns>
            public static ClanData[] GetClans()
            {
                var count = SteamFriends.GetClanCount();
                var results = new ClanData[count];
                for (int i = 0; i < count; i++)
                {
                    results[i] = SteamFriends.GetClanByIndex(i);
                }
                return results;
            }

            /// <summary>
            /// Retrieves the number of members currently present in the chat of a specified Steam group (clan).
            /// </summary>
            /// <remarks>
            /// This method retrieves the count of users in the chat of a specified Steam group.
            /// Note that the local user must be in the relevant chat or lobby to retrieve this information.
            /// Additionally, for large Steam groups, the local user may not be able to iterate through all members.
            /// If specific details about each member are required, this method should be followed by retrieving individual members using the GetChatMemberByIndex method.
            /// </remarks>
            /// <param name="clanId">The Steam group (clan) for which the chat member count is being retrieved.</param>
            /// <returns>The number of users currently present in the specified group's chat.</returns>
            public static int GetChatMemberCount(ClanData clanId) => SteamFriends.GetClanChatMemberCount(clanId);

            /// <summary>
            /// Retrieves a list of members currently in the chat of a specified Steam group.
            /// </summary>
            /// <remarks>
            /// This method retrieves the members of a Steam group chat. Note that the local user must be in the lobby to access the Steam IDs of other users in that specific lobby. After retrieving the member count, you can iterate through the members using other methods if needed.
            /// </remarks>
            /// <param name="clanId">The Steam group for which to retrieve the chat members.</param>
            /// <returns>An array of <see cref="UserData"/> objects representing the members in the chat.</returns>
            public static UserData[] GetChatMembers(ClanData clanId)
            {
                var count = SteamFriends.GetClanChatMemberCount(clanId);

                if (count > 0)
                {
                    var results = new UserData[count];
                    for (int i = 0; i < count; i++)
                    {
                        results[i] = SteamFriends.GetChatMemberByIndex(clanId, i);
                    }

                    return results;
                }
                else
                    return Array.Empty<UserData>();
            }

            /// <summary>
            /// Retrieves the content of a message from a Steam group chat room.
            /// </summary>
            /// <param name="clanChatId">The Steam ID of the group chat room where the message resides.</param>
            /// <param name="index">The index of the message within the chat room. Typically corresponds to the m_iMessageID field of the GameConnectedClanChatMsg_t structure.</param>
            /// <param name="type">Outputs the type of the chat entry, such as text messages or other event types.</param>
            /// <param name="chatter">Outputs the Steam ID of the user who sent the message.</param>
            /// <returns>The content of the chat message as a string. Returns an empty string if the message could not be retrieved.</returns>
            public static string GetChatMessage(CSteamID clanChatId, int index, out EChatEntryType type,
                out CSteamID chatter)
            {
                if (SteamFriends.GetClanChatMessage(clanChatId, index, out string result, 8193, out type, out chatter) >
                    0)
                {
                    return result;
                }
                else
                    return string.Empty;
            }

            /// <summary>
            /// Retrieves the content of a message from a Steam group chat room.
            /// </summary>
            /// <param name="clanChat">The Steam group chat room from which the message is to be retrieved.</param>
            /// <param name="index">The index of the message to retrieve. This should correspond to the m_iMessageID field of GameConnectedClanChatMsg_t.</param>
            /// <param name="type">Outputs the type of chat entry (e.g. message, notification) that was retrieved.</param>
            /// <param name="chatter">Outputs the Steam ID of the user who sent the message.</param>
            /// <returns>The content of the chat message as a string. Returns an empty string if the retrieval fails.</returns>
            public static string GetChatMessage(ChatRoom clanChat, int index, out EChatEntryType type,
                out CSteamID chatter)
            {
                if (SteamFriends.GetClanChatMessage(clanChat.id, index, out string result, 8193, out type,
                        out chatter) > 0)
                {
                    return result;
                }
                else
                    return string.Empty;
            }

            /// <summary>
            /// Retrieves the total number of Steam groups (clans) the current user is a member of.
            /// </summary>
            /// <returns>
            /// An integer representing the count of Steam groups that the current user belongs to.
            /// </returns>
            public static int GetClanCount() => SteamFriends.GetClanCount();

            /// <summary>
            /// Retrieves the display name of the specified Steam group if the group is known to the local client.
            /// </summary>
            /// <remarks>
            /// This method queries the Steam Friends API to return the name associated with the provided group.
            /// </remarks>
            /// <param name="clanId">The Steam group identifier to retrieve the name for.</param>
            /// <returns>The display name of the specified Steam group, or an empty string if the group is unknown.</returns>
            public static string GetName(ClanData clanId) => SteamFriends.GetClanName(clanId);

            /// <summary>
            /// Gets the Steam ID of the officer at the specified index within a Steam group.
            /// </summary>
            /// <remarks>
            /// Ensure that GetClanOfficerCount is called successfully before using this method to get the number of officers in the group.
            /// </remarks>
            /// <param name="clanId">The identifier of the Steam group from which the officer's information will be retrieved.</param>
            /// <param name="officerIndex">The index of the officer within the group. Must be less than the total number of officers.</param>
            /// <returns>The officer's Steam ID represented as a UserData object.</returns>
            public static UserData GetOfficerByIndex(ClanData clanId, int officerIndex) =>
                SteamFriends.GetClanOfficerByIndex(clanId, officerIndex);

            /// <summary>
            /// Retrieves the list of officers for the specified clan.
            /// </summary>
            /// <remarks>
            /// The method queries the Steamworks API to gather the officers of the provided clan.
            /// If the clan has no officers, an empty array is returned.
            /// </remarks>
            /// <param name="clanId">The Steam ID of the group to retrieve officer information from.</param>
            /// <returns>An array of <see cref="UserData"/> representing the officers of the specified clan.</returns>
            public static UserData[] GetOfficers(ClanData clanId)
            {
                SteamFriends.RequestClanOfficerList(clanId);

                var count = SteamFriends.GetClanOfficerCount(clanId);
                if (count > 0)
                {
                    var results = new UserData[count];
                    for (int i = 0; i < count; i++)
                    {
                        results[i] = SteamFriends.GetClanOfficerByIndex(clanId, i);
                    }

                    return results;
                }
                else
                    return Array.Empty<UserData>();
            }

            /// <summary>
            /// Gets the number of officers (administrators and moderators) in a specified Steam group, including the owner. This count can be used for iteration to retrieve each officer's Steam ID using GetClanOfficerByIndex.
            /// </summary>
            /// <remarks>
            /// You must call RequestClanOfficerList before using this method to ensure the officer data is available.
            /// </remarks>
            /// <param name="clanId">The Steam group to query for its officer count.</param>
            /// <returns>Returns the total number of officers in the specified Steam group.</returns>
            public static int GetOfficerCount(ClanData clanId) => SteamFriends.GetClanOfficerCount(clanId);

            /// <summary>
            /// Retrieves the owner of the specified Steam group (clan).
            /// </summary>
            /// <remarks>
            /// Ensure that you invoke the RequestClanOfficerList method beforehand to collect the required data needed by this method.
            /// </remarks>
            /// <param name="clanId">The identifier of the Steam group (clan) whose owner is being requested.</param>
            /// <returns>Returns a UserData object representing the owner of the specified clan.</returns>
            public static UserData GetOwner(ClanData clanId) => SteamFriends.GetClanOwner(clanId);

            /// <summary>
            /// Gets the unique tag (abbreviation) for the specified Steam group if it is known by the local client.
            /// The tag is a unique identifier for the group, limited to 12 characters.
            /// It may be displayed next to the name of group members in some games.
            /// </summary>
            /// <param name="clanId">The Steam group to retrieve the tag for.</param>
            /// <returns>The unique tag (abbreviation) of the specified Steam group.</returns>
            public static string GetTag(ClanData clanId) => SteamFriends.GetClanTag(clanId);

            /// <summary>
            /// Opens the specified Steam group chat room in the Steam UI.
            /// </summary>
            /// <param name="clanChatRoomId">The unique identifier of the Steam group chat room to be opened.</param>
            /// <returns>
            /// true if the user successfully entered the Steam group chat room; false in the following scenarios:
            /// The chat room does not exist, the user does not have access to join it,
            /// the user is rate-limited, or the user is chat restricted.
            /// </returns>
            public static bool OpenChatWindowInSteam(CSteamID clanChatRoomId) =>
                SteamFriends.OpenClanChatWindowInSteam(clanChatRoomId);

            /// <summary>
            /// Opens the specified Steam group chat room in the Steam UI.
            /// </summary>
            /// <remarks>
            /// This method attempts to open the Steam group chat room associated with the provided chat room object.
            /// The success of this operation depends on several factors, including user permissions, rate limits,
            /// and restrictions on the user's account.
            /// </remarks>
            /// <param name="clanChat">The chat room object representing the Steam group chat to open.</param>
            /// <returns>
            /// True if the user successfully entered the Steam group chat room; otherwise, false.
            /// The method returns false in the following cases:
            /// - The specified Steam group chat room does not exist, or the user lacks access permissions.
            /// - The user is currently subject to a rate limit imposed by Steam.
            /// - The user is restricted from using Steam chat.
            /// </returns>
            public static bool OpenChatWindowInSteam(ChatRoom clanChat) =>
                SteamFriends.OpenClanChatWindowInSteam(clanChat.id);

            /// <summary>
            /// Sends a message to a Steam group chat room.
            /// </summary>
            /// <param name="clanChatId">The unique identifier of the Steam group chat room to which the message is to be sent.</param>
            /// <param name="message">The content of the message to send. The maximum allowed length is 2048 characters.</param>
            /// <returns>
            /// True if the message was successfully sent; otherwise, false. A return value of false may indicate one of the following conditions:
            /// - The current user is not in the specified group chat.
            /// - The current user is not connected to Steam.
            /// - The current user is rate-limited.
            /// - The current user is chat restricted.
            /// - The message exceeds the allowed character limit.
            /// </returns>
            public static bool SendChatMessage(CSteamID clanChatId, string message) =>
                SteamFriends.SendClanChatMessage(clanChatId, message);

            /// <summary>
            /// Sends a message to a Steam group chat room.
            /// </summary>
            /// <remarks>
            /// This method allows the user to send a message to a specific Steam group chat room.
            /// Note that the operation may fail if the user is not properly connected to Steam,
            /// is not a member of the group chat, has exceeded rate limits, is chat restricted,
            /// or if the message violates the character limit.
            /// </remarks>
            /// <param name="clanChat">The chat room object representing the Steam group chat where the message will be sent.</param>
            /// <param name="message">The message content to be sent to the group chat. The message must not exceed 2048 characters.</param>
            /// <returns>
            /// Returns true if the message was successfully sent.
            /// Returns false if the operation fails due to reasons such as the user not being in the group chat, not being connected to Steam,
            /// being rate-limited, being chat restricted, or exceeding the character limit.
            /// </returns>
            public static bool SendChatMessage(ChatRoom clanChat, string message) =>
                SteamFriends.SendClanChatMessage(clanChat.id, message);

            /// <summary>
            /// Determines whether the specified user is an administrator in the given clan chat room.
            /// </summary>
            /// <param name="clanChatId">The unique identifier of the clan chat room.</param>
            /// <param name="userId">The unique identifier of the user to check for admin status.</param>
            /// <returns>True if the specified user is a clan chat administrator; otherwise, false.</returns>
            public static bool IsClanChatAdmin(CSteamID clanChatId, CSteamID userId) =>
                SteamFriends.IsClanChatAdmin(clanChatId, userId);

            /// <summary>
            /// Determines whether a specified user is an administrator in a given clan chat room.
            /// </summary>
            /// <param name="clanChat">The chat room associated with the clan.</param>
            /// <param name="userId">The Steam ID of the user to check for administrator privileges.</param>
            /// <returns>True if the user is an administrator of the clan chat room; otherwise, false.</returns>
            public static bool IsClanChatAdmin(ChatRoom clanChat, CSteamID userId) =>
                SteamFriends.IsClanChatAdmin(clanChat.id, userId);

            /// <summary>
            /// Determines whether the specified Steam group (clan) is publicly accessible.
            /// </summary>
            /// <param name="clanId">The identifier of the clan to check for public accessibility.</param>
            /// <returns>A boolean indicating whether the clan is public.</returns>
            public static bool IsClanPublic(ClanData clanId) => SteamFriends.IsClanPublic(clanId);

            /// <summary>
            /// Checks if the specified Steam group is an official game group or community hub.
            /// </summary>
            /// <param name="clanId">The identifier of the Steam group to check.</param>
            /// <returns>True if the specified Steam group is an official game group or community hub; otherwise, false.</returns>
            public static bool IsClanOfficialGameGroup(ClanData clanId) => SteamFriends.IsClanOfficialGameGroup(clanId);

            /// <summary>
            /// Determines whether the specified Steam group chat room is currently open in the Steam UI.
            /// </summary>
            /// <param name="clanChatId">The Steam group chat room identifier (CSteamID) to check.</param>
            /// <returns>Returns true if the specified chat room is open in the Steam UI, otherwise false.</returns>
            public static bool IsClanChatWindowOpenInSteam(CSteamID clanChatId) =>
                SteamFriends.IsClanChatWindowOpenInSteam(clanChatId);

            /// <summary>
            /// Requests information about the officers (administrators and moderators) of a specified Steam group (clan).
            /// </summary>
            /// <remarks>
            /// NOTE: You can only request information about Steam groups that the user is a member of.
            /// Additionally, this method does not automatically download avatar images for the officers.
            /// If an avatar image for an officer is unavailable, use the RequestUserInformation method to download it.
            /// </remarks>
            /// <param name="clanId">The unique identifier of the Steam group (clan) whose officer list is being requested.</param>
            /// <param name="callback">The callback to handle the result of the request, providing the officer list and a success state.</param>
            public static void RequestClanOfficerList(CSteamID clanId, Action<ClanOfficerListResponse_t, bool> callback)
            {
                if (callback == null)
                    return;

                _clanOfficerListResponseT ??= CallResult<ClanOfficerListResponse_t>.Create();

                var handle = SteamFriends.RequestClanOfficerList(clanId);
                _clanOfficerListResponseT.Set(handle, callback.Invoke);
            }

            /// <summary>
            /// Closes the specified Steam group chat room in the Steam UI.
            /// </summary>
            /// <param name="clanChatId">The Steam ID of the Steam group chat room to close.</param>
            /// <returns>True if the chat window was successfully closed; otherwise, false.</returns>
            public static bool CloseClanChatWindowInSteam(CSteamID clanChatId) =>
                SteamFriends.CloseClanChatWindowInSteam(clanChatId);

            /// <summary>
            /// Closes the specified Steam group chat room in the Steam UI.
            /// </summary>
            /// <param name="clanChat">The Steam group chat room to close, specified by its ChatRoom identifier.</param>
            /// <returns>
            /// Returns true if the chat room was successfully closed, otherwise false.
            /// </returns>
            public static bool CloseClanChatWindowInSteam(ChatRoom clanChat) =>
                SteamFriends.CloseClanChatWindowInSteam(clanChat.id);

            /// <summary>
            /// Retrieves and updates the activity counts for a specified list of Steam groups.
            /// </summary>
            /// <remarks>
            /// This method can be used to refresh activity data for Steam groups, including those the user is not a member of. The result of the operation is provided via a callback.
            /// </remarks>
            /// <param name="clans">The array of Steam groups for which activity data should be updated.</param>
            /// <param name="callback">The action to handle the result of the request, receiving the activity data and success status from the Steam API.</param>
            public static void DownloadClanActivityCounts(CSteamID[] clans,
                Action<DownloadClanActivityCountsResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _downloadClanActivityCountsResultT ??= CallResult<DownloadClanActivityCountsResult_t>.Create();

                var handle = SteamFriends.DownloadClanActivityCounts(clans, clans.Length);
                _downloadClanActivityCountsResultT.Set(handle, callback.Invoke);
            }
        }
    }
}
#endif