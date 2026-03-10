#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Invite", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyInvite : MonoBehaviour
    {
        private SteamLobbyData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLobbyData>();
        }

        public void OpenOverlay()
        {
            if (_mInspector.Data.IsValid)
                API.Overlay.Client.ActivateInviteDialog(_mInspector.Data);
            else
                Debug.LogWarning("No lobby to invite to");
        }

        public void InviteUser(UserData user)
        {
            if (_mInspector.Data.IsValid)
                _mInspector.Data.InviteUserToLobby(user);
            else
                Debug.LogWarning("No lobby to invite to");
        }

        public void InviteFromString(string id)
        {
            var user = UserData.Get(id);
            InviteUser(user);
        }

        public void InviteFromInput(TMPro.TMP_InputField input)
        {
            if(input != null)
                InviteFromString(input.text);
        }

        public void InviteFromUser(SteamUserData user)
        {
            if (user != null && user.Data.IsValid && !user.Data.IsMe)
                InviteUser(user.Data);
        }
    }
}
#endif