#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Join on Invite", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyJoin))]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyJoinOnInvite : MonoBehaviour
    {
        public enum JoinOnMode
        {
            WithInitialInvite,
            AfterAcceptInFriendChat
        }

        public enum FilterMode
        {
            None,
            IgnoreIfInParty,
            IgnoreIfInSession,
            IgnoreIfInAny,
        }

        public enum PreprocessOptions
        {
            None,
            LeaveAllFirst,
            LeavePartyFirst,
            LeaveSessionFirst,
        }

        [SettingsField(0,false, "Join")]
        public JoinOnMode mode = JoinOnMode.WithInitialInvite;
        [SettingsField(0,false, "Join")]
        public FilterMode filter = FilterMode.None;
        [SettingsField(0,false, "Join")]
        public PreprocessOptions preprocess = PreprocessOptions.None;

        private SteamLobbyData _mInspector;
        private SteamLobbyJoin _mJoin;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLobbyData>();
            _mJoin = GetComponent<SteamLobbyJoin>();
            SteamTools.Events.OnLobbyInvite += HandleInviteReceived;
            SteamTools.Events.OnLobbyJoinRequested += HandleInviteAccepted;
        }

        private void OnDestroy()
        {
            SteamTools.Events.OnLobbyInvite -= HandleInviteReceived;
            SteamTools.Events.OnLobbyJoinRequested -= HandleInviteAccepted;
        }

        private bool CanProcess()
        {
            switch(filter)
            {
                case FilterMode.None:
                    return true;
                case FilterMode.IgnoreIfInParty:
                    return !LobbyData.PartyLobby(out var _);
                case FilterMode.IgnoreIfInSession:
                    return !LobbyData.SessionLobby(out var _);
                case FilterMode.IgnoreIfInAny:
                    return API.Matchmaking.Client.MemberOfLobbies.Count == 0;
                default:  return true;
            }
        }

        private void Preprocess()
        {
            if (preprocess == PreprocessOptions.None)
                return;

            var lobbies = API.Matchmaking.Client.MemberOfLobbies.ToArray();
            for (int i = 0; i < lobbies.Length; i++)
            {
                switch(preprocess)
                {
                    case PreprocessOptions.LeaveAllFirst:
                        lobbies[i].Leave();
                        break;
                    case PreprocessOptions.LeaveSessionFirst:
                        if (lobbies[i].IsSession)
                            lobbies[i].Leave();
                        break;
                    case PreprocessOptions.LeavePartyFirst:
                        if (lobbies[i].IsParty)
                            lobbies[i].Leave();
                        break;
                }
            }
        }

        private void HandleInviteReceived(UserData user, LobbyData lobby, GameData game)
        {
            if (mode != JoinOnMode.WithInitialInvite
                || lobby.IsAMember(UserData.Me))
                return;

            Preprocess();
            if (!CanProcess())
                return;

            if (_mJoin != null)
                _mJoin.Join(lobby);
            else
                Debug.LogWarning("To join a lobby you are invited to the GameObject must have a Steam Lobby Join component");
        }

        private void HandleInviteAccepted(LobbyData arg0, UserData arg1)
        {
            if (mode != JoinOnMode.AfterAcceptInFriendChat
                || arg0.IsAMember(UserData.Me))
                return;

            Preprocess();
            if (!CanProcess())
                return;

            if (_mJoin != null)
                _mJoin.Join(arg0);
            else
                Debug.LogWarning("To join a lobby you are invited to the GameObject must have a Steam Lobby Join component");
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
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyJoinOnInvite), true)]
    public class SteamLobbyJoinOnInviteEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif