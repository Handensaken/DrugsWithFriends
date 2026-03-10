#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Workshop Item")]
    [HelpURL("https://kb.heathen.group/steam/features/workshop")]
    public class SteamWorkshopItemDetailData : MonoBehaviour
    {
        public WorkshopItemDetails Data
        {
            get => _mData;
            set
            {
                _mData = value;
                if(_mEvents != null)
                    _mEvents.onChange?.Invoke();
            }
        }

        private WorkshopItemDetails _mData;
        private SteamWorkshopItemDetailDataEvents _mEvents;

        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates = new();

        private void Awake()
        {
            _mEvents = GetComponent<SteamWorkshopItemDetailDataEvents>();
        }

        public void Get(PublishedFileId_t fileId)
        {
            WorkshopItemDetails.Get(fileId, HandleItemGet);
        }

        public void LoadPreview()
        {
            if(_mEvents != null
                && _mData != null
                && _mData.SourceItemDetails.m_nPreviewFileSize > 0)
            {
                _mData.GetPreviewImage((name, data) =>
                {
                    _mEvents.onPreviewImageLoaded?.Invoke(data);
                });
            }
        }

        public void Subscribe()
        {
            if (_mData != null)
            {
                _mData.Subscribe(HandleSubscribed);
            }
        }

        public void Unsubscribe()
        {
            if (_mData != null)
            {
                _mData.Unsubscribe(HandleUnsubscribe);
            }
        }

        public void DownloadItem()
        {
            if (_mData != null)
            {
                _mData.DownloadItem(false);
            }
        }

        public void DownloadItemHighPriority()
        {
            if (_mData != null)
            {
                _mData.DownloadItem(true);
            }
        }

        public void Delete()
        {
            if (_mData != null)
            {
                _mData.DeleteItem(HandleItemDelete);
            }
        }

        public void UpVote()
        {
            if (_mData != null)
            {
                _mData.SetVote(true, HandleVoteSet);
            }
        }

        public void DownVote()
        {
            if (_mData != null)
            {
                _mData.SetVote(false, HandleVoteSet);
            }
        }

        public void StartPlaytime()
        {
            if (_mData != null)
            {
                _mData.StartPlayTime(HandleStartPlaytime);
            }
        }

        public void StopPlaytime()
        {
            if (_mData != null)
            {
                _mData.StartPlayTime(HandleEndPlaytime);
            }
        }

        private void HandleItemGet(WorkshopItemDetails details)
        {
            Data = details;
        }

        private void HandleStartPlaytime(StartPlaytimeTrackingResult_t t, bool arg2)
        {
            if (_mEvents != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    _mEvents.onPlayStartedFailed?.Invoke(t.m_eResult);
                else
                    _mEvents.onPlayStarted?.Invoke(_mData.FileId);
            }
        }

        private void HandleEndPlaytime(StartPlaytimeTrackingResult_t t, bool arg2)
        {
            if (_mEvents != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    _mEvents.onPlayEndedFailed?.Invoke(t.m_eResult);
                else
                    _mEvents.onPlayEnded?.Invoke(_mData.FileId);
            }
        }

        private void HandleVoteSet(SetUserItemVoteResult_t t, bool arg2)
        {
            if (_mEvents != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    _mEvents.onVoteSetFailed?.Invoke(t.m_eResult);
                else
                    _mEvents.onVoteSet?.Invoke(t.m_nPublishedFileId);
            }
        }

        private void HandleItemDelete(DeleteItemResult_t t, bool arg2)
        {
            if (_mEvents != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    _mEvents.onDeleteFailed?.Invoke(t.m_eResult);
                else
                    _mEvents.onDelete?.Invoke(t.m_nPublishedFileId);
            }
        }

        private void HandleUnsubscribe(RemoteStorageUnsubscribePublishedFileResult_t t, bool arg2)
        {
            if (_mEvents != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    _mEvents.onUnsubscribeFailed?.Invoke(t.m_eResult);
                else
                    _mEvents.onUnsubscribed?.Invoke(t.m_nPublishedFileId);
            }
        }

        private void HandleSubscribed(RemoteStorageSubscribePublishedFileResult_t t, bool arg2)
        {
            if (_mEvents != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    _mEvents.onSubscribeFailed?.Invoke(t.m_eResult);
                else
                    _mEvents.onSubscribed?.Invoke(t.m_nPublishedFileId);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteamWorkshopItemDetailData), true)]
    public class SteamWorkshopItemDetailDataEditor : ModularEditor
    {
        private SteamToolsSettings _settings;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamWorkshopItemDetailTitle),
            typeof(SteamWorkshopItemDetailDescription),
            // Preview Image isn't advisable because Unity cant handle the range of 
            // preview images Steam supports
            // Instead we provide WorkshopItemDetail.GetPreviewImage the dev needs to use a 3rd party image loader
            // to handle gif, bmp, etc.
            typeof(SteamWorkshopItemDetailRatingFill),
            typeof(SteamWorkshopItemDetailUpVoteLabel),
            typeof(SteamWorkshopItemDetailDownVoteLabel),
            typeof(SteamWorkshopItemDetailTotalVotesLabel),
            typeof(SteamWorkshopItemDetailCreatedData),
            typeof(SteamWorkshopItemDetailModifiedDate),
            typeof(SteamWorkshopItemDetailEdit),
            typeof(SteamWorkshopItemDetailDataEvents),
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
                , "https://kb.heathen.group/steam/features/workshop"
                , "https://discord.gg/heathen-group-463483739612381204"
                , null);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif