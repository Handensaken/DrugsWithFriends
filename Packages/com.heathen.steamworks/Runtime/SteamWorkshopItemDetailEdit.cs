#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System.IO;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemDetailData), "Edit", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailDataEvents))]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailEdit : MonoBehaviour
    {
        [SettingsField(0, false,"Editor")]
        public SteamWorkshopItemEditorData component;
        [SettingsField(0, false,"Quick Edits")]
        public TMPro.TMP_InputField changeNote;
        [SettingsField(0, false,"Quick Edits")]
        public TMPro.TMP_InputField title;
        [SettingsField(0, false,"Quick Edits")]
        public TMPro.TMP_InputField description;
        [SettingsField(0, false,"Quick Edits")]
        public TMPro.TMP_InputField contentFolder;
        [SettingsField(0, false,"Quick Edits")]
        public TMPro.TMP_InputField previewImageFile;
        [SettingsField(0, false,"Quick Edits")]
        public TMPro.TMP_InputField metadata;

        private SteamWorkshopItemDetailData _mInspector;
        private SteamWorkshopItemDetailDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamWorkshopItemDetailData>();
            _mEvents = GetComponent<SteamWorkshopItemDetailDataEvents>();

            _mEvents.onChange.AddListener(HandleOnChanged);
        }

        private void HandleOnChanged()
        {
            if (title != null)
                title.text = _mInspector.Data != null ? _mInspector.Data.Title : string.Empty;
            if (description != null)
                description.text = _mInspector.Data != null ? _mInspector.Data.Title : string.Empty;
            if (metadata != null)
                metadata.text = _mInspector.Data != null ? _mInspector.Data.Title : string.Empty;
        }

        private string GetChangeNote()
        {
            if(changeNote != null)
                return changeNote.text;
            else
                return string.Empty;
        }

        public void SetEditor()
        {
            if (_mInspector.Data != null && component != null)
            {
                component.Data = new()
                {
                    appId = _mInspector.Data.ConsumerApp,
                    description = _mInspector.Data.Description,
                    metadata = _mInspector.Data.metadata,
                    title = _mInspector.Data.Title,
                    PublishedFileId = _mInspector.Data.FileId,
                    visibility = _mInspector.Data.Visibility,
                    tags = _mInspector.Data.Tags,
                    Content = _mInspector.Data.FolderPath,
                };
            }
        }

        public void UpdateTitle()
        {
            if (_mInspector.Data != null
                && title != null
                && !string.IsNullOrEmpty(title.text))
            {
                _mInspector.Data.UpdateTitle(title.text, GetChangeNote(), HandleEditResult);
            }
        }

        public void UpdateDescription()
        {
            if (_mInspector.Data != null
                && description != null
                && !string.IsNullOrEmpty(description.text))
            {
                _mInspector.Data.UpdateDescription(description.text, GetChangeNote(), HandleEditResult);
            }
        }

        public void UpdateContent()
        {
            if (_mInspector.Data != null
                && contentFolder != null
                && !string.IsNullOrEmpty(contentFolder.text)
                && Directory.Exists(contentFolder.text))
            {
                _mInspector.Data.UpdateContent(new(contentFolder.text), GetChangeNote(), HandleEditResult);
            }
        }

        public void UpdatePreviewImage()
        {
            if (_mInspector.Data != null
                && previewImageFile != null
                && !string.IsNullOrEmpty(previewImageFile.text)
                && File.Exists(previewImageFile.text))
            {
                _mInspector.Data.UpdatePreviewImage(new(previewImageFile.text), GetChangeNote(), HandleEditResult);
            }
        }

        public void UpdateMetadata()
        {
            if (_mInspector.Data != null
                && metadata != null
                && !string.IsNullOrEmpty(metadata.text))
            {
                _mInspector.Data.UpdateMetadata(metadata.text, GetChangeNote(), HandleEditResult);
            }
        }

        public void UpdateAll()
        {
            if (_mInspector.Data == null)
                return;

            if ((title == null || string.IsNullOrEmpty(title.text))
                && (description == null || string.IsNullOrEmpty(description.text))
                && (contentFolder == null || string.IsNullOrEmpty(contentFolder.text) || !Directory.Exists(contentFolder.text))
                && (previewImageFile == null || string.IsNullOrEmpty(previewImageFile.text) || !File.Exists(previewImageFile.text))
                && (metadata == null || string.IsNullOrEmpty(metadata.text)))
                return;

            var handle = API.UserGeneratedContent.Client.StartItemUpdate(_mInspector.Data.ConsumerApp, _mInspector.Data.FileId);

            if (title != null && !string.IsNullOrEmpty(title.text))
                SteamUGC.SetItemTitle(handle, title.text);

            if (description != null && !string.IsNullOrEmpty(description.text))
                SteamUGC.SetItemDescription(handle, description.text);

            if (contentFolder != null && !string.IsNullOrEmpty(contentFolder.text) && Directory.Exists(contentFolder.text))
                SteamUGC.SetItemContent(handle, new DirectoryInfo(contentFolder.text).FullName);

            if (previewImageFile != null && !string.IsNullOrEmpty(previewImageFile.text) && File.Exists(previewImageFile.text))
                SteamUGC.SetItemPreview(handle, new FileInfo(previewImageFile.text).FullName);

            if (metadata != null && !string.IsNullOrEmpty(metadata.text))
                SteamUGC.SetItemMetadata(handle, metadata.text);


            API.UserGeneratedContent.Client.SubmitItemUpdate(handle, GetChangeNote(), HandleEditResult);
            
        }

        private void HandleEditResult(SubmitItemUpdateResult_t t, bool arg2)
        {
            if(_mEvents != null)
            {
                if (!arg2 && t.m_eResult == EResult.k_EResultOK)
                    _mEvents.onEdited?.Invoke(t.m_nPublishedFileId);
                else
                    _mEvents.onEditFailed?.Invoke(t.m_eResult);
            }
        }
    }
}
#endif