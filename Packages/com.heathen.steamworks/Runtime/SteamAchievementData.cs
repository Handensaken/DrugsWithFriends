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
    /// <summary>
    /// Represents data for a Steam achievement.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Steamworks/Achievement")]
    [HelpURL("https://heathen.group/kb/steam-features-achievements/")]
    public class SteamAchievementData : MonoBehaviour
    {
        /// <summary>
        /// The API name of the achievement as defined in the Steamworks portal.
        /// </summary>
        public string apiName;
        /// <summary>
        /// Gets or sets the achievement data using the API name.
        /// </summary>
        public AchievementData Data
        {
            get => apiName;
            set => apiName = value.ApiName;
        }

        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates;

        /// <summary>
        /// Unlocks the achievement.
        /// </summary>
        public void Unlock() => Data.Unlock();
        /// <summary>
        /// Clears the achievement status.
        /// </summary>
        public void Clear() => Data.Clear();
        /// <summary>
        /// Stores the achievement status to Steam.
        /// </summary>
        public void Store() => Data.Store();
        /// <summary>
        /// Sets the achievement as unlocked or cleared.
        /// </summary>
        /// <param name="value">If true, unlocks the achievement; if false, clears it.</param>
        public void SetAchieved(bool value)
        {
            if (value)
                Data.Unlock();
            else
                Data.Clear(); 
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for <see cref="SteamAchievementData"/>.
    /// </summary>
    [CustomEditor(typeof(SteamAchievementData), true)]
    public class SteamAchievementDataEditor : ModularEditor
    {
        private string[] _options;
        private int _selectedIndex;
        private SerializedProperty _apiNameProp;
        private SteamToolsSettings _settings;

        /// <summary>
        /// Allowed types for this editor.
        /// </summary>
        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamAchievementName),
            typeof(SteamAchievementDescription),
            typeof(SteamAchievementIcon),
            typeof(SteamAchievementChanged),
        };

        private void OnEnable()
        {
            _apiNameProp = serializedObject.FindProperty("apiName");
            RefreshOptions();
        }

        private void RefreshOptions()
        {
            _settings = SteamToolsSettings.GetOrCreate();
            var list = _settings != null && _settings.achievements != null
                ? _settings.achievements
                : new List<string>();

            if (list.Count > 0)
            {
                _options = list.ToArray();
                var current = _apiNameProp.stringValue;
                _selectedIndex = Mathf.Max(0, Array.IndexOf(_options, current));
                if (_selectedIndex < 0)
                    _selectedIndex = 0;
            }
            else
            {
                _options = null;
            }
        }

        /// <summary>
        /// Draws the inspector GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if(_settings)
                _settings = SteamToolsSettings.GetOrCreate();

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Player/Steamworks");
            if (EditorGUILayout.LinkButton("Portal"))
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + _settings.Get(_settings.ActiveApp.Value).applicationId.ToString());
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam/features/achievements");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // === Achievement dropdown ===
            if (_options == null || _options.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No achievements found!.\n\n" +
                    "Open Project Settings > Player > Steamworks to configure your achievements.",
                    MessageType.Warning
                );

                serializedObject.ApplyModifiedProperties();
                return;
            }

            _selectedIndex = EditorGUILayout.Popup(_selectedIndex, _options);
            if (_selectedIndex >= 0 && _selectedIndex < _options.Length)
                _apiNameProp.stringValue = _options[_selectedIndex];

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