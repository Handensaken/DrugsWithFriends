#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [ModularEvents(typeof(SteamLobbyMemberData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyMemberData))]
    public class SteamLobbyMemberDataEvents : MonoBehaviour
    {
        [EventField]
        public UnityEvent<bool> onIsLobbyOwnerStatus;
        [EventField]
        public UnityEvent<bool> onReadyChanged;
        [EventField]
        public UnityEvent<LobbyData, LobbyMemberData> onMetadataChanged;

        private SteamLobbyMemberData _mSteamLobbyMemberData;
        private bool _mReady = false;

        private void Awake()
        {
            _mSteamLobbyMemberData = GetComponent<SteamLobbyMemberData>();
            SteamTools.Events.OnLobbyDataUpdate += GlobalDataUpdate;
            SteamTools.Events.OnLobbyChatUpdate += ChatStateUpdate;
            _mReady = _mSteamLobbyMemberData.Data.IsReady;
            onReadyChanged?.Invoke(_mReady);
        }

        private void OnDestroy()
        {
            SteamTools.Events.OnLobbyDataUpdate -= GlobalDataUpdate;
            SteamTools.Events.OnLobbyChatUpdate -= ChatStateUpdate;
        }
        
        private void GlobalDataUpdate(LobbyData lobby, LobbyMemberData? member)
        {
            if (lobby == _mSteamLobbyMemberData.Data.lobby && member.HasValue && member.Value == _mSteamLobbyMemberData.Data)
            {
                onIsLobbyOwnerStatus?.Invoke(lobby.Owner.user == _mSteamLobbyMemberData.Data.user);

                if (_mReady != _mSteamLobbyMemberData.Data.IsReady)
                {
                    onReadyChanged?.Invoke(_mSteamLobbyMemberData.Data.IsReady);
                }

                onMetadataChanged?.Invoke(lobby, member.Value);
            }
        }

        private void ChatStateUpdate(LobbyData lobby, UserData user, EChatMemberStateChange state)
        {
            if (lobby == _mSteamLobbyMemberData.Data.lobby)
            {
                onIsLobbyOwnerStatus?.Invoke(lobby.Owner.user == _mSteamLobbyMemberData.Data.user);
            }
        }
    }
}
#endif