#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamUserData), "Invite", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserLobbyInvite : MonoBehaviour
    {
        [FormerlySerializedAs("CreateIfMissing")] 
        [SettingsField(0, false,"Invite")]
        public bool createIfMissing = true;
        private SteamUserData _mSteamUserData;

        private void Awake()
        {
            _mSteamUserData = GetComponent<SteamUserData>();
        }

        public void Invite(SteamLobbyData lobby)
        {
            if (lobby == null)
                return;

            if (lobby.Data.IsValid)
            {
                lobby.Data.InviteUserToLobby(_mSteamUserData.Data);
            }
            else if (createIfMissing)
            {
                var creator = lobby.GetComponent<SteamLobbyCreate>();
                var events = lobby.GetComponent<SteamLobbyDataEvents>();
                events.onCreate.AddListener(HandleLobbyCreated);
                creator.Create();
            }

            void HandleLobbyCreated(LobbyData arg0)
            {
                var events = lobby.GetComponent<SteamLobbyDataEvents>();
                events.onCreate.RemoveListener(HandleLobbyCreated);
                lobby.Data.InviteUserToLobby(_mSteamUserData.Data);
            }
        }

        public void Invite(LobbyData lobby)
        {
            if (_mSteamUserData.Data.IsValid
                && lobby.IsValid)
            {
                lobby.InviteUserToLobby(_mSteamUserData.Data);
            }
        }
    }
}
#endif