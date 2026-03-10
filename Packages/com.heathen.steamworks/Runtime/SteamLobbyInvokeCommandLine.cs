#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Command Line", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyDataEvents))]
    public class SteamLobbyInvokeCommandLine : MonoBehaviour
    {
        public enum Rule
        {
            Any,
            PartyOnly,
            SessionOnly,
            GeneralOnly,
            NotParty,
            NotSession,
            NotGeneral,
        }

        [SettingsField(0,false, "Launch Command")]
        public Rule joinRequestedWhen;

        private SteamLobbyDataEvents _mEvents;
        private LobbyData _pendingLobby = 0;

        private void Start()
        {
            _mEvents = GetComponent<SteamLobbyDataEvents>();

            _pendingLobby = API.Matchmaking.Client.GetCommandLineConnectLobby();
            if (_pendingLobby.IsValid)
            {
                SteamTools.Events.OnLobbyDataUpdate += HandleLobbyDataUpdate;
                _pendingLobby.RequestData();
            }
        }

        private void HandleLobbyDataUpdate(LobbyData lobby, LobbyMemberData? member)
        {
            SteamTools.Events.OnLobbyDataUpdate -= HandleLobbyDataUpdate;
            switch (joinRequestedWhen)
            {
                case Rule.Any:
                    _mEvents.onLobbyJoinRequest?.Invoke(_pendingLobby, UserData.Me);
                    break;
                case Rule.GeneralOnly:
                    if (_pendingLobby.IsGeneral)
                        _mEvents.onLobbyJoinRequest?.Invoke(_pendingLobby, UserData.Me);
                    break;
                case Rule.NotGeneral:
                    if (!_pendingLobby.IsGeneral)
                        _mEvents.onLobbyJoinRequest?.Invoke(_pendingLobby, UserData.Me);
                    break;
                case Rule.PartyOnly:
                    if (_pendingLobby.IsParty)
                        _mEvents.onLobbyJoinRequest?.Invoke(_pendingLobby, UserData.Me);
                    break;
                case Rule.NotParty:
                    if (!_pendingLobby.IsParty)
                        _mEvents.onLobbyJoinRequest?.Invoke(_pendingLobby, UserData.Me);
                    break;
                case Rule.SessionOnly:
                    if (_pendingLobby.IsSession)
                        _mEvents.onLobbyJoinRequest?.Invoke(_pendingLobby, UserData.Me);
                    break;
                case Rule.NotSession:
                    if (!_pendingLobby.IsSession)
                        _mEvents.onLobbyJoinRequest?.Invoke(_pendingLobby, UserData.Me);
                    break;
            }
        }
    }
}
#endif