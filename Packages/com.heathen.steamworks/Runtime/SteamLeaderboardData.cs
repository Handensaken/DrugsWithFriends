#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Heathen.SteamworksIntegration.UI;
using Steamworks;
using System;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Leaderboard exposing features as Fields and Settings to the Unity Inspector
    /// </summary>
    [AddComponentMenu("Steamworks/Leaderboard")]
    [HelpURL("https://heathen.group/kb/leaderboards/")]
    public class SteamLeaderboardData : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public enum LeaderboardSortMethod
        {
            TopIsLowestScore = 1,  // the top-score is the lowest number
            TopIsHighestScore = 2, // the top-score is the highest number
        }

        // the display type (used by the Steam Community website) for a leaderboard
        public enum LeaderboardDisplayType
        {
            Numeric = 1,           // simple numerical score
            TimeSeconds = 2,       // the score represents a time, in seconds
            TimeMilliSeconds = 3,  // the score represents a time, in milliseconds
        }

        public string apiName;
        public bool createIfMissing;
        public LeaderboardDisplayType createAsDisplay = LeaderboardDisplayType.Numeric;
        public LeaderboardSortMethod createWithSort = LeaderboardSortMethod.TopIsLowestScore;

        public LeaderboardData Data
        {
            get => _data;
            set
            {
                _data = value;
                if (_events)
                    _events.onChange?.Invoke();
            }
        }

        [FormerlySerializedAs("_delegates")] 
        [FormerlySerializedAs("m_Delegates")] 
        [SerializeField]
        private List<string> delegates;
        private LeaderboardData _data;
        private SteamLeaderboardDataEvents _events;

        private void Awake()
        {
            _events = GetComponent<SteamLeaderboardDataEvents>();
        }

        private void Interface_OnReady()
        {
            SteamTools.Interface.OnReady -= Interface_OnReady;

            if (!_data.IsValid)
            {
                if(!string.IsNullOrEmpty(apiName))
                {
                    _data = SteamTools.Interface.GetBoard(apiName);
                    if (!_data.IsValid)
                    {
                        if (createIfMissing)
                            API.Leaderboards.Client.FindOrCreate(Data.apiName, (ELeaderboardSortMethod)createWithSort, (ELeaderboardDisplayType)createAsDisplay, (data, ioError) =>
                            {
                                if (!ioError)
                                {
                                    _data = data;
                                    if (_events != null)
                                        _events.onFindOrCreate?.Invoke();
                                }
                                else if (_events != null)
                                    _events.onFindOrCreateFailure?.Invoke();
                            });
                    }
                }
            }
        }

        private void Start()
        {
            _events = GetComponent<SteamLeaderboardDataEvents>();
            if (SteamTools.Interface.IsReady)
                Interface_OnReady();
            else
                SteamTools.Interface.OnReady += Interface_OnReady;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteamLeaderboardData), true)]
    public class SteamLeaderboardDataEditor : ModularEditor
    {
        private SteamToolsSettings _settings;

        private string[] _options;
        private int _selectedIndex;
        private SerializedProperty _apiNameProp;
        private SerializedProperty _createIfMissingProp;
        private SerializedProperty _createAsDisplayProp;
        private SerializedProperty _createWithSortProp;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new[]
        {
            typeof(SteamLeaderboardDataEvents),
            typeof(SteamLeaderboardDisplay),
            typeof(SteamLeaderboardName),
            typeof(SteamLeaderboardRank),
            typeof(SteamLeaderboardUpload),
            typeof(SteamLeaderboardUserEntry),
        };

        private void OnEnable()
        {
            _apiNameProp = serializedObject.FindProperty("apiName");
            _createIfMissingProp = serializedObject.FindProperty("createIfMissing");
            _createAsDisplayProp = serializedObject.FindProperty("createAsDisplay");
            _createWithSortProp = serializedObject.FindProperty("createWithSort");

            _settings = SteamToolsSettings.GetOrCreate();
            
            RefreshOptions();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void RefreshOptions()
        {
            var settings = SteamToolsSettings.GetOrCreate();
            var list = settings && settings.leaderboards != null
                ? settings.leaderboards
                : new List<string>();

            var temp = new string[list.Count + 1];
            for (int i = 0; i < list.Count; i++)
                temp[i] = list[i];
            temp[list.Count] = "<new>";

            _options = temp;

            var current = _apiNameProp.stringValue;
            _selectedIndex = Array.IndexOf(_options, current);
            if (_selectedIndex < 0)
                _selectedIndex = _options.Length - 1; // default to <new>
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // --- Header links ---
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Player/Steamworks");
            if (EditorGUILayout.LinkButton("Portal"))
                if (_settings.ActiveApp != null)
                    Application.OpenURL("https://partner.steamgames.com/apps/landing/" +
                                        _settings.Get(_settings.ActiveApp.Value).applicationId);
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam/features/leaderboards");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (_options == null)
                RefreshOptions();

            _selectedIndex = EditorGUILayout.Popup("Leaderboard", _selectedIndex, _options);

            if (_selectedIndex >= 0 && _options != null && _selectedIndex < _options.Length - 1)
            {
                // Existing leaderboard selected
                _apiNameProp.stringValue = _options[_selectedIndex];
            }
            else
            {
                // <new> selected — show editable fields
                EditorGUILayout.PropertyField(_apiNameProp, new GUIContent("API Name"));
                EditorGUILayout.PropertyField(_createIfMissingProp);
                EditorGUILayout.PropertyField(_createAsDisplayProp);
                EditorGUILayout.PropertyField(_createWithSortProp);
            }

            EditorGUILayout.Space();

            // --- Features Dropdown ---
            HideAllAllowedComponents();
            DrawAddFieldDropdown();

            // --- Draw existing components via attributes ---
            EditorGUI.indentLevel++;
            DrawModularComponents();
            EditorGUI.indentLevel--;

            // --- Draw Functions as Flags (single-instance components) ---
            DrawFunctionFlags();

            // --- Draw Settings / Elements / Templates / Events ---
            DrawFields<SettingsFieldAttribute>("Settings");
            DrawFields<ElementFieldAttribute>("Elements");
            DrawFields<TemplateFieldAttribute>("Templates");
            DrawEventFields();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif