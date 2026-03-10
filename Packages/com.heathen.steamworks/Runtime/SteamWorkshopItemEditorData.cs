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
    [AddComponentMenu("Steamworks/Workshop Item Editor")]
    [HelpURL("https://kb.heathen.group/steam/features/workshop")]
    public class SteamWorkshopItemEditorData : MonoBehaviour
    {
        /// <summary>
        /// The id of the application that will use this content
        /// </summary>
        public uint consumingAppId;
        /// <summary>
        /// The title of the item
        /// </summary>
        public TMPro.TMP_InputField title;
        /// <summary>
        /// The description of the item
        /// </summary>
        public TMPro.TMP_InputField description;
        /// <summary>
        /// The local folder where the item's content is located
        /// </summary>
        public TMPro.TMP_InputField contentFolderPath;
        /// <summary>
        /// The local file that is the item's main preview image, this must be smaller than the max size allowed in the app's Cloud Storage
        /// </summary>
        public TMPro.TMP_InputField previewFilePath;

        public WorkshopItemEditorData Data
        {
            get => _mData;
            set
            {
                _mData = value;
                if (_mEvents != null)
                    _mEvents.onChange?.Invoke();
            }
        }

        private WorkshopItemEditorData _mData;
        private SteamWorkshopItemEditorDataEvents _mEvents;
        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates = new();

        private void Awake()
        {
            _mEvents = GetComponent<SteamWorkshopItemEditorDataEvents>();

            if (title != null)
                title.onValueChanged.AddListener(HandleTitleUpdate);

            if (description != null)
                description.onValueChanged.AddListener(HandleDescriptionUpdate);

            if (contentFolderPath != null)
                contentFolderPath.onValueChanged.AddListener(HandleContentFolderUpdate);

            if (previewFilePath != null)
                previewFilePath.onValueChanged.AddListener(HandlePreviewFileUpdate);
        }

        private void HandleTitleUpdate(string arg0)
        {
            _mData.title = arg0;
        }

        private void HandleDescriptionUpdate(string arg0)
        {
            _mData.description = arg0;
        }

        private void HandleContentFolderUpdate(string arg0)
        {
            _mData.Content = new(arg0);
        }

        private void HandlePreviewFileUpdate(string arg0)
        {
            _mData.Preview = new(arg0);
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(SteamWorkshopItemEditorData), true)]
    public class SteamWorkshopItemEditorDataEditor : ModularEditor
    {
        private SteamToolsSettings _settings;
        private SerializedProperty _title;
        private SerializedProperty _description;
        private SerializedProperty _contentFolderPath;
        private SerializedProperty _previewFilePath;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamWorkshopItemEditorCreateAndUpdate),
            typeof(SteamWorkshopItemEditorDataEvents),
        };

        private void OnEnable()
        {
            _settings = SteamToolsSettings.GetOrCreate();
            _title = serializedObject.FindProperty(nameof(SteamWorkshopItemEditorData.title));
            _description = serializedObject.FindProperty(nameof(SteamWorkshopItemEditorData.description));
            _contentFolderPath = serializedObject.FindProperty(nameof(SteamWorkshopItemEditorData.contentFolderPath));
            _previewFilePath = serializedObject.FindProperty(nameof(SteamWorkshopItemEditorData.previewFilePath));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefault(
                "Project/Player/Steamworks"
                , $"https://partner.steamgames.com/apps/landing/{_settings.Get(_settings.ActiveApp.Value).applicationId}"
                , "https://kb.heathen.group/steam/features/workshop"
                , "https://discord.gg/heathen-group-463483739612381204"
                , new[] { _title, _description, _contentFolderPath, _previewFilePath });

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif