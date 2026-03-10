#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemEditorData), "Create & Update", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemEditorData))]
    public class SteamWorkshopItemEditorCreateAndUpdate : MonoBehaviour
    {
        /// <summary>
        /// Any metadata associated with the item, this is optional
        /// </summary>
        [SettingsField(0, false,"Optional")]
        public string metadata;
        /// <summary>
        /// The YouTube video ID of additional preview videos
        /// </summary>]
        [SettingsField(0, false,"Optional")]
        public string[] additionalYouTubeIds;
        /// <summary>
        /// Additional preview images
        /// </summary>
        [SettingsField(0, false,"Optional")]
        public WorkshopItemPreviewFile[] additionalPreviews;
        /// <summary>
        /// additional KVP tags
        /// </summary>
        [SettingsField(0, false,"Optional")]
        public WorkshopItemKeyValueTag[] additionalKeyValueTags;
        /// <summary>
        /// Any tags associated with the item, this is optional
        /// </summary>
        [SettingsField(0, false,"Optional")]
        public string[] tags;
        

        private SteamWorkshopItemEditorData _inspector;
        private SteamWorkshopItemEditorDataEvents _events;

        private void Awake()
        {
            _inspector = GetComponent<SteamWorkshopItemEditorData>();
            _events = GetComponent<SteamWorkshopItemEditorDataEvents>();
        }

        public void CreateNew()
        {
            var data = _inspector.Data;
            data.visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;
            data.metadata = metadata;
            data.tags = tags;
            _inspector.Data = data;

            data.Create(additionalPreviews, additionalYouTubeIds, additionalKeyValueTags, HandleCompleted, HandleUploaded, HandleFileCreated);
        }

        public void CreateOrUpdate()
        {
            var data = _inspector.Data;
            data.metadata = metadata;
            data.tags = tags;
            _inspector.Data = data;

            if (data.PublishedFileId.HasValue)
                data.Update(HandleUpdateCompleted, HandleUploaded);
            else
            {
                data.visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;
                data.Create(additionalPreviews, additionalYouTubeIds, additionalKeyValueTags, HandleCompleted, HandleUploaded, HandleFileCreated);
            }
        }

        private void HandleUpdateCompleted(WorkshopItemDataUpdateStatus status)
        {
            _inspector.Data = status.Data;

            if (status.HasError)
            {
                if (_events)
                {
                    EResult resultCode = EResult.k_EResultFail;
                    if (status.SubmitItemUpdateResult.HasValue)
                        resultCode = status.SubmitItemUpdateResult.Value.m_eResult;

                    _events.onCreateUpdateError?.Invoke(resultCode, status.ErrorMessage);
                }
            }
            else
            {
                if (_events)
                {
                    _events.onCreateUpdateSuccess?.Invoke();
                }
            }
        }

        private void HandleFileCreated(CreateItemResult_t t)
        {
            if (t.m_eResult == EResult.k_EResultOK)
            {
                var data = _inspector.Data;
                data.PublishedFileId = t.m_nPublishedFileId;
                _inspector.Data = data;
            }
        }

        private void HandleUploaded(UGCUpdateHandle_t t)
        {
            // Future use
        }

        private void HandleCompleted(WorkshopItemDataCreateStatus status)
        {
            _inspector.Data = status.Data;

            if(status.HasError)
            {
                var resultCode = EResult.k_EResultFail;
                if (status.SubmitItemUpdateResult.HasValue)
                    resultCode = status.SubmitItemUpdateResult.Value.m_eResult;
                else
                if (status.CreateItemResult.HasValue)
                    resultCode = status.CreateItemResult.Value.m_eResult;

                if (_events != null)
                    _events.onCreateUpdateError?.Invoke(resultCode, status.ErrorMessage);
            }
            else
            {
                if (_events != null)
                {
                    _events.onCreateUpdateSuccess?.Invoke();
                    if (status.CreateItemResult is { m_bUserNeedsToAcceptWorkshopLegalAgreement: true })
                        _events.onUserNeedsToAcceptWorkshopAgreement?.Invoke();
                }
            }
        }
    }
}
#endif