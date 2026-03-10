#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/User")]
    public class SteamUserData : MonoBehaviour, ISteamUserData
    {
        public enum ManagedEvents
        {
            Changed,
            Clicked,
        }

        public bool localUser = false;

        public UserData Data
        {
            get => _mData;
            set
            {
                _mData = value;
                onChanged?.Invoke(value, (EPersonaChange)int.MaxValue);
            }
        }

        [HideInInspector]
        public UnityEvent<UserData, EPersonaChange> onChanged;

        private UserData _mData;
        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates;

        private void Awake()
        {
            SteamTools.Events.OnPersonaStateChange += GlobalPersonaUpdate;
        }

        private void Start()
        {
            SteamTools.Interface.WhenReady(HandleInitialization);
        }

        private void HandleInitialization()
        {
            if (localUser) Data = UserData.Me;
        }

        private void OnDestroy()
        {
            SteamTools.Events.OnPersonaStateChange -= GlobalPersonaUpdate;
        }

        private void GlobalPersonaUpdate(UserData user, EPersonaChange changeFlag)
        {
            if (user == Data)
                onChanged?.Invoke(user, changeFlag);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteamUserData), true)]
    public class SteamUserDataEditor : ModularEditor
    {
        private SerializedProperty _localUserProp;
        private SteamToolsSettings _settings;

        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamUserAvatar),
            typeof(SteamUserHexInput),
            typeof(SteamUserHexLabel),
            typeof(SteamUserLevel),
            typeof(SteamUserName),
            typeof(SteamUserStatus),
            typeof(SteamUserLobbyInvite),
            typeof(SteamUserDataEvents),
        };

        private void OnEnable()
        {
            _settings = SteamToolsSettings.GetOrCreate();
            _localUserProp = serializedObject.FindProperty("localUser");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefault(
                "Project/Player/Steamworks"
                , $"https://partner.steamgames.com/apps/landing/{_settings.Get(_settings.ActiveApp.Value).applicationId}"
                , "https://kb.heathen.group/steam/features/lobby"
                , "https://discord.gg/heathen-group-463483739612381204"
                , new[] { _localUserProp });

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif