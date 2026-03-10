#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Friends = Heathen.SteamworksIntegration.API.Friends;

namespace Heathen.SteamworksIntegration
{
    public class RichPresenceReader : MonoBehaviour
    {
        [Serializable]
        public class RichPresenceReaderUpdatedEvent : UnityEvent<RichPresenceReader>
        { }

        public AppData App { get; private set; } = AppId_t.Invalid;
        public UserData User { get => _currentUser; set => Apply(value); }
        public Dictionary<string, string> Values { get; private set; } = new Dictionary<string, string>();

        public RichPresenceReaderUpdatedEvent evtUpdate;

        private UserData _currentUser = CSteamID.Nil;

        private void OnEnable()
        {
            SteamTools.Events.OnFriendRichPresenceUpdate += HandleChange;
        }

        private void OnDisable()
        {
            SteamTools.Events.OnFriendRichPresenceUpdate -= HandleChange;
        }

        public void Apply(UserData user)
        {
            _currentUser = user;

            if(user.GetGamePlayed(out var gameInfo))
            {
                App = gameInfo.Game;
                Values = Friends.Client.GetFriendRichPresence(user);
                evtUpdate.Invoke(this);
            }
            else
            {
                App = AppId_t.Invalid;
                Values.Clear();
                evtUpdate.Invoke(this);
            }
        }

        private void HandleChange(UserData friend, AppData app)
        {
            if (friend != _currentUser) return;
            App = app;
            Values = Friends.Client.GetFriendRichPresence(friend);
            evtUpdate.Invoke(this);
        }
    }
}
#endif