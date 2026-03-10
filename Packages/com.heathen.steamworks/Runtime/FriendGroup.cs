#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Friends = Heathen.SteamworksIntegration.API.Friends;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Represents a group of friends categorized by their current status, such as online, offline,
    /// in-game, or other custom categories.
    /// Used in conjunction with the <see cref="FriendGroupsDisplay"/> to manage and display friend groups.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/friend-group")]
    public class FriendGroup : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI label;
        [SerializeField]
        private TMPro.TextMeshProUGUI counter;
        [SerializeField]
        private Toggle toggle;
        [SerializeField]
        private GameObject recordTemplate;
        [SerializeField]
        private Transform content;
        private enum GroupType
        {
            None,
            Online,
            Offline,
            InGame,
            OtherGame,
        }
        private readonly Dictionary<UserData, GameObject> _records = new();
        private GroupType _type = GroupType.None;

        private void OnEnable()
        {
            SteamTools.Events.OnPersonaStateChange += HandleStateChange;
        }

        private void OnDisable()
        {
            SteamTools.Events.OnPersonaStateChange -= HandleStateChange;
        }

        private void HandleStateChange(UserData user, EPersonaChange change)
        {
            if (!user.IsMe)
            {
                switch (_type)
                {
                    case GroupType.Online:
                        if (user.State == EPersonaState.k_EPersonaStateOffline
                            || user.State == EPersonaState.k_EPersonaStateInvisible)
                            Remove(user);
                        else
                            Add(user);
                        break;
                    case GroupType.Offline:
                        if (user.State != EPersonaState.k_EPersonaStateOffline
                            && user.State != EPersonaState.k_EPersonaStateInvisible)
                            Remove(user);
                        else
                            Add(user);
                        break;
                    case GroupType.InGame:
                        if (user.GetGamePlayed(out FriendGameInfo thisGameCheck) && thisGameCheck.Game.App == SteamUtils.GetAppID())
                            Add(user);
                        else
                            Remove(user);
                        break;
                    case GroupType.OtherGame:
                        if (user.GetGamePlayed(out FriendGameInfo otherGameCheck) && otherGameCheck.Game.App != SteamUtils.GetAppID())
                            Add(user);
                        else
                            Remove(user);
                        break;
                }
            }
        }

        private void Remove(UserData user)
        {
            if (_records.ContainsKey(user))
            {
                var target = _records[user];
                _records.Remove(user);
                Destroy(target);
                counter.text = "(" + _records.Count.ToString() + ")";
            }
        }

        private void Add(UserData user)
        {
            //Add the user and then resort the display
            if (!_records.ContainsKey(user))
            {
                AddNewRecord(user);
                SortRecords();
                counter.text = "(" + _records.Count.ToString() + ")";
            }
            else
                _records[user].GetComponent<ISteamUserData>().Data = user;
        }

        private void AddNewRecord(UserData user)
        {
            var go = Instantiate(recordTemplate, content);
            var comp = go.GetComponent<ISteamUserData>();
            comp.Data = user;
            _records.Add(user, go);
        }

        private void SortRecords()
        {
            var keys = _records.Keys.ToList();
            keys.Sort((a, b) => string.Compare(a.Nickname, b.Nickname, StringComparison.Ordinal));

            foreach (var key in keys)
            {
                _records[key].transform.SetAsLastSibling();
            }

            counter.text = "(" + _records.Count.ToString() + ")";
        }

        /// <summary>
        /// Initializes the custom group with a specified name, collection of users, and expansion state.
        /// </summary>
        /// <param name="name">The name of the group to be displayed.</param>
        /// <param name="users">The collection of users to populate within the group.</param>
        /// <param name="expanded">Determines whether the group should start in an expanded state.</param>
        public void InitializeCustom(string name, List<UserData> users, bool expanded)
        {
            label.text = name;
            toggle.isOn = expanded;
            _type = GroupType.None;

            foreach (var user in users)
                if (!_records.ContainsKey(user))
                    AddNewRecord(user);

            SortRecords();
        }

        /// <summary>
        /// Initialises the group display specifically for online members.
        /// </summary>
        /// <param name="name">The name of the online group to be displayed.</param>
        /// <param name="users">The list of online users to populate within the group.</param>
        /// <param name="expanded">Determines whether the group should start in an expanded state.</param>
        public void InitializeOnline(string name, List<UserData> users, bool expanded)
        {
            label.text = name;
            toggle.isOn = expanded;
            _type = GroupType.Online;

            foreach (var user in users)
                if (!_records.ContainsKey(user))
                    AddNewRecord(user);

            SortRecords();
        }

        /// <summary>
        /// Initialises the offline group with the specified name, list of users, and expansion state.
        /// </summary>
        /// <param name="name">The name of the offline group to be displayed.</param>
        /// <param name="users">The collection of users to populate in the group.</param>
        /// <param name="expanded">Determines whether the group should start in an expanded state.</param>
        public void InitializeOffline(string name, List<UserData> users, bool expanded)
        {
            label.text = name;
            toggle.isOn = expanded;
            _type = GroupType.Offline;

            foreach (var user in users)
                if (!_records.ContainsKey(user))
                    AddNewRecord(user);

            SortRecords();
        }

        /// <summary>
        /// Configures and initialises the group to represent friends currently in-game, setting its display name, user list, and expansion state.
        /// </summary>
        /// <param name="name">The title of the group to be displayed in the UI.</param>
        /// <param name="users">The collection of users currently in-game that will be shown in the group.</param>
        /// <param name="expanded">Indicates whether the group should be expanded when initialized.</param>
        public void InitializeInGame(string name, List<UserData> users, bool expanded)
        {
            label.text = name;
            toggle.isOn = expanded;
            _type = GroupType.InGame;

            foreach (var user in users)
                if (!_records.ContainsKey(user))
                    AddNewRecord(user);

            SortRecords();
        }


        /// <summary>
        /// Initialises the group with the specified name, a collection of users, and expansion state,
        /// and categorises it as "Other Game".
        /// </summary>
        /// <param name="groupName">The name of the group to be displayed.</param>
        /// <param name="users">The collection of users to populate within the group.</param>
        /// <param name="expanded">Determines whether the group should start in an expanded state.</param>
        public void InitializeInOther(string groupName, List<UserData> users, bool expanded)
        {
            label.text = groupName;
            toggle.isOn = expanded;
            _type = GroupType.OtherGame;

            foreach (var user in users)
                if (!_records.ContainsKey(user))
                    AddNewRecord(user);

            SortRecords();
        }
    }
}
#endif