#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents data for a Steam Input action, providing access to its set, layer, and action details.
    /// </summary>
    [AddComponentMenu("Steamworks/Input Action")]
    [HelpURL("https://heathen.group/kb/input/")]
    public class SteamInputActionData : MonoBehaviour, ISteamInputActionData
    {
        [SerializeField]
        private string setName;
        [SerializeField]
        private string layerName;
        [SerializeField]
        private string actionName;
        
        /// <summary>
        /// Gets or sets the input action set data.
        /// </summary>
        public InputActionSetData Set
        {
            get => _mSet;
            set => _mSet = value;
        }

        /// <summary>
        /// Gets or sets the input action set layer data.
        /// </summary>
        public InputActionSetLayerData Layer
        {
            get => _mLayer;
            set => _mLayer = value;
        }

        /// <summary>
        /// Gets or sets the input action data.
        /// </summary>
        public InputActionData Action
        {
            get => _mAction;
            set => _mAction = value;
        }
        private InputActionSetData _mSet;
        private InputActionSetLayerData _mLayer;
        private InputActionData _mAction;
        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates;

        private void Start()
        {
            if (SteamTools.Interface.IsReady)
                Interface_OnReady();
            else
                SteamTools.Interface.OnReady += Interface_OnReady;
        }

        private void Interface_OnReady()
        {
            SteamTools.Interface.OnReady -= Interface_OnReady;

            _mSet = SteamTools.Interface.GetSet(layerName);
            _mLayer = new() { LayerName = layerName };
            _mAction = SteamTools.Interface.GetAction(actionName);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for <see cref="SteamInputActionData"/>, providing a modular interface for managing input actions.
    /// </summary>
    [CustomEditor(typeof(SteamInputActionData))]
    public class SteamInputActionDataEditor : ModularEditor
    {
        private SerializedProperty _setNameProp;
        private SerializedProperty _layerNameProp;
        private SerializedProperty _actionNameProp;

        private SteamToolsSettings _settings;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new[]
        {
            typeof(SteamInputActionName),
            typeof(SteamInputActionGlyph),
            typeof(SteamInputActionEvent),
        };

        private string[] _setOptions = Array.Empty<string>();
        private string[] _layerOptions = Array.Empty<string>();
        private string[] _actionOptions = Array.Empty<string>();

        private void OnEnable()
        {
            _setNameProp = serializedObject.FindProperty("setName");
            _layerNameProp = serializedObject.FindProperty("layerName");
            _actionNameProp = serializedObject.FindProperty("actionName");

            _settings = SteamToolsSettings.GetOrCreate();

            if (_settings != null)
            {
                _setOptions = _settings.inputSets?.ToArray() ?? Array.Empty<string>();
                _layerOptions = _settings.inputLayers?.ToArray() ?? Array.Empty<string>();
                _actionOptions = _settings.inputActions?.ToArray() ?? Array.Empty<string>();
            }
        }

        /// <summary>
        /// Renders the custom inspector GUI for Steam input action data.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Player/Steamworks");
            if (EditorGUILayout.LinkButton("Portal"))
                // ReSharper disable once PossibleInvalidOperationException
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + _settings.Get(_settings.ActiveApp.Value).applicationId.ToString());
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam/features/input");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            if (_actionOptions == null || _actionOptions.Length == 0)
            {
                EditorGUILayout.HelpBox("No Actions Founds! Configure Steamworks in Project Settings > Player > Steamworks.", MessageType.Warning);

                serializedObject.ApplyModifiedProperties();
                return;
            }

            // ---- Dropdowns for Set / Layer / Action ----
            DrawPopup("Set", _setNameProp, _setOptions);
            DrawPopup("Layer", _layerNameProp, _layerOptions);
            DrawPopup("Action", _actionNameProp, _actionOptions);

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

        private void DrawPopup(string label, SerializedProperty prop, string[] options)
        {
            int index = Mathf.Max(0, Array.IndexOf(options, prop.stringValue));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(60f));
            index = EditorGUILayout.Popup(index, options);
            EditorGUILayout.EndHorizontal();

            if (index >= 0 && index < options.Length)
                prop.stringValue = options[index];
        }
    }
#endif
}
#endif