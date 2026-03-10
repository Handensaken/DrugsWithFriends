#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Friends = Heathen.SteamworksIntegration.API.Friends;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// This control component is focused on emulating Steam's Friend List functionality.
    /// It organises and displays the local player's friends into categories such as Playing, Online, Offline,
    /// and custom user-defined groups, similar to the layout in the Steam Client's Friend List.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/friend-groups-display")]
    public class FriendGroupsDisplay : MonoBehaviour
    {
        /// <summary>
        /// A serialized field that holds a reference to the transform container for displaying
        /// the list of friends who are currently playing the same game as the local player.
        /// This container is managed and populated dynamically by the FriendGroupsDisplay component
        /// to visually organise friends into an "In Game" category.
        /// </summary>
        [SerializeField] private Transform inGameCollection;

        /// <summary>
        /// A serialized field that holds a reference to the transform container for displaying
        /// the list of friends who are currently playing games other than the one the local player is currently in.
        /// This container is dynamically managed by the FriendGroupsDisplay component to organize friends
        /// into an "Other Games" category.
        /// </summary>
        [SerializeField] private Transform inOtherGameCollection;

        /// <summary>
        /// A serialized field that holds a reference to the transform container used for displaying
        /// friends grouped into custom user-defined categories. This container is dynamically populated
        /// and managed by the FriendGroupsDisplay component to visually represent friend groups
        /// beyond the standard categories of Playing, Online, and Offline.
        /// </summary>
        [SerializeField]
        private Transform groupedCollection;

        /// <summary>
        /// A serialized field that holds a reference to the transform container used for displaying
        /// the list of friends who are currently online. This container is dynamically populated
        /// by the FriendGroupsDisplay component to visually categorize friends into an "Online" group
        /// based on their Steam presence status.
        /// </summary>
        [SerializeField]
        private Transform onlineCollection;

        /// <summary>
        /// A serialized field that holds a reference to the transform container for displaying
        /// the list of friends who are currently offline. This container is dynamically managed
        /// by the FriendGroupsDisplay component to organize and visually represent an "Offline" category
        /// within the friend groups display.
        /// </summary>
        [SerializeField]
        private Transform offlineCollection;

        /// <summary>
        /// A serialized field that references a prefab used as the visual and functional template
        /// for dynamically creating group UI elements within the FriendGroupsDisplay component.
        /// Each instantiated group represents a category of the local player's friends, such as
        /// "Online", "Offline", or custom-defined groups, and is populated with relevant friend data.
        /// </summary>
        [SerializeField]
        private GameObject groupPrefab;

        private void OnEnable()
        {
            SteamTools.Interface.WhenReady(UpdateDisplay);
        }

        private void OnDisable()
        {
            Clear();
        }

        /// <summary>
        /// Clears all the collections by removing and destroying all child objects from
        /// in-game, grouped, online, offline, and other game collections.
        /// </summary>
        public void Clear()
        {
            if (inGameCollection != null
                && inGameCollection.childCount > 0)
            {
                foreach (Transform tran in inGameCollection)
                {
                    Destroy(tran.gameObject);
                }
            }

            if (groupedCollection != null
                && groupedCollection.childCount > 0)
            {
                foreach (Transform tran in groupedCollection)
                {
                    Destroy(tran.gameObject);
                }
            }

            if (onlineCollection != null
                && onlineCollection.childCount > 0)
            {
                foreach (Transform tran in onlineCollection)
                {
                    Destroy(tran.gameObject);
                }
            }

            if (offlineCollection != null
                && offlineCollection.childCount > 0)
            {
                foreach (Transform tran in offlineCollection)
                {
                    Destroy(tran.gameObject);
                }
            }

            if (inOtherGameCollection != null
                && inOtherGameCollection.childCount > 0)
            {
                foreach (Transform tran in inOtherGameCollection)
                {
                    Destroy(tran.gameObject);
                }
            }
        }

        /// <summary>
        /// Updates the display by clearing existing groups, categorizing friends into groups such as
        /// "Online," "In Game," "Other Games," and "Offline," and generating the corresponding UI components
        /// for each group. Custom groups, if defined, are also processed and displayed.
        /// </summary>
        public void UpdateDisplay()
        {
            Clear();

            List<UserData> online = new();
            List<UserData> inGame = new();
            List<UserData> inOtherGame = new();
            List<UserData> offline = new();
            Dictionary<string, List<UserData>> customGroups = new();

            var friends = Friends.Client.GetFriends(EFriendFlags.k_EFriendFlagImmediate);
            var groups = Friends.Client.GetFriendsGroups();

            foreach (var group in groups)
            {
                var groupName = Friends.Client.GetFriendsGroupName(group);
                if (!customGroups.ContainsKey(groupName))
                    customGroups.Add(groupName, new List<UserData>());

                var list = customGroups[groupName];

                foreach (var user in Friends.Client.GetFriendsGroupMembersList(group))
                {
                    if (user != UserData.Me && !list.Contains(user))
                        list.Add(user);
                }
            }

            foreach (var user in friends)
            {
                if (user == UserData.Me)
                    continue;

                if (user.GetGamePlayed(out FriendGameInfo gameInfo))
                {
                    online.Add(user);
                    if (gameInfo.Game.IsMe)
                    {
                        //In this game
                        inGame.Add(user);
                    }
                    else
                    {
                        //In other game
                        inOtherGame.Add(user);
                    }
                }
                else if (user.State != EPersonaState.k_EPersonaStateOffline
                    && user.State != EPersonaState.k_EPersonaStateInvisible)
                {
                    //On line in some form
                    online.Add(user);
                }
                else
                {
                    //Off line or hidden
                    offline.Add(user);
                }
            }

            if (onlineCollection != null)
            {
                onlineCollection.gameObject.SetActive(true);
                var onlineGo = Instantiate(groupPrefab, onlineCollection);
                var onlineComp = onlineGo.GetComponent<FriendGroup>();
                onlineComp.InitializeOnline("Online", online, true);
            }

            if (offlineCollection != null)
            {
                var offlineGo = Instantiate(groupPrefab, offlineCollection);
                var offlineComp = offlineGo.GetComponent<FriendGroup>();
                offlineComp.InitializeOffline("Offline", offline, false);
            }

            if (inGameCollection != null)
            {
                var inGameGo = Instantiate(groupPrefab, inGameCollection);
                var inGameComp = inGameGo.GetComponent<FriendGroup>();
                inGameComp.InitializeInGame("In Game", inGame, true);
            }

            if (inOtherGameCollection != null)
            {
                var otherGo = Instantiate(groupPrefab, inOtherGameCollection);
                var otherComp = otherGo.GetComponent<FriendGroup>();
                otherComp.InitializeInOther("Other Games", inOtherGame, true);
            }

            if (customGroups.Count > 0)
            {
                foreach (var kvp in customGroups)
                {
                    var kvpGo = Instantiate(groupPrefab, groupedCollection);
                    var kvpComp = kvpGo.GetComponent<FriendGroup>();
                    kvpComp.InitializeCustom(kvp.Key, kvp.Value, true);
                }
            }
            else
            {
                groupedCollection.gameObject.SetActive(false);
            }
        }
    }
}
#endif