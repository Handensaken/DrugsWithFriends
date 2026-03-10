#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Create", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyCreate : MonoBehaviour
    {
        public enum SteamLobbyType
        {
            Private = 0,        // the only way to join the lobby is to invite someone else
            FriendsOnly = 1,    // shows for friends or invitees, but not in the lobby list
            Public = 2,         // visible for friends and in the lobby list
            Invisible = 3,      // returned by search, but not visible to other friends
        }

        /// <summary>
        /// If true and creating a Party, it will leave any existing lobby first, if true when creating a session it will notify any existing party of the new session lobby.
        /// </summary>
        [SettingsField(0, true)]
        [Tooltip("If true and creating a Party it will leave any existing lobby first, if true when creating a session it will notify any existing party of the new session lobby.")]
        public bool partyWise;
        /// <summary>
        /// How will this lobby be used? This is an optional feature. If set to Party or Session then features of the LobbyData object can be used in code to fetch the created lobby such as LobbyData.GetGroup(...)
        /// </summary>
        [SettingsField(0,false,"Create")]
        [Tooltip("How will this lobby be used? This is an optional feature. If set to Party or Session then features of the LobbyData object can be used in code to fetch the created lobby such as LobbyData.GetGroup(...)")]
        public SteamLobbyModeType usageHint = SteamLobbyModeType.Session;
        /// <summary>
        /// The number of slots the newly created lobby should have
        /// </summary>
        [SettingsField(0,false, "Create")]
        [Tooltip("The number of slots the newly created lobby should have")]
        public int slots;
        /// <summary>
        /// The type of lobby to create
        /// </summary>
        [SettingsField(0,false, "Create")]
        [Tooltip("The type of lobby to create")]
        public SteamLobbyType type;
        
        private SteamLobbyData _mInspector;
        private SteamLobbyDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLobbyData>();
            _mEvents = GetComponent<SteamLobbyDataEvents>();
        }

        public void Create()
        {
            LobbyData partyLobby = CSteamID.Nil;

            if (partyWise && LobbyData.PartyLobby(out partyLobby) && usageHint == SteamLobbyModeType.Party)
            {
                partyLobby.Leave();
                partyLobby = CSteamID.Nil;
            }

            if(partyLobby.IsValid && !partyLobby.IsOwner)
            {
                Debug.LogWarning("Only a party lobby leader can create lobbies");
                return;
            }

            LobbyData.Create((ELobbyType)type, usageHint, slots, (createResult, createdLobby, createIoError) =>
            {
                if (!createIoError && createResult == EResult.k_EResultOK)
                {
                    _mInspector.Data = createdLobby;

                    if(_mEvents != null)
                        _mEvents.onCreate?.Invoke(createdLobby);

                    if (partyLobby.IsValid && usageHint == SteamLobbyModeType.Session)
                    {
                        if(type == SteamLobbyType.Private)
                        {
                            foreach(var member in partyLobby.Members)
                            {
                                if (!member.user.IsMe)
                                    member.user.InviteToLobby(createdLobby);
                            }
                        }
                        else
                            partyLobby[LobbyData.DataSessionLobby] = createdLobby.ToString();
                    }
                }
                else
                {
                    if(_mEvents != null)
                        _mEvents.onCreationFailure?.Invoke(createResult);
                }
            });
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyCreate), true)]
    public class SteamLobbyCreateEditor : UnityEditor.Editor
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