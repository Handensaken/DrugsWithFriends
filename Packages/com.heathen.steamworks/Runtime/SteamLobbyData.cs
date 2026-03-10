#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Lobby")]
    [HelpURL("https://heathen.group/kb/lobby/")]
    public class SteamLobbyData : MonoBehaviour, ISteamLobbyData
    {
        public enum LoadOnStart
        {
            None,
            Any,
            Party,
            Session,
            General
        }

        public LoadOnStart load = LoadOnStart.None;

        public LobbyData Data
        {
            get => _mData;
            set
            {
                _mData = value;
                onChanged?.Invoke(value);
            }
        }

        [HideInInspector]
        public LobbyDataEvent onChanged;

        private LobbyData _mData;

        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates;

        private void Start()
        {
            switch (load)
            {
                case LoadOnStart.Any:
                    if (API.Matchmaking.Client.MemberOfLobbies.Count > 0)
                        Data = API.Matchmaking.Client.MemberOfLobbies[0];
                    break;
                case LoadOnStart.General:
                    if (API.Matchmaking.Client.MemberOfLobbies.Count > 0)
                        foreach (var lobby in API.Matchmaking.Client.MemberOfLobbies)
                            if (lobby.IsGeneral)
                            {
                                Data = lobby;
                                break;
                            }
                    break;
                case LoadOnStart.Session:
                    if (API.Matchmaking.Client.MemberOfLobbies.Count > 0)
                        foreach (var lobby in API.Matchmaking.Client.MemberOfLobbies)
                            if (lobby.IsSession)
                            {
                                Data = lobby;
                                break;
                            }
                    break;
                case LoadOnStart.Party:
                    if (API.Matchmaking.Client.MemberOfLobbies.Count > 0)
                        foreach (var lobby in API.Matchmaking.Client.MemberOfLobbies)
                            if (lobby.IsParty)
                            {
                                Data = lobby;
                                break;
                            }
                    break;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteamLobbyData), true)]
    public class SteamLobbyDataEditor : ModularEditor
    {
        private SteamToolsSettings _settings;
        private SerializedProperty _loadProperty;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new[]
        {
            // Fields
            typeof(SteamLobbyHexIdInputField),
            typeof(SteamLobbyHexIdLabel),
            typeof(SteamLobbyMaxSlots),
            typeof(SteamLobbyMemberCount),
            typeof(SteamLobbyMembers),
            typeof(SteamLobbyName),

            // Functions / single-instance components
            typeof(SteamLobbyCreate),
            typeof(SteamLobbyJoin),
            typeof(SteamLobbyJoinOnInvite),
            typeof(SteamLobbyJoinSessionLobby),
            typeof(SteamLobbyLeave),
            typeof(SteamLobbyInvite),
            typeof(SteamLobbyQuickMatch),
            typeof(SteamLobbyMetadata),
            typeof(SteamLobbyGameServer),
            typeof(SteamLobbyChatUI),
            typeof(SteamLobbyInputUI),
            typeof(SteamLobbyDataEvents),
            typeof(SteamLobbyInvokeCommandLine)
        };

        private void OnEnable()
        {
            _settings = SteamToolsSettings.GetOrCreate();
            _loadProperty = serializedObject.FindProperty("load");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefault(
                "Project/Player/Steamworks"
                , $"https://partner.steamgames.com/apps/landing/{_settings.Get(_settings.ActiveApp.Value).applicationId}"
                , "https://kb.heathen.group/steam/features/lobby"
                , "https://discord.gg/heathen-group-463483739612381204"
                , new[]{ _loadProperty });
            
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif