#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Join", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyJoin : MonoBehaviour
    {
        /// <summary>
        /// If true and creating a Party, it will leave any existing lobby first, if true when creating a session it will notify any existing party of the new session lobby.
        /// </summary>
        [SettingsField(0, true)]
        [Tooltip("If true and creating a Party it will leave any existing lobby first, if true when creating a session it will notify any existing party of the new session lobby.")]
        public bool partyWise;

        private SteamLobbyData _mInspector;
        private SteamLobbyDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLobbyData>();
            _mEvents = GetComponent<SteamLobbyDataEvents>();
        }

        public void RequestJoin(SteamLobbyJoin toRequest)
        {
            if (toRequest != null && _mInspector.Data.IsValid)
                toRequest.Join(_mInspector.Data);
        }

        public void JoinFromIdString(string id)
        {
            Join(LobbyData.Get(id));
        }

        public void JoinFromIdInputField(TMPro.TMP_InputField input)
        {
            Join(LobbyData.Get(input.text));
        }

        public void Join(LobbyData lobby)
        {
            // If lobby is not valid
            if (!lobby.IsValid)
                return;

            // If we are already tracking this lobby
            if (lobby == _mInspector.Data)
                return;

            LobbyData partyLobby = CSteamID.Nil;
            if (partyWise && LobbyData.PartyLobby(out partyLobby))
            {
                if (!partyLobby.IsOwner)
                {
                    Debug.LogWarning("Only a party lobby leader can join lobbies");
                    return;
                }
            }

            lobby.Join((enterLobby, enterIoError) =>
            {
                if (!enterIoError && enterLobby.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                {
                    _mInspector.Data = enterLobby.Lobby;
                    if (_mEvents != null)
                        _mEvents.onEnterSuccess?.Invoke(enterLobby.Lobby);

                    if (partyLobby.IsValid && enterLobby.Lobby.IsSession)
                    {
                        partyLobby[LobbyData.DataSessionLobby] = enterLobby.Lobby.ToString();
                    }
                }
                else
                {
                    if(_mEvents != null)
                        _mEvents.onEnterFailure?.Invoke(enterLobby.Response);
                }
            });
        }

        public void JoinOnRequestEvent(LobbyData lobby, UserData user)
        {
            Join(lobby);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyJoin), true)]
    public class SteamLobbyJoinEditor : UnityEditor.Editor
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