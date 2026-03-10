#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Lobby Member")]
    [HelpURL("https://kb.heathen.group/steam/features/lobby/unity-lobby/steam-lobby-member-data")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamLobbyMemberData : MonoBehaviour
    {
        public LobbyMemberData Data
        {
            get => _mData;
            set
            {
                _mData = value;
                if(_mUserData == null)
                    _mUserData = GetComponent<SteamUserData>();
                _mUserData.Data = value.user;
                if (_mEvents != null)
                    _mEvents.onMetadataChanged?.Invoke(_mData.lobby, _mData);
            }
        }
        
        

        public LobbyData Lobby
        {
            get => Data.lobby;
        }

        private LobbyMemberData _mData;
        private SteamUserData _mUserData;
        private SteamLobbyMemberDataEvents _mEvents;
        [FormerlySerializedAs("m_Delegates")] 
        [SerializeField]
        private List<string> Delegates;

        private void Awake()
        {
            _mUserData = GetComponent<SteamUserData>();
            _mEvents = GetComponent<SteamLobbyMemberDataEvents>();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteamLobbyMemberData), true)]
    public class SteamLobbyMemberDataEditor : ModularEditor
    {
        private SteamToolsSettings _settings;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamLobbyMemberChatMessage),
            typeof(SteamLobbyMemberDataEvents),
        };

        private void OnEnable()
        {
            _settings = SteamToolsSettings.GetOrCreate();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefault(
                "Project/Player/Steamworks"
                , $"https://partner.steamgames.com/apps/landing/{_settings.Get(_settings.ActiveApp.Value).applicationId}"
                , "https://kb.heathen.group/steam/features/lobby"
                , "https://discord.gg/heathen-group-463483739612381204"
                , null);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif