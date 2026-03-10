#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Join Session", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    [RequireComponent(typeof(SteamLobbyJoin))]
    public class SteamLobbyJoinSessionLobby : MonoBehaviour
    {
        [SettingsField(0,false, "Join")]
        public SteamLobbyData partyLobbyData;
        [SettingsField(0,false, "Join")]
        public bool leaveOnSessionClear = true;
        private SteamLobbyData _mInspector;
        private SteamLobbyJoin _mJoin;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLobbyData>();
            
            if (partyLobbyData != null)
                partyLobbyData.onChanged.AddListener(HandleChange);

            SteamTools.Events.OnLobbyDataUpdate += HandleDataUpdate;
        }

        private void OnDestroy()
        {
            if (partyLobbyData != null)
                partyLobbyData.onChanged.RemoveListener(HandleChange);

            SteamTools.Events.OnLobbyDataUpdate -= HandleDataUpdate;
        }

        private void HandleChange(LobbyData arg0)
        {
            // A new party lobby has been set, we should check if this party lobby is valid, and if so is a session declared on it
            // Check if the party is valid
            if(arg0.IsValid)
            {
                // Get the session ID if any is declared
                var sessionId = arg0[LobbyData.DataSessionLobby];
                
                // If this is not null, then we have a declared session lobby ID
                if(!string.IsNullOrEmpty(sessionId))
                {
                    var sessionLobby = LobbyData.Get(sessionId);
                    JoinSessionLobby(sessionLobby);
                }
            }
        }

        private void HandleDataUpdate(LobbyData lobby, LobbyMemberData? member)
        {
            // If we are tracking a part, this update is for this party, and the update is a lobby update, not a member update
            if (partyLobbyData != null && partyLobbyData.Data == lobby && !member.HasValue)
            {
                // Get the session ID if any is declared
                var sessionId = partyLobbyData.Data[LobbyData.DataSessionLobby];

                // If this is not null, then we have a declared session lobby ID
                if (!string.IsNullOrEmpty(sessionId))
                {
                    var sessionLobby = LobbyData.Get(sessionId);
                    JoinSessionLobby(sessionLobby);
                }
                else if (leaveOnSessionClear && _mInspector.Data.IsValid)
                {
                    // empty session lobby so we should leave if we are in a session lobby
                    _mInspector.Data.Leave();
                    _mInspector.Data = CSteamID.Nil;
                }
            }
        }

        private void JoinSessionLobby(LobbyData sessionLobby)
        {
            // If it's a valid lobby, check if we are members of it
            if (sessionLobby.IsValid)
            {
                // If our current lobby is different from this lobby ... leave the current
                if (_mInspector.Data != sessionLobby)
                {
                    if (_mInspector.Data.IsValid)
                        _mInspector.Data.Leave();
                }

                // If we are not already a member of this session lobby, join it
                if (!sessionLobby.IsAMember(UserData.Me))
                {
                    _mJoin.Join(sessionLobby);
                }
                // We are already a member so set it as the tracked lobby on this parent
                else
                {
                    _mInspector.Data = sessionLobby;
                }
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyJoinSessionLobby), true)]
    public class SteamLobbyJoinSessionLobbyEditor : UnityEditor.Editor
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