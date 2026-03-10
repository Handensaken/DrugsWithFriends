#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides static methods and events for interacting with Steamworks User Generated Content (UGC) systems.
    /// </summary>
    public static class UserGeneratedContent
    {
        /// <summary>
        /// Checks if the specified <paramref name="checkflag"/> value is present in the 'value'.
        /// </summary>
        /// <param name="value">The item state to check against.</param>
        /// <param name="checkflag">The specific state flag to test for.</param>
        /// <returns>Returns true if the <paramref name="checkflag"/> is present in the 'value'; otherwise, returns false.</returns>
        public static bool ItemStateHasFlag(EItemState value, EItemState checkflag)
        {
            return (value & checkflag) == checkflag;
        }

        /// <summary>
        /// Checks if all the specified flags in <paramref name="checkflags"/> are present in the 'value'.
        /// </summary>
        /// <param name="value">The item state to check against.</param>
        /// <param name="checkflags">An array of state flags to verify in the 'value'.</param>
        /// <returns>Returns true if all the <paramref name="checkflags"/> are present in the 'value'; otherwise, returns false.</returns>
        public static bool ItemStateHasAllFlags(EItemState value, params EItemState[] checkflags)
        {
            return checkflags.All(checkflag => (value & checkflag) == checkflag);
        }

        /// <summary>
        /// Provides functionality for interacting with Steam Workshop client-side operations,
        /// including item creation, updates, queries, and dependency management within the Steamworks UGC system.
        /// </summary>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                _evtItemDownloaded = new WorkshopDownloadedItemResultEvent();
                _evtItemInstalled = new WorkshopItemInstalledEvent();

                ImageLoadRequests.Clear();

                _addAppDependencyResults = null;
                _addUgcDependencyResults = null;
                _userFavoriteItemsListChanged = null;
                _createdItem = null;
                _deleteItem = null;
                _appDependenciesResult = null;
                _getUserItemVoteResult = null;
                _removeAppDependencyResult = null;
                _removeDependencyResult = null;
                _steamUgcRequestUgcDetailsResult = null;
                _steamUgcQueryCompleted = null;
                _setUserItemVoteResult = null;
                _startPlaytimeTrackingResult = null;
                _stopPlaytimeTrackingResult = null;
                _submitItemUpdateResult = null;
                _remoteStorageSubscribePublishedFileResult = null;
                _remoteStorageUnsubscribePublishedFileResult = null;
                _workshopEulaStatus = null;
                _remoteStorageDownloadUgcResult = null;
                _downloadItem = null;
                _itemInstalled = null;
            }

            private static WorkshopDownloadedItemResultEvent _evtItemDownloaded = new WorkshopDownloadedItemResultEvent();
            private static WorkshopItemInstalledEvent _evtItemInstalled = new WorkshopItemInstalledEvent();

            private static CallResult<AddAppDependencyResult_t> _addAppDependencyResults;
            private static CallResult<AddUGCDependencyResult_t> _addUgcDependencyResults;
            private static CallResult<UserFavoriteItemsListChanged_t> _userFavoriteItemsListChanged;
            private static CallResult<CreateItemResult_t> _createdItem;
            private static CallResult<DeleteItemResult_t> _deleteItem;
            private static CallResult<GetAppDependenciesResult_t> _appDependenciesResult;
            private static CallResult<GetUserItemVoteResult_t> _getUserItemVoteResult;
            private static CallResult<RemoveAppDependencyResult_t> _removeAppDependencyResult;
            private static CallResult<RemoveUGCDependencyResult_t> _removeDependencyResult;
            private static CallResult<SteamUGCRequestUGCDetailsResult_t> _steamUgcRequestUgcDetailsResult;
            private static CallResult<SteamUGCQueryCompleted_t> _steamUgcQueryCompleted;
            private static CallResult<SetUserItemVoteResult_t> _setUserItemVoteResult;
            private static CallResult<StartPlaytimeTrackingResult_t> _startPlaytimeTrackingResult;
            private static CallResult<StopPlaytimeTrackingResult_t> _stopPlaytimeTrackingResult;
            private static CallResult<SubmitItemUpdateResult_t> _submitItemUpdateResult;
            private static CallResult<RemoteStorageSubscribePublishedFileResult_t> _remoteStorageSubscribePublishedFileResult;
            private static CallResult<RemoteStorageUnsubscribePublishedFileResult_t> _remoteStorageUnsubscribePublishedFileResult;
            private static CallResult<WorkshopEULAStatus_t> _workshopEulaStatus;
            private static CallResult<RemoteStorageDownloadUGCResult_t> _remoteStorageDownloadUgcResult;

            private static Callback<DownloadItemResult_t> _downloadItem;
            private static Callback<ItemInstalled_t> _itemInstalled;

            /// <summary>
            /// Represents image data associated with a user-generated content (UGC) item in the Steamworks system.
            /// </summary>
            public class ImageData
            {
                /// <summary>
                /// Represents the file path associated with the image data of a user-generated content (UGC) item in the Steamworks system.
                /// </summary>
                public string Path;

                /// <summary>
                /// Represents the raw byte array data of an image texture associated with a user-generated content (UGC) item in the Steamworks system.
                /// Typically used to hold image data that can be rendered or processed further.
                /// </summary>
                public byte[] Texture;
            }

            private static readonly Dictionary<ulong, ImageData> MLoadedImages = new();

            #region Events

            /// <summary>
            /// Event triggered when a workshop item has been successfully downloaded.
            /// </summary>
            /// <remarks>
            /// This represents a callback for the Steamworks API when a UGC item is downloaded.
            /// It provides access to the details of the download operation via the <see cref="DownloadItemResult_t"/> structure.
            /// </remarks>
            public static WorkshopDownloadedItemResultEvent OnItemDownloaded
            {
                get
                {
                    _downloadItem ??= Callback<DownloadItemResult_t>.Create(_evtItemDownloaded.Invoke);

                    return _evtItemDownloaded;
                }
            }

            /// <summary>
            /// Event triggered when a Workshop item is installed for the client in the Steamworks system.
            /// This can be used to handle post-installation actions or updates related to the installed item.
            /// </summary>
            public static WorkshopItemInstalledEvent OnWorkshopItemInstalled
            {
                get
                {
                    _itemInstalled ??= Callback<ItemInstalled_t>.Create(_evtItemInstalled.Invoke);

                    return _evtItemInstalled;
                }
            }

            #endregion

            #region Workshop System

            private struct ImageLoadRequest
            {
                public UGCHandle_t ImageFile;
                public Action<string, byte[]> Callback;
            }

            private static readonly Queue<ImageLoadRequest> ImageLoadRequests = new();
            private static bool _imageProcessing;

            /// <summary>
            /// Retrieves an image associated with the specified UGC (User-Generated Content) handle asynchronously.
            /// </summary>
            /// <param name="imageFile">The handle representing the image file in the UGC system.</param>
            /// <param name="callback">
            /// A callback function that receives the path to the image and its texture data as a byte array.
            /// The callback is invoked when the image is available.
            /// </param>
            public static void GetUgcImage(UGCHandle_t imageFile, Action<string, byte[]> callback)
            {
                if (callback == null)
                    return;

                if (MLoadedImages.TryGetValue(imageFile.m_UGCHandle, out var data))
                    callback(data.Path, data.Texture);
                else
                {
                    try
                    {
                        Debug.Log($"Enqueue image: {imageFile}");
                        ImageLoadRequests.Enqueue(new() { ImageFile = imageFile, Callback = callback });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"UGC Download: Exception = {ex.Message}");
                    }
                }
            }

            internal static void ImageWorker_Tick()
            {
                if (_imageProcessing || ImageLoadRequests.Count == 0)
                    return;

                var currentRequest = ImageLoadRequests.Dequeue();
                if (currentRequest.Callback == null)
                    return;

                _imageProcessing = true;

                _remoteStorageDownloadUgcResult ??= CallResult<RemoteStorageDownloadUGCResult_t>.Create();

                var previewCall = SteamRemoteStorage.UGCDownload(currentRequest.ImageFile, 1);

                _remoteStorageDownloadUgcResult.Set(previewCall, (result, ioError) =>
                {
                    try
                    {
                        if (ioError || result.m_eResult != EResult.k_EResultOK)
                        {
                            Debug.LogError($"UGC Download failed: {result.m_eResult}, ioError: {ioError}. No file loaded.");
                            return;
                        }

                        int totalSize = result.m_nSizeInBytes;
                        if (totalSize <= 0)
                        {
                            Debug.LogError("UGC file size is 0, skipping.");
                            return;
                        }

                        var buffer = new byte[totalSize];

                        SteamRemoteStorage.UGCRead(
                            result.m_hFile,
                            buffer,
                            totalSize,
                            0,
                            EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);
                        SteamRemoteStorage.UGCRead(result.m_hFile, null, 0, 0, EUGCReadAction.k_EUGCRead_Close);

                        MLoadedImages.TryAdd(currentRequest.ImageFile.m_UGCHandle, new() { Path = result.m_pchFileName, Texture = buffer });
                        currentRequest.Callback(result.m_pchFileName, buffer);
                    }
                    finally
                    {
                        _imageProcessing = false;
                    }
                });
            }

            /// <summary>
            /// Creates a new workshop item and handles its initialisation, status updates, and callbacks.
            /// </summary>
            /// <param name="item">The editor data defining the workshop item being created.</param>
            /// <param name="additionalPreviews">An array of additional preview files to attach to the workshop item.</param>
            /// <param name="additionalYouTubeIds">An array of YouTube video IDs to be associated with the workshop item.</param>
            /// <param name="additionalKeyValueTags">An array of key-value tags to be added to the workshop item.</param>
            /// <param name="completedCallback">An optional callback invoked upon completion of the item creation process, providing creation status details.</param>
            /// <param name="uploadStartedCallback">An optional callback triggered when the upload process for the workshop item begins, providing the upload handle.</param>
            /// <param name="fileCreatedCallback">An optional callback triggered when the workshop item file is successfully created, providing the creation result.</param>
            /// <returns>Returns true if the item creation workflow is initiated successfully.</returns>
            public static bool CreateItem(WorkshopItemEditorData item, WorkshopItemPreviewFile[] additionalPreviews, string[] additionalYouTubeIds, WorkshopItemKeyValueTag[] additionalKeyValueTags, Action<WorkshopItemDataCreateStatus> completedCallback = null, Action<UGCUpdateHandle_t> uploadStartedCallback = null, Action<CreateItemResult_t> fileCreatedCallback = null)
            {
                _createdItem ??= CallResult<CreateItemResult_t>.Create();

                _submitItemUpdateResult ??= CallResult<SubmitItemUpdateResult_t>.Create();

                var call = SteamUGC.CreateItem(item.appId, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
                _createdItem.Set(call, (createResult, createIOError) =>
                {
                    if (createIOError || createResult.m_eResult != EResult.k_EResultOK)
                    { 
                        if (createIOError)
                        {
                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                            {
                                HasError = true,
                                ErrorMessage = "Steamworks Client failed to create UGC item.",
                                CreateItemResult = createResult,
                            });
                        }
                        else
                        {
                            switch(createResult.m_eResult)
                            {
                                case EResult.k_EResultInsufficientPrivilege:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "The user is currently restricted from uploading content due to a hub ban, account lock, or community ban. They would need to contact Steam Support.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultBanned:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "The user doesn't have permission to upload content to this hub because they have an active VAC or Game ban.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultTimeout:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "The operation took longer than expected. Have the user retry the creation process.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultNotLoggedOn:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "The user is not currently logged into Steam.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultServiceUnavailable:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "The workshop server hosting the content is having issues - have the user retry.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultInvalidParam:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "One of the submission fields contains something not being accepted by that field.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultAccessDenied:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "There was a problem trying to save the title and description. Access was denied.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultLimitExceeded:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "The user has exceeded their Steam Cloud quota. Have them remove some items and try again.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultFileNotFound:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "The uploaded file could not be found.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultDuplicateRequest:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "The file was already successfully uploaded. The user just needs to refresh.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultDuplicateName:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "The user already has a Steam Workshop item with that name.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultServiceReadOnly:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "Due to a recent password or email change, the user is not allowed to upload new content. Usually this restriction will expire in 5 days, but can last up to 30 days if the account has been inactive recently.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                                default:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = "Unexpected result please see the createItemResult.m_eResult status for more information.",
                                        CreateItemResult = createResult,
                                    });
                                    break;
                            }
                        }
                    }
                    else
                    {
                        fileCreatedCallback?.Invoke(createResult);
                        var updateHandle = SteamUGC.StartItemUpdate(item.appId, createResult.m_nPublishedFileId);
                        var hasError = false;
                        var sb = new System.Text.StringBuilder();

                        if (!string.IsNullOrEmpty(item.title))
                        {
                            if (!SteamUGC.SetItemTitle(updateHandle, item.title))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item title.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("The title was not provided and is required; the update might be rejected by Valve");
                        }

                        if (!string.IsNullOrEmpty(item.description))
                        {
                            if (!SteamUGC.SetItemDescription(updateHandle, item.description))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item description.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("The description was not provided and is required; the update might be rejected by Valve");
                        }

                        if (!SteamUGC.SetItemVisibility(updateHandle, item.visibility))
                        {
                            hasError = true;
                            if (sb.Length > 0)
                                sb.Append("\n");
                            sb.Append("Failed to update item visibility.");
                        }

                        if (item.tags != null && item.tags.Any())
                        {
                            if (!SteamUGC.SetItemTags(updateHandle, item.tags.ToList()))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item tags.");
                            }
                        }

                        if (item.Content is { Exists: true })
                        {
                            if (!SteamUGC.SetItemContent(updateHandle, item.Content.FullName))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item content location.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("The content folder does not exist and is required; the update might be rejected by Valve");
                        }

                        if (item.Preview is { Exists: true })
                        {
                            if (!SteamUGC.SetItemPreview(updateHandle, item.Preview.FullName))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item preview.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("The preview image does not exist and is required; the update might be rejected by Valve");
                        }

                        if (additionalPreviews is { Length: > 0 })
                        {
                            foreach (var previewFile in additionalPreviews)
                            {
                                if (!SteamUGC.AddItemPreviewFile(updateHandle, previewFile.source, previewFile.type))
                                {
                                    hasError = true;
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Failed to add item preview: " + previewFile.source + ".");
                                }
                            }
                        }

                        if (additionalYouTubeIds is { Length: > 0 })
                        {
                            foreach (var video in additionalYouTubeIds)
                            {
                                if (!SteamUGC.AddItemPreviewVideo(updateHandle, video))
                                {
                                    hasError = true;
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Failed to add item video: " + video + ".");
                                }
                            }
                        }

                        if (additionalKeyValueTags is { Length: > 0 })
                        {
                            foreach (var tag in additionalKeyValueTags)
                            {
                                if (!SteamUGC.AddItemKeyValueTag(updateHandle, tag.key, tag.value))
                                {
                                    hasError = true;
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Failed to add item key value tag: " + tag.key + ":" + tag.value);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(item.metadata))
                        {
                            if (!SteamUGC.SetItemMetadata(updateHandle, item.metadata))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item metadata.");
                            }
                        }

                        var siu = SteamUGC.SubmitItemUpdate(updateHandle, "Initial Creation");
                        _submitItemUpdateResult.Set(siu, (updateResult, updateIOError) =>
                        {
                            if (updateIOError || updateResult.m_eResult != EResult.k_EResultOK)
                            {
                                hasError = true;

                                if (updateIOError)
                                {
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Steamworks Client failed to submit item updates.");

                                    item.PublishedFileId = createResult.m_nPublishedFileId;
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = sb.ToString(),
                                        Data = item,
                                        CreateItemResult = createResult,
                                        SubmitItemUpdateResult = updateResult,
                                    });
                                }
                                else
                                {
                                    switch(updateResult.m_eResult)
                                    {
                                        case EResult.k_EResultFail:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("Generic failure.");

                                            item.PublishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                HasError = true,
                                                ErrorMessage = sb.ToString(),
                                                Data = item,
                                                CreateItemResult = createResult,
                                                SubmitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        case EResult.k_EResultInvalidParam:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("Either the provided app ID is invalid or doesn't match the consumer app ID of the item or, you have not enabled ISteamUGC for the provided app ID on the Steam Workshop Configuration App Admin page.\nThe preview file is smaller than 16 bytes.");

                                            item.PublishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                HasError = true,
                                                ErrorMessage = sb.ToString(),
                                                Data = item,
                                                CreateItemResult = createResult,
                                                SubmitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        case EResult.k_EResultAccessDenied:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("The user doesn't own a license for the provided app ID.");

                                            item.PublishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                HasError = true,
                                                ErrorMessage = sb.ToString(),
                                                Data = item,
                                                CreateItemResult = createResult,
                                                SubmitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        case EResult.k_EResultFileNotFound:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("Failed to get the workshop info for the item or failed to read the preview file or the content folder is not valid.");

                                            item.PublishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                HasError = true,
                                                ErrorMessage = sb.ToString(),
                                                Data = item,
                                                CreateItemResult = createResult,
                                                SubmitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        case EResult.k_EResultLockingFailed:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("Failed to acquire UGC Lock.");

                                            item.PublishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                HasError = true,
                                                ErrorMessage = sb.ToString(),
                                                Data = item,
                                                CreateItemResult = createResult,
                                                SubmitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        case EResult.k_EResultLimitExceeded:

                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("The preview image is too large, it must be less than 1 Megabyte; or there is not enough space available on the users Steam Cloud.");

                                            item.PublishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                HasError = true,
                                                ErrorMessage = sb.ToString(),
                                                Data = item,
                                                CreateItemResult = createResult,
                                                SubmitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        default:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("Unexpected status message from Steam client, please see the submitItemUpdateResult.m_eResult status for more information.");

                                            item.PublishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                HasError = true,
                                                ErrorMessage = sb.ToString(),
                                                Data = item,
                                                CreateItemResult = createResult,
                                                SubmitItemUpdateResult = updateResult,
                                            });
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                item.PublishedFileId = createResult.m_nPublishedFileId;
                                completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                {
                                    HasError = hasError,
                                    ErrorMessage = hasError ? sb.ToString() : string.Empty,
                                    Data = item,
                                    CreateItemResult = createResult,
                                    SubmitItemUpdateResult = updateResult,
                                });
                            }
                        });
                        uploadStartedCallback?.Invoke(updateHandle);
                    }
                });

                return true;
            }

            /// <summary>
            /// Updates a Workshop item with the specified data and additional parameters.
            /// </summary>
            /// <param name="item">The Workshop item editor data containing properties to update.</param>
            /// <param name="additionalPreviews">An array of additional preview files to associate with the item.</param>
            /// <param name="additionalYouTubeIds">An array of additional YouTube video IDs to link to the item.</param>
            /// <param name="additionalKeyValueTags">An array of custom key-value tags to add to the item.</param>
            /// <param name="callback">An optional callback to invoke with the update status once the update process is completed.</param>
            /// <param name="uploadStartedCallback">An optional callback to invoke when the upload process begins.</param>
            /// <returns>Returns true if the update process was successfully started; otherwise, returns false.</returns>
            public static bool UpdateItem(WorkshopItemEditorData item, WorkshopItemPreviewFile[] additionalPreviews, string[] additionalYouTubeIds, WorkshopItemKeyValueTag[] additionalKeyValueTags, Action<WorkshopItemDataUpdateStatus> callback = null, Action<UGCUpdateHandle_t> uploadStartedCallback = null)
            {
                _createdItem ??= CallResult<CreateItemResult_t>.Create();

                _submitItemUpdateResult ??= CallResult<SubmitItemUpdateResult_t>.Create();

                if (!item.PublishedFileId.HasValue)
                    return false;

                var updateHandle = SteamUGC.StartItemUpdate(item.appId, item.PublishedFileId.Value);
                var hasError = false;
                var sb = new System.Text.StringBuilder();
                if (!SteamUGC.SetItemTitle(updateHandle, item.title))
                {
                    hasError = true;
                    if (sb.Length > 0)
                        sb.Append("\n");
                    sb.Append("Failed to update item title.");
                }

                if (!string.IsNullOrEmpty(item.description))
                {
                    if (!SteamUGC.SetItemDescription(updateHandle, item.description))
                    {
                        hasError = true;
                        if (sb.Length > 0)
                            sb.Append("\n");
                        sb.Append("Failed to update item description.");
                    }
                }

                if (!SteamUGC.SetItemVisibility(updateHandle, item.visibility))
                {
                    hasError = true;
                    if (sb.Length > 0)
                        sb.Append("\n");
                    sb.Append("Failed to update item visibility.");
                }

                if (item.tags != null && item.tags.Any())
                {
                    if (!SteamUGC.SetItemTags(updateHandle, item.tags.ToList()))
                    {
                        hasError = true;
                        if (sb.Length > 0)
                            sb.Append("\n");
                        sb.Append("Failed to update item tags.");
                    }
                }

                if (!SteamUGC.SetItemContent(updateHandle, item.Content.FullName))
                {
                    hasError = true;
                    if (sb.Length > 0)
                        sb.Append("\n");
                    sb.Append("Failed to update item content location.");
                }

                if (!SteamUGC.SetItemPreview(updateHandle, item.Preview.FullName))
                {
                    hasError = true;
                    if (sb.Length > 0)
                        sb.Append("\n");
                    sb.Append("Failed to update item preview.");
                }

                if (additionalPreviews is { Length: > 0 })
                {
                    foreach (var previewFile in additionalPreviews)
                    {
                        if (!SteamUGC.AddItemPreviewFile(updateHandle, previewFile.source, previewFile.type))
                        {
                            hasError = true;
                            if (sb.Length > 0)
                                sb.Append("\n");
                            sb.Append("Failed to add item preview: " + previewFile.source + ".");
                        }
                    }
                }

                if (additionalYouTubeIds is { Length: > 0 })
                {
                    foreach (var video in additionalYouTubeIds)
                    {
                        if (!SteamUGC.AddItemPreviewVideo(updateHandle, video))
                        {
                            hasError = true;
                            if (sb.Length > 0)
                                sb.Append("\n");
                            sb.Append("Failed to add item video: " + video + ".");
                        }
                    }
                }

                if (additionalKeyValueTags is { Length: > 0 })
                {
                    foreach (var tag in additionalKeyValueTags)
                    {
                        if (!SteamUGC.AddItemKeyValueTag(updateHandle, tag.key, tag.value))
                        {
                            hasError = true;
                            if (sb.Length > 0)
                                sb.Append("\n");
                            sb.Append("Failed to add item key value tag: " + tag.key + ":" + tag.value);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(item.metadata))
                {
                    if (!SteamUGC.SetItemMetadata(updateHandle, item.metadata))
                    {
                        hasError = true;
                        if (sb.Length > 0)
                            sb.Append("\n");
                        sb.Append("Failed to update item metadata.");
                    }
                }

                var siu = SteamUGC.SubmitItemUpdate(updateHandle, "Initial Creation");
                _submitItemUpdateResult.Set(siu, (updateResult, updateIOError) =>
                {
                    if (updateIOError || updateResult.m_eResult != EResult.k_EResultOK)
                    {
                        hasError = true;

                        if (updateIOError)
                        {
                            if (sb.Length > 0)
                                sb.Append("\n");
                            sb.Append("Steamworks Client failed to submit item updates.");

                            callback?.Invoke(new WorkshopItemDataUpdateStatus
                            {
                                HasError = true,
                                ErrorMessage = sb.ToString(),
                                Data = item,
                                SubmitItemUpdateResult = updateResult,
                            });
                        }
                        else
                        {
                            switch (updateResult.m_eResult)
                            {
                                case EResult.k_EResultFail:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Generic failure.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = sb.ToString(),
                                        Data = item,
                                        SubmitItemUpdateResult = updateResult,
                                    });
                                    break;
                                case EResult.k_EResultInvalidParam:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Either the provided app ID is invalid or doesn't match the consumer app ID of the item or, you have not enabled ISteamUGC for the provided app ID on the Steam Workshop Configuration App Admin page.\nThe preview file is smaller than 16 bytes.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = sb.ToString(),
                                        Data = item,
                                        SubmitItemUpdateResult = updateResult,
                                    });
                                    break;
                                case EResult.k_EResultAccessDenied:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("The user doesn't own a license for the provided app ID.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = sb.ToString(),
                                        Data = item,
                                        SubmitItemUpdateResult = updateResult,
                                    });
                                    break;
                                case EResult.k_EResultFileNotFound:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Failed to get the workshop info for the item or failed to read the preview file or the content folder is not valid.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = sb.ToString(),
                                        Data = item,
                                        SubmitItemUpdateResult = updateResult,
                                    });
                                    break;
                                case EResult.k_EResultLockingFailed:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Failed to acquire UGC Lock.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = sb.ToString(),
                                        Data = item,
                                        SubmitItemUpdateResult = updateResult,
                                    });
                                    break;
                                case EResult.k_EResultLimitExceeded:

                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("The preview image is too large, it must be less than 1 Megabyte; or there is not enough space available on the users Steam Cloud.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = sb.ToString(),
                                        Data = item,
                                        SubmitItemUpdateResult = updateResult,
                                    });
                                    break;
                                default:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Unexpected status message from Steam client, please see the submitItemUpdateResult.m_eResult status for more information.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        HasError = true,
                                        ErrorMessage = sb.ToString(),
                                        Data = item,
                                        SubmitItemUpdateResult = updateResult,
                                    });
                                    break;
                            }
                        }
                    }
                    else
                    {
                        callback?.Invoke(new WorkshopItemDataUpdateStatus
                        {
                            HasError = hasError,
                            ErrorMessage = hasError ? sb.ToString() : string.Empty,
                            Data = item,
                            SubmitItemUpdateResult = updateResult,
                        });
                    }
                });
                uploadStartedCallback?.Invoke(updateHandle);
                return true;
            }

            /// <summary>
            /// Adds a dependency between the specified item and the given app ID. This creates a soft dependency that is visible on the web, but the application is responsible for determining whether the item can be used based on this relationship.
            /// </summary>
            /// <param name="fileId">The ID of the published file to which the dependency will be added.</param>
            /// <param name="appId">The ID of the app to be set as a dependency for the published file.</param>
            /// <param name="callback">A callback function to be invoked when the operation completes, providing the result of the operation and a boolean indicating success or failure.</param>
            public static void AddAppDependency(PublishedFileId_t fileId, AppId_t appId,
                Action<AddAppDependencyResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _addAppDependencyResults ??= CallResult<AddAppDependencyResult_t>.Create();

                var call = SteamUGC.AddAppDependency(fileId, appId);
                _addAppDependencyResults.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Adds a workshop item as a dependency to the specified parent workshop item.
            /// </summary>
            /// <param name="parentFileId">The published file ID of the parent workshop item.</param>
            /// <param name="childFileId">The published file ID of the child workshop item to be added as a dependency.</param>
            /// <param name="callback">The callback action to handle the result of the operation, including the success status and result details.</param>
            public static void AddDependency(PublishedFileId_t parentFileId, PublishedFileId_t childFileId,
                Action<AddUGCDependencyResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _addUgcDependencyResults ??= CallResult<AddUGCDependencyResult_t>.Create();

                var call = SteamUGC.AddDependency(parentFileId, childFileId);
                _addUgcDependencyResults.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Adds an excluded tag to a UGC query, limiting the query results to exclude items with the specified tag.
            /// </summary>
            /// <param name="handle">The UGC query handle to which the excluded tag will be added.</param>
            /// <param name="tagName">The name of the tag to exclude from the query results.</param>
            /// <returns>Returns true if the tag was successfully added to the query; otherwise, returns false.</returns>
            public static bool AddExcludedTag(UGCQueryHandle_t handle, string tagName) =>
                SteamUGC.AddExcludedTag(handle, tagName);

            /// <summary>
            /// Adds a key-value tag pair to a workshop item during an update operation. Keys can map to multiple values in a one-to-many relationship. Key names must contain only alphanumeric characters or underscores, and the length of both keys and values must not exceed 255 characters. An exact match performs searches for key-value tags.
            /// </summary>
            /// <param name="handle">The update handle associated with the item being customised.</param>
            /// <param name="key">The key to associate with the specified value.</param>
            /// <param name="value">The value to map to the specified key.</param>
            /// <returns>Returns true if the key-value pair was successfully added; otherwise, returns false.</returns>
            public static bool AddItemKeyValueTag(UGCUpdateHandle_t handle, string key, string value) =>
                SteamUGC.AddItemKeyValueTag(handle, key, value);

            /// <summary>
            /// Adds a preview file to a Steam Workshop item during an update.
            /// The preview file must be under 1MB and should be in a format suitable for rendering both on the web and in the application (e.g. JPG, PNG, or GIF).
            /// </summary>
            /// <param name="handle">The update handle associated with the Workshop item.</param>
            /// <param name="previewFile">The file path to the preview file to be added.</param>
            /// <param name="type">The type of preview file, defined by the <see cref="EItemPreviewType"/> enumeration.</param>
            /// <returns>Returns true if the preview file is successfully added; otherwise, returns false.</returns>
            public static bool
                AddItemPreviewFile(UGCUpdateHandle_t handle, string previewFile, EItemPreviewType type) =>
                SteamUGC.AddItemPreviewFile(handle, previewFile, type);

            /// <summary>
            /// Adds a YouTube video as a preview to the specified workshop item.
            /// </summary>
            /// <param name="handle">The update handle associated with the item being modified.</param>
            /// <param name="videoId">The unique identifier of the YouTube video to be added as a preview.</param>
            /// <returns>Returns true if the video is successfully added as a preview; otherwise, returns false.</returns>
            public static bool AddItemPreviewVideo(UGCUpdateHandle_t handle, string videoId) =>
                SteamUGC.AddItemPreviewVideo(handle, videoId);

            /// <summary>
            /// Adds a workshop item to the user's favorites list.
            /// </summary>
            /// <param name="appId">The application ID associated with the workshop item.</param>
            /// <param name="fileId">The unique identifier of the workshop item to be added to favorites.</param>
            /// <param name="callback">Callback to handle the result of the operation, providing the updated favorite items list and a success flag.</param>
            public static void AddItemToFavorites(AppId_t appId, PublishedFileId_t fileId,
                Action<UserFavoriteItemsListChanged_t, bool> callback)
            {
                if (callback == null)
                    return;

                _userFavoriteItemsListChanged ??= CallResult<UserFavoriteItemsListChanged_t>.Create();

                var call = SteamUGC.AddItemToFavorites(appId, fileId);
                _userFavoriteItemsListChanged.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Adds a required key-value tag to a UGC query filter. Only workshop items with the specified key and value will match the query.
            /// </summary>
            /// <param name="handle">The handle representing the UGC query to which the tag will be added.</param>
            /// <param name="key">The key of the required tag to filter items by.</param>
            /// <param name="value">The value of the required tag to filter items by.</param>
            /// <returns>Returns true if the tag was successfully added to the UGC query; otherwise, returns false.</returns>
            public static bool AddRequiredKeyValueTag(UGCQueryHandle_t handle, string key, string value) =>
                SteamUGC.AddRequiredKeyValueTag(handle, key, value);

            /// <summary>
            /// Adds a required tag to a UGC query, filtering results to include only UGC items with the specified tag.
            /// </summary>
            /// <param name="handle">The handle of the UGC query to which the tag will be added.</param>
            /// <param name="tagName">The required tag to filter the UGC query results.</param>
            /// <returns>Returns true if the tag was successfully added; otherwise, returns false.</returns>
            public static bool AddRequiredTag(UGCQueryHandle_t handle, string tagName) =>
                SteamUGC.AddRequiredTag(handle, tagName);

            /// <summary>
            /// Creates an empty workshop item associated with the specified application ID and file type.
            /// </summary>
            /// <param name="appId">The application ID to associate the workshop item with.</param>
            /// <param name="type">The type of the workshop file being created.</param>
            /// <param name="callback">A callback to handle the result of the item creation operation, providing the creation result and success status.</param>
            public static void CreateItem(AppId_t appId, EWorkshopFileType type,
                Action<CreateItemResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _createdItem ??= CallResult<CreateItemResult_t>.Create();

                var call = SteamUGC.CreateItem(appId, type);
                _createdItem.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Creates a query request to retrieve all user-generated content (UGC) matching the specified criteria.
            /// The returned query handle must be released by calling WorkshopReleaseQueryRequest when no longer needed.
            /// </summary>
            /// <param name="queryType">The type of query to perform, determining how the results are filtered and sorted.</param>
            /// <param name="matchingFileType">The type of UGC files to include in the query results.</param>
            /// <param name="creatorAppId">The App ID of the application that created the content. Use 0 to match any creator application.</param>
            /// <param name="consumerAppId">The App ID of the application consuming the content. This is typically your application ID.</param>
            /// <param name="page">The page of results to fetch, starting at 1 for the first page of results.</param>
            /// <returns>Returns a handle to the created UGC query, which can be used to execute and retrieve the results of the query.</returns>
            public static UGCQueryHandle_t CreateQueryAllRequest(EUGCQuery queryType,
                EUGCMatchingUGCType matchingFileType, AppId_t creatorAppId, AppId_t consumerAppId, uint page) =>
                SteamUGC.CreateQueryAllUGCRequest(queryType, matchingFileType, creatorAppId, consumerAppId, page);

            /// <summary>
            /// Creates a query request to retrieve details of specific workshop items.
            /// The returned handle must be released by invoking the WorkshopReleaseQueryRequest method when it is no longer necessary.
            /// </summary>
            /// <param name="fileIds">An array of unique identifiers representing the workshop items to retrieve details for.</param>
            /// <returns>Returns a handle of the type UGCQueryHandle_t, which represents the query request for the specified workshop items.</returns>
            public static UGCQueryHandle_t CreateQueryDetailsRequest(PublishedFileId_t[] fileIds) =>
                SteamUGC.CreateQueryUGCDetailsRequest(fileIds, (uint)fileIds.GetLength(0));

            /// <summary>
            /// Creates a query request to retrieve details for specific workshop items.
            /// The returned query handle must be released using the WorkshopReleaseQueryRequest method once the query has been processed.
            /// </summary>
            /// <param name="fileIds">A list of workshop item IDs to include in the query.</param>
            /// <returns>Returns a handle to the query request, which can be used to execute the query and fetch the results.</returns>
            public static UGCQueryHandle_t CreateQueryDetailsRequest(List<PublishedFileId_t> fileIds) =>
                SteamUGC.CreateQueryUGCDetailsRequest(fileIds.ToArray(), (uint)fileIds.Count);

            /// <summary>
            /// Creates a query request to retrieve details for specific workshop items.
            /// The returned handle must be released using `WorkshopReleaseQueryRequest` when no longer needed.
            /// </summary>
            /// <param name="fileIds">A collection of workshop item identifiers to retrieve details for.</param>
            /// <returns>A handle representing the created query request.</returns>
            public static UGCQueryHandle_t CreateQueryDetailsRequest(IEnumerable<PublishedFileId_t> fileIds)
            {
                var publishedFileIdTs = fileIds as PublishedFileId_t[] ?? fileIds.ToArray();
                return SteamUGC.CreateQueryUGCDetailsRequest(publishedFileIdTs, (uint)publishedFileIdTs.Count());
            }

            /// <summary>
            /// Creates a user UGC (User Generated Content) query request. This function allows querying UGC associated with a specified user, such as subscribed content.
            /// Ensure to release the handle returned by this function using WorkshopReleaseQueryRequest upon completion.
            /// </summary>
            /// <param name="accountId">The Account ID to query the UGC for. Use CSteamID.GetAccountID to retrieve the account ID from a Steamworks ID.</param>
            /// <param name="listType">Specifies the type of UGC list to retrieve.</param>
            /// <param name="matchingType">Defines the type of UGC to query for.</param>
            /// <param name="sortOrder">Specifies the order in which the UGC list will be sorted.</param>
            /// <param name="creatorAppId">The App ID of the application where the UGC was created. This may differ from the consumerAppId if your creation tool is a separate application.</param>
            /// <param name="consumerAppId">The App ID of the current game or application that interacts with the UGC. Ensure not to pass the App ID of a separate creation tool here.</param>
            /// <param name="page">The page number of the results to retrieve. Page numbers should start at 1.</param>
            /// <returns>Returns a handle of the type UGCQueryHandle_t for the created UGC query request.</returns>
            public static UGCQueryHandle_t CreateQueryUserRequest(AccountID_t accountId, EUserUGCList listType,
                EUGCMatchingUGCType matchingType, EUserUGCListSortOrder sortOrder, AppId_t creatorAppId,
                AppId_t consumerAppId, uint page) => SteamUGC.CreateQueryUserUGCRequest(accountId, listType,
                matchingType, sortOrder, creatorAppId, consumerAppId, page);

            /// <summary>
            /// Releases the specified UGC query request, freeing associated resources.
            /// </summary>
            /// <param name="handle">The handle for the UGC query request to be released.</param>
            /// <returns>Returns true if the query request was successfully released; otherwise, returns false.</returns>
            public static bool ReleaseQueryRequest(UGCQueryHandle_t handle) => SteamUGC.ReleaseQueryUGCRequest(handle);

            /// <summary>
            /// Requests the deletion of a user-generated content (UGC) item identified by its file ID.
            /// </summary>
            /// <param name="fileId">The unique identifier of the UGC item to be deleted.</param>
            /// <param name="callback">The callback to invoke upon completion, providing the result of the delete operation and a success flag.</param>
            public static void DeleteItem(PublishedFileId_t fileId, Action<DeleteItemResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _deleteItem ??= CallResult<DeleteItemResult_t>.Create();

                var call = SteamUGC.DeleteItem(fileId);
                _deleteItem.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Request download of a UGC item
            /// </summary>
            /// <param name="fileId"></param>
            /// <param name="setHighPriority"></param>
            /// <returns></returns>
            public static bool DownloadItem(PublishedFileId_t fileId, bool setHighPriority) => SteamUGC.DownloadItem(fileId, setHighPriority);

            /// <summary>
            /// Requests the app dependencies for a specified workshop item.
            /// </summary>
            /// <param name="fileId">The unique identifier of the workshop item for which app dependencies are being requested.</param>
            /// <param name="callback">The callback to invoke once the operation is complete, providing the result and a sign of success or failure.</param>
            public static void GetAppDependencies(PublishedFileId_t fileId,
                Action<GetAppDependenciesResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _appDependenciesResult ??= CallResult<GetAppDependenciesResult_t>.Create();

                var call = SteamUGC.GetAppDependencies(fileId);
                _appDependenciesResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Retrieves the download progress information for a specified UGC (User Generated Content) item.
            /// </summary>
            /// <param name="fileId">The unique identifier of the workshop item to fetch download progress for.</param>
            /// <param name="completion">Outputs the completion percentage of the download as a floating-point value, where 0.5 represents 50% complete.</param>
            /// <returns>True if the download information was successfully retrieved; otherwise, false.</returns>
            public static bool GetItemDownloadInfo(PublishedFileId_t fileId, out float completion)
            {
                var result = SteamUGC.GetItemDownloadInfo(fileId, out var current, out var total);
                if (result)
                    completion = total > 0 ? Convert.ToSingle(Convert.ToDouble(current) / Convert.ToDouble(total)) : 0;
                else
                    completion = 0;
                return result;
            }

            /// <summary>
            /// Retrieves the installation information of a workshop item.
            /// </summary>
            /// <param name="fileId">The unique identifier of the workshop item to retrieve information for.</param>
            /// <param name="sizeOnDisk">The size of the workshop item on the disk, in bytes.</param>
            /// <param name="folderPath">The folder path where the workshop item is installed.</param>
            /// <param name="timeStamp">The timestamp representing when the workshop item was last updated.</param>
            /// <returns>Returns true if the workshop item is installed and contains valid content; otherwise, returns false.</returns>
            public static bool GetItemInstallInfo(PublishedFileId_t fileId, out ulong sizeOnDisk, out string folderPath,
                out DateTime timeStamp)
            {
                var result = SteamUGC.GetItemInstallInfo(fileId, out sizeOnDisk, out folderPath, 1024, out var iTimeStamp);
                timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                timeStamp = timeStamp.AddSeconds(iTimeStamp);
                return result;
            }

            /// <summary>
            /// Retrieves installation information for a Steam Workshop item.
            /// </summary>
            /// <param name="fileId">The unique identifier of the Workshop item.</param>
            /// <param name="sizeOnDisk">Outputs the size of the item on the disk in bytes.</param>
            /// <param name="folderPath">Outputs the path to the installed item's folder.</param>
            /// <param name="folderSize">The maximum size of the folder path buffer.</param>
            /// <param name="timeStamp">Outputs the installation timestamp of the item as a DateTime.</param>
            /// <returns>True if the Workshop item is installed; false if it is not installed, has no content, or the folder size is zero.</returns>
            public static bool GetItemInstallInfo(PublishedFileId_t fileId, out ulong sizeOnDisk, out string folderPath,
                uint folderSize, out DateTime timeStamp)
            {
                var result =
                    SteamUGC.GetItemInstallInfo(fileId, out sizeOnDisk, out folderPath, folderSize, out var iTimeStamp);
                timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                timeStamp = timeStamp.AddSeconds(iTimeStamp);
                return result;
            }

            /// <summary>
            /// Retrieves the current state of a specified workshop item on the client.
            /// </summary>
            /// <param name="fileId">The unique identifier of the workshop item.</param>
            /// <returns>The state flags of the specified workshop item as an <see cref="EItemState"/> enumeration.</returns>
            public static EItemState GetItemState(PublishedFileId_t fileId)
            {
                return (EItemState)SteamUGC.GetItemState(fileId);
            }

            /// <summary>
            /// Retrieves the progress of an item update associated with the given update handle.
            /// </summary>
            /// <param name="handle">The update handle representing the item update process.</param>
            /// <param name="completion">An output parameter receiving the completion percentage of the update, where 0.0 represents 0% and 1.0 represents 100%.</param>
            /// <returns>Returns the current status of the item update as an <see cref="EItemUpdateStatus"/> enumeration value.</returns>
            public static EItemUpdateStatus GetItemUpdateProgress(UGCUpdateHandle_t handle, out float completion)
            {
                var result = SteamUGC.GetItemUpdateProgress(handle, out ulong current, out ulong total);
                if (result != EItemUpdateStatus.k_EItemUpdateStatusInvalid)
                    completion = Convert.ToSingle(current / (double)total);
                else
                    completion = 0;
                return result;
            }

            /// <summary>
            /// Retrieves additional preview details for a specific item in a UGC query.
            /// </summary>
            /// <param name="handle">The handle of the UGC query to retrieve results from.</param>
            /// <param name="index">The index of the item within the query results to retrieve details for.</param>
            /// <param name="previewIndex">The index of the additional preview to retrieve details for.</param>
            /// <param name="urlOrVideoId">Outputs the URL or Video ID of the preview.</param>
            /// <param name="urlOrVideoSize">The size of the buffer allocated for urlOrVideoId in bytes.</param>
            /// <param name="fileName">The original file name of the preview. Can be null if not required.</param>
            /// <param name="fileNameSize">The size of the buffer allocated for fileName in bytes.</param>
            /// <param name="type">Outputs the type of the preview (e.g. image, video, etc.).</param>
            /// <returns>Returns true if the preview details were successfully retrieved; otherwise, false if the handle is invalid or if the indices are out of bounds.</returns>
            public static bool GetQueryAdditionalPreview(UGCQueryHandle_t handle, uint index, uint previewIndex,
                out string urlOrVideoId, uint urlOrVideoSize, out string fileName, uint fileNameSize,
                out EItemPreviewType type) => SteamUGC.GetQueryUGCAdditionalPreview(handle, index, previewIndex,
                out urlOrVideoId, urlOrVideoSize, out fileName, fileNameSize, out type);

            /// <summary>
            /// Retrieves the child items of a specified UGC query result.
            /// </summary>
            /// <param name="handle">The handle representing the UGC query.</param>
            /// <param name="index">The index of the result within the query to retrieve child items for.</param>
            /// <param name="fileIds">An array that will be populated with the child item IDs.</param>
            /// <param name="maxEntries">The maximum number of entries that the provided array can hold.</param>
            /// <returns>Returns true if the child items were successfully populated in the array. Returns false if the UGC query handle is invalid or the index is out of bounds.</returns>
            public static bool GetQueryChildren(UGCQueryHandle_t handle, uint index, PublishedFileId_t[] fileIds,
                uint maxEntries) => SteamUGC.GetQueryUGCChildren(handle, index, fileIds, maxEntries);

            /// <summary>
            /// Retrieves the key-value tag details associated with a specific workshop item
            /// from a UGC query result.
            /// </summary>
            /// <param name="handle">The handle to the UGC query containing the results.</param>
            /// <param name="index">The index of the workshop item within the query results to retrieve the tag from.</param>
            /// <param name="keyValueTagIndex">The index of the key-value tag to retrieve from the specified item.</param>
            /// <param name="key">Outputs the key part of the key-value tag.</param>
            /// <param name="value">Outputs the value part of the key-value tag.</param>
            /// <returns>Returns true if the key and value were successfully retrieved; otherwise, returns false.</returns>
            public static bool GetQueryKeyValueTag(UGCQueryHandle_t handle, uint index, uint keyValueTagIndex,
                out string key, out string value)
            {
                var ret = SteamUGC.GetQueryUGCKeyValueTag(handle, index, keyValueTagIndex, out key, 2048, out value,
                    2048);
                key = key.Trim();
                value = value.Trim();
                return ret;
            }

            /// <summary>
            /// Retrieves the key-value tag details associated with a specific workshop item from a UGC query result.
            /// </summary>
            /// <param name="handle">The handle representing the UGC query to retrieve results from.</param>
            /// <param name="index">The index of the item in the query result set.</param>
            /// <param name="keyValueTagIndex">The index of the key-value tag associated with the item.</param>
            /// <param name="key">Outputs the key of the key-value pair. The string is copied to this parameter.</param>
            /// <param name="keySize">The allocated size of the key string in bytes.</param>
            /// <param name="value">Outputs the value of the key-value pair. The string is copied to this parameter.</param>
            /// <param name="valueSize">The allocated size of the value string in bytes.</param>
            /// <returns>Returns true if the key and value were successfully retrieved and populated; otherwise, returns false if the handle is invalid or the indices are out of bounds.</returns>
            public static bool GetQueryKeyValueTag(UGCQueryHandle_t handle, uint index, uint keyValueTagIndex,
                out string key, uint keySize, out string value, uint valueSize) =>
                SteamUGC.GetQueryUGCKeyValueTag(handle, index, keyValueTagIndex, out key, keySize, out value,
                    valueSize);

            /// <summary>
            /// Retrieves metadata associated with a UGC (User-Generated Content) query result.
            /// </summary>
            /// <param name="handle">The handle corresponding to the UGC query request.</param>
            /// <param name="index">The index of the item within the query result set.</param>
            /// <param name="metadata">An output parameter that will contain the item's metadata on success.</param>
            /// <param name="size">The maximum size of the metadata string to retrieve.</param>
            /// <returns>Returns true if the metadata was successfully retrieved; otherwise, false.</returns>
            public static bool GetQueryMetadata(UGCQueryHandle_t handle, uint index, out string metadata, uint size) =>
                SteamUGC.GetQueryUGCMetadata(handle, index, out metadata, size);

            /// <summary>
            /// Retrieves the number of additional previews associated with the specified UGC query item.
            /// </summary>
            /// <param name="handle">The handle of the UGC query containing the desired item.</param>
            /// <param name="index">The index of the UGC item within the query to retrieve additional preview data for.</param>
            /// <returns>Returns the number of additional previews available for the specified UGC item.</returns>
            public static uint GetQueryNumAdditionalPreviews(UGCQueryHandle_t handle, uint index) =>
                SteamUGC.GetQueryUGCNumAdditionalPreviews(handle, index);

            /// <summary>
            /// Retrieves the number of key-value tags associated with a specific item in a UGC query result.
            /// </summary>
            /// <param name="handle">The handle to the UGC query from which the tags are being retrieved.</param>
            /// <param name="index">The index of the item within the query results whose tag count is being retrieved.</param>
            /// <returns>Returns the number of key-value tags associated with the specified item.</returns>
            public static uint GetQueryNumKeyValueTags(UGCQueryHandle_t handle, uint index) =>
                SteamUGC.GetQueryUGCNumKeyValueTags(handle, index);

            /// <summary>
            /// Retrieves the preview URL for an item in a user-generated content (UGC) query.
            /// </summary>
            /// <param name="handle">The handle that represents the UGC query.</param>
            /// <param name="index">The index of the item within the query results to retrieve the preview URL for.</param>
            /// <param name="url">The output string that will contain the preview URL if the operation is successful.</param>
            /// <param name="urlSize">The size of the buffer allocated for the preview URL.</param>
            /// <returns>Returns true if the preview URL is successfully retrieved; otherwise, returns false.</returns>
            public static bool GetQueryPreviewURL(UGCQueryHandle_t handle, uint index, out string url, uint urlSize) =>
                SteamUGC.GetQueryUGCPreviewURL(handle, index, out url, urlSize);

            /// <summary>
            /// Retrieves the details of a UGC (User Generated Content) item from a query result at the specified index.
            /// </summary>
            /// <param name="handle">The handle representing the UGC query.</param>
            /// <param name="index">The index of the item within the query results to retrieve details for.</param>
            /// <param name="details">An output parameter that holds the UGC item details upon successful retrieval.</param>
            /// <returns>Returns true if the item details are successfully retrieved; otherwise, returns false.</returns>
            public static bool GetQueryResult(UGCQueryHandle_t handle, uint index, out SteamUGCDetails_t details) =>
                SteamUGC.GetQueryUGCResult(handle, index, out details);

            /// <summary>
            /// Retrieves a specific statistic for an item in a user-generated content (UGC) query.
            /// </summary>
            /// <param name="handle">The handle to the UGC query.</param>
            /// <param name="index">The index of the item within the query results.</param>
            /// <param name="statType">The type of statistic to retrieve, specified by the <see cref="EItemStatistic"/> enum.</param>
            /// <param name="statValue">When this method returns, contains the retrieved statistic value if successful.</param>
            /// <returns>Returns true if the statistic was successfully retrieved; otherwise, returns false.</returns>
            public static bool GetQueryStatistic(UGCQueryHandle_t handle, uint index, EItemStatistic statType,
                out ulong statValue) => SteamUGC.GetQueryUGCStatistic(handle, index, statType, out statValue);

            /// <summary>
            /// Retrieves details of all items the user is subscribed to and invokes the provided callback with the results.
            /// </summary>
            /// <param name="withLongDescription">Determines whether to include the full long description for each item.</param>
            /// <param name="withMetadata">Specifies whether to include metadata for each item.</param>
            /// <param name="withKeyValueTags">Indicates whether to include key-value tags for each item.</param>
            /// <param name="withAdditionalPreviews">Specifies whether to include additional preview images beyond the main preview.</param>
            /// <param name="withPlayTimeStatsInDays">The number of days over which to retrieve playtime statistics for the items, or 0 to skip playtime stats.</param>
            /// <param name="callback">A callback function to execute with the list of retrieved WorkshopItemDetails objects.</param>
            public static void GetSubscribedItems(bool withLongDescription, bool withMetadata, bool withKeyValueTags,
                bool withAdditionalPreviews, uint withPlayTimeStatsInDays, Action<List<WorkshopItemDetails>> callback)
            {
                var query = UgcQuery.GetSubscribed(withLongDescription, withMetadata, withKeyValueTags,
                    withAdditionalPreviews, withPlayTimeStatsInDays);
                if (query != null)
                {
                    query.Execute(_ =>
                    {
                        callback?.Invoke(query.ResultsList);
                        query.Dispose();
                    });
                }
                else
                {
                    callback?.Invoke(new());
                }
            }

            /// <summary>
            /// Retrieves the vote status of a specified user-generated content (UGC) item.
            /// </summary>
            /// <param name="fileId">The unique identifier of the UGC item.</param>
            /// <param name="callback">The callback to invoke with the result of the operation, providing the vote result and a success status.</param>
            public static void GetUserItemVote(PublishedFileId_t fileId, Action<GetUserItemVoteResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _getUserItemVoteResult ??= CallResult<GetUserItemVoteResult_t>.Create();

                var call = SteamUGC.GetUserItemVote(fileId);
                _getUserItemVoteResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Asynchronously retrieves the status of whether the user has accepted or rejected the Workshop EULA for the current app.
            /// </summary>
            /// <param name="callback">The callback to be invoked with the Workshop EULA status and a success indicator. The first parameter
            /// represents the WorkshopEULAStatus_t data, and the second parameter indicates whether the operation was successful.</param>
            public static void GetWorkshopEulaStatus(Action<WorkshopEULAStatus_t, bool> callback)
            {
                if (callback == null)
                    return;

                _workshopEulaStatus ??= CallResult<WorkshopEULAStatus_t>.Create();

                var handle = SteamUGC.GetWorkshopEULAStatus();
                _workshopEulaStatus.Set(handle, callback.Invoke);
            }

            /// <summary>
            /// Displays the latest Workshop End User Licence Agreement (EULA) in an overlay window, allowing the user to review and accept or decline it.
            /// </summary>
            /// <returns>Returns true if the overlay window for the Workshop EULA is successfully shown; otherwise, returns false.</returns>
            public static bool ShowWorkshopEula() => SteamUGC.ShowWorkshopEULA();

            /// <summary>
            /// Requests the removal of an application dependency from a User Generated Content (UGC) item.
            /// </summary>
            /// <param name="fileId">The unique identifier of the UGC item from which the application dependency should be removed.</param>
            /// <param name="appId">The unique identifier of the application to be removed as a dependency from the UGC item.</param>
            /// <param name="callback">
            /// A callback function to be executed upon completion of the operation. The callback receives the result of the removal process
            /// and a success flag indicating whether the operation was successful.
            /// </param>
            public static void RemoveAppDependency(PublishedFileId_t fileId, AppId_t appId,
                Action<RemoveAppDependencyResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _removeAppDependencyResult ??= CallResult<RemoveAppDependencyResult_t>.Create();

                var call = SteamUGC.RemoveAppDependency(fileId, appId);
                _removeAppDependencyResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Requests the removal of a dependency between two UGC items.
            /// </summary>
            /// <param name="parentFileId">The unique identifier of the parent UGC item.</param>
            /// <param name="childFileId">The unique identifier of the child UGC item to be removed as a dependency.</param>
            /// <param name="callback">The callback to execute upon completion of the operation, providing the result and success status.</param>
            public static void RemoveDependency(PublishedFileId_t parentFileId, PublishedFileId_t childFileId,
                Action<RemoveUGCDependencyResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _removeDependencyResult ??= CallResult<RemoveUGCDependencyResult_t>.Create();

                var call = SteamUGC.RemoveDependency(parentFileId, childFileId);
                _removeDependencyResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Removes a UGC (User Generated Content) item from the user's favorites.
            /// </summary>
            /// <param name="appId">The unique identifier of the application associated with the UGC item.</param>
            /// <param name="fileId">The unique identifier of the UGC item to be removed from favorites.</param>
            /// <param name="callback">The callback invoked upon completion, providing the result of the operation and a success flag.</param>
            public static void RemoveItemFromFavorites(AppId_t appId, PublishedFileId_t fileId,
                Action<UserFavoriteItemsListChanged_t, bool> callback)
            {
                if (callback == null)
                    return;

                _userFavoriteItemsListChanged ??= CallResult<UserFavoriteItemsListChanged_t>.Create();

                var call = SteamUGC.RemoveItemFromFavorites(appId, fileId);
                _userFavoriteItemsListChanged.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Removes key-value tags associated with a UGC item update.
            /// </summary>
            /// <param name="handle">The handle for the UGC item update process.</param>
            /// <param name="key">The key of the key-value pair to remove.</param>
            /// <returns>Returns true if the key-value tags were successfully removed; otherwise, returns false.</returns>
            public static bool RemoveItemKeyValueTags(UGCUpdateHandle_t handle, string key) =>
                SteamUGC.RemoveItemKeyValueTags(handle, key);

            /// <summary>
            /// Removes a preview image or video from a UGC (User-Generated Content) item.
            /// </summary>
            /// <param name="handle">The handle of the UGC update process.</param>
            /// <param name="index">The zero-based index of the preview to remove.</param>
            /// <returns>Returns true if the preview was successfully removed; otherwise, returns false.</returns>
            public static bool RemoveItemPreview(UGCUpdateHandle_t handle, uint index) =>
                SteamUGC.RemoveItemPreview(handle, index);

            /// <summary>
            /// Requests the details of a User-Generated Content (UGC) item.
            /// </summary>
            /// <param name="fileId">The unique identifier of the UGC item to request details for.</param>
            /// <param name="maxAgeSeconds">Specifies the maximum allowable age of the cached data in seconds.</param>
            /// <param name="callback">The callback function to invoke with the retrieved UGC details and success status.</param>
            public static void RequestDetails(PublishedFileId_t fileId, uint maxAgeSeconds,
                Action<SteamUGCRequestUGCDetailsResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _steamUgcRequestUgcDetailsResult ??= CallResult<SteamUGCRequestUGCDetailsResult_t>.Create();

                var call = SteamUGC.RequestUGCDetails(fileId, maxAgeSeconds);
                _steamUgcRequestUgcDetailsResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Sends a User-Generated Content (UGC) query request to the Steamworks API.
            /// </summary>
            /// <param name="handle">The handle representing the UGC query to be sent.</param>
            /// <param name="callback">The callback to be invoked upon query completion, providing the result and a success status.</param>
            public static void SendQueryUgcRequest(UGCQueryHandle_t handle,
                Action<SteamUGCQueryCompleted_t, bool> callback)
            {
                if (callback == null)
                    return;

                _steamUgcQueryCompleted ??= CallResult<SteamUGCQueryCompleted_t>.Create();

                var call = SteamUGC.SendQueryUGCRequest(handle);
                _steamUgcQueryCompleted.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Configures whether cached responses are permitted for a specific UGC query and defines the maximum age for the cached data.
            /// </summary>
            /// <param name="handle">The handle representing the UGC query.</param>
            /// <param name="maxAgeSeconds">The maximum age, in seconds, that the cached response is allowed to have.</param>
            /// <returns>Returns true if the operation was successful; otherwise, returns false.</returns>
            public static bool SetAllowCachedResponse(UGCQueryHandle_t handle, uint maxAgeSeconds) =>
                SteamUGC.SetAllowCachedResponse(handle, maxAgeSeconds);

            /// <summary>
            /// Sets a filter to match a specific cloud file name in a user-generated content (UGC) query.
            /// </summary>
            /// <param name="handle">The handle of the UGC query to apply the file name filter to.</param>
            /// <param name="fileName">The name of the cloud file to filter for in the UGC query.</param>
            /// <returns>Returns true if the file name filter was successfully applied; otherwise, returns false.</returns>
            public static bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string fileName) =>
                SteamUGC.SetCloudFileNameFilter(handle, fileName);

            /// <summary>
            /// Sets the content folder for a workshop item during an update process.
            /// </summary>
            /// <param name="handle">The handle associated with the item update operation.</param>
            /// <param name="folder">The path to the folder containing the item's content.</param>
            /// <returns>Returns true if the content folder was successfully set; otherwise, false.</returns>
            public static bool SetItemContent(UGCUpdateHandle_t handle, string folder) =>
                SteamUGC.SetItemContent(handle, folder);

            /// <summary>
            /// Updates the description of a user-generated content (UGC) item.
            /// </summary>
            /// <param name="handle">The handle representing the update process for the UGC item.</param>
            /// <param name="description">The new description text to set for the UGC item.</param>
            /// <returns>Returns true if the description was successfully set; otherwise, returns false.</returns>
            public static bool SetItemDescription(UGCUpdateHandle_t handle, string description) =>
                SteamUGC.SetItemDescription(handle, description);

            /// <summary>
            /// Assigns custom metadata to a workshop item.
            /// </summary>
            /// <param name="handle">The update handle associated with the workshop item to modify.</param>
            /// <param name="metadata">The metadata string to associate with the workshop item. This should not exceed the maximum allowed length.</param>
            /// <returns>Returns true if the metadata was successfully set; otherwise, returns false.</returns>
            public static bool SetItemMetadata(UGCUpdateHandle_t handle, string metadata) =>
                SteamUGC.SetItemMetadata(handle, metadata);

            /// <summary>
            /// Sets the preview image or file for a workshop item.
            /// </summary>
            /// <param name="handle">The update handle associated with the workshop item being modified.</param>
            /// <param name="previewFile">The path to the preview file to be set for the workshop item.</param>
            /// <returns>Returns true if the preview file was successfully set; otherwise, returns false.</returns>
            public static bool SetItemPreview(UGCUpdateHandle_t handle, string previewFile) =>
                SteamUGC.SetItemPreview(handle, previewFile);

            /// <summary>
            /// Assigns a list of tags to a specific item for Steam Workshop integration.
            /// </summary>
            /// <param name="handle">The unique update handle for the item being modified.</param>
            /// <param name="tags">A list of tags to assign to the item.</param>
            /// <returns>Returns true if the tags were successfully set; otherwise, returns false.</returns>
            public static bool SetItemTags(UGCUpdateHandle_t handle, List<string> tags) =>
                SteamUGC.SetItemTags(handle, tags);

            /// <summary>
            /// Sets the title for a user-generated content (UGC) item during an update operation.
            /// </summary>
            /// <param name="handle">The handle for the UGC update operation.</param>
            /// <param name="title">The title to assign to the UGC item.</param>
            /// <returns>Returns true if the title was set successfully; otherwise, returns false.</returns>
            public static bool SetItemTitle(UGCUpdateHandle_t handle, string title) =>
                SteamUGC.SetItemTitle(handle, title);

            /// <summary>
            /// Sets the language for the item update.
            /// </summary>
            /// <param name="handle">The update handle associated with the item.</param>
            /// <param name="language">The language code to set, formatted as a string.</param>
            /// <returns>Returns true if the operation succeeds; otherwise, returns false.</returns>
            public static bool SetItemUpdateLanguage(UGCUpdateHandle_t handle, string language) =>
                SteamUGC.SetItemUpdateLanguage(handle, language);

            /// <summary>
            /// Updates the visibility setting of a user-generated content (UGC) item.
            /// </summary>
            /// <param name="handle">The handle representing the UGC item being updated.</param>
            /// <param name="visibility">The desired visibility level for the UGC item.</param>
            /// <returns>Returns true if the visibility update was successfully applied; otherwise, returns false.</returns>
            public static bool SetItemVisibility(UGCUpdateHandle_t handle,
                ERemoteStoragePublishedFileVisibility visibility) => SteamUGC.SetItemVisibility(handle, visibility);

            /// <summary>
            /// Sets the language for the specified user-generated content (UGC) query handle.
            /// </summary>
            /// <param name="handle">The UGC query handle to which the language will be applied.</param>
            /// <param name="language">The language code to set for the UGC query, typically in ISO 639-1 format.</param>
            /// <returns>Returns true if the language was successfully set for the UGC query; otherwise, returns false.</returns>
            public static bool SetLanguage(UGCQueryHandle_t handle, string language) =>
                SteamUGC.SetLanguage(handle, language);

            /// <summary>
            /// Configures a UGC (User-Generated Content) query to match any of the specified tags.
            /// </summary>
            /// <param name="handle">The handle to the UGC query being configured.</param>
            /// <param name="anyTag">Indicates whether the query should match any tag (true) or require all tags to match (false).</param>
            /// <returns>Returns true if the operation was successful; otherwise, returns false.</returns>
            public static bool SetMatchAnyTag(UGCQueryHandle_t handle, bool anyTag) =>
                SteamUGC.SetMatchAnyTag(handle, anyTag);

            /// <summary>
            /// Configures the number of days used to calculate the trend ranking in a UGC query.
            /// </summary>
            /// <param name="handle">The handle to the UGC query being modified.</param>
            /// <param name="days">The number of days to consider for ranking by trend.</param>
            /// <returns>Returns true if the operation is successfully applied; otherwise, returns false.</returns>
            public static bool SetRankedByTrendDays(UGCQueryHandle_t handle, uint days) =>
                SteamUGC.SetRankedByTrendDays(handle, days);

            /// <summary>
            /// Configures the query to include additional previews in the returned results.
            /// </summary>
            /// <param name="handle">The handle to the UGC query being configured.</param>
            /// <param name="additionalPreviews">A boolean value indicating whether to include additional previews in the query results.</param>
            /// <returns>Returns true if the configuration was successfully applied; otherwise, returns false.</returns>
            public static bool SetReturnAdditionalPreviews(UGCQueryHandle_t handle, bool additionalPreviews) =>
                SteamUGC.SetReturnAdditionalPreviews(handle, additionalPreviews);

            /// <summary>
            /// Configures whether child items of a given workshop item should be included in the query results.
            /// </summary>
            /// <param name="handle">The handle representing the UGC query to configure.</param>
            /// <param name="returnChildren">A boolean value indicating whether to include child items in the query results.</param>
            /// <returns>Returns true if the operation is successful, otherwise false.</returns>
            public static bool SetReturnChildren(UGCQueryHandle_t handle, bool returnChildren) =>
                SteamUGC.SetReturnChildren(handle, returnChildren);

            /// <summary>
            /// Configures whether key-value tags should be included in the results of a UGC query.
            /// </summary>
            /// <param name="handle">The UGC query handle identifying the query to be modified.</param>
            /// <param name="tags">A boolean value indicating whether key-value tags should be included in the query results.</param>
            /// <returns>Returns true if the operation to set the return of key-value tags was successful; otherwise, returns false.</returns>
            public static bool SetReturnKeyValueTags(UGCQueryHandle_t handle, bool tags) =>
                SteamUGC.SetReturnKeyValueTags(handle, tags);

            /// <summary>
            /// Configures the query to return extended descriptions for user-generated content (UGC).
            /// </summary>
            /// <param name="handle">The UGC query handle representing the query to modify.</param>
            /// <param name="longDescription">A boolean indicating whether to include long descriptions in the query results.</param>
            /// <returns>Returns true if the operation was successful; otherwise, returns false.</returns>
            public static bool SetReturnLongDescription(UGCQueryHandle_t handle, bool longDescription) =>
                SteamUGC.SetReturnLongDescription(handle, longDescription);

            /// <summary>
            /// Configures the query to return metadata associated with the UGC (User-Generated Content).
            /// </summary>
            /// <param name="handle">The handle for the UGC query to modify.</param>
            /// <param name="metadata">A boolean indicating whether metadata information should be included in the query results.</param>
            /// <returns>Returns true if the operation was successful; otherwise, returns false.</returns>
            public static bool SetReturnMetadata(UGCQueryHandle_t handle, bool metadata) =>
                SteamUGC.SetReturnMetadata(handle, metadata);

            /// <summary>
            /// Configures a UGC query to return only the IDs of the matched items.
            /// </summary>
            /// <param name="handle">The handle to the UGC query object.</param>
            /// <param name="onlyIds">A boolean indicating whether the query should return only item IDs.
            /// Set to true to return only IDs, or false to return full item data.</param>
            /// <returns>Returns true if the operation was successful; otherwise, returns false.</returns>
            public static bool SetReturnOnlyIDs(UGCQueryHandle_t handle, bool onlyIds) =>
                SteamUGC.SetReturnOnlyIDs(handle, onlyIds);

            /// <summary>
            /// Configures the query to include playtime statistics for the specified number of days.
            /// </summary>
            /// <param name="handle">The handle to the user-generated content (UGC) query.</param>
            /// <param name="days">The number of days for which playtime statistics should be returned.</param>
            /// <returns>Returns true if the operation to set playtime statistics was successful; otherwise, returns false.</returns>
            public static bool SetReturnPlaytimeStats(UGCQueryHandle_t handle, uint days) =>
                SteamUGC.SetReturnPlaytimeStats(handle, days);

            /// <summary>
            /// Configures the query to return only the total matching result count instead of full details on matching items.
            /// </summary>
            /// <param name="handle">The query handle to configure.</param>
            /// <param name="totalOnly">A boolean value indicating whether to return only the total count of matching results.</param>
            /// <returns>Returns true if the configuration was successfully applied to the query handle; otherwise, returns false.</returns>
            public static bool SetReturnTotalOnly(UGCQueryHandle_t handle, bool totalOnly) =>
                SteamUGC.SetReturnTotalOnly(handle, totalOnly);

            /// <summary>
            /// Sets the search text for the provided UGC query handle. The search text is used to filter workshop items based on the specified keywords.
            /// </summary>
            /// <param name="handle">The UGC query handle to apply the search text to.</param>
            /// <param name="text">The search text used to filter the workshop items.</param>
            /// <returns>Returns true if the search text was successfully set; otherwise, returns false.</returns>
            public static bool SetSearchText(UGCQueryHandle_t handle, string text) =>
                SteamUGC.SetSearchText(handle, text);

            /// <summary>
            /// Submits a vote for a specific user-generated content item.
            /// </summary>
            /// <param name="fileID">The unique identifier of the user-generated content item.</param>
            /// <param name="voteUp">A boolean indicating whether the vote is positive (true) or negative (false).</param>
            /// <param name="callback">The action to invoke when the operation is completed, containing the result of the vote submission and a boolean indicating success.</param>
            public static void SetUserItemVote(PublishedFileId_t fileID, bool voteUp,
                Action<SetUserItemVoteResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _setUserItemVoteResult ??= CallResult<SetUserItemVoteResult_t>.Create();

                var call = SteamUGC.SetUserItemVote(fileID, voteUp);
                _setUserItemVoteResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Initiates an update process for a user-generated content (UGC) item.
            /// </summary>
            /// <param name="appId">The application ID associated with the content.</param>
            /// <param name="fileID">The ID of the published file to be updated.</param>
            /// <returns>
            /// Returns a UGCUpdateHandle_t structure that represents the handle for the update session.
            /// </returns>
            public static UGCUpdateHandle_t StartItemUpdate(AppId_t appId, PublishedFileId_t fileID) =>
                SteamUGC.StartItemUpdate(appId, fileID);

            /// <summary>
            /// Initiates playtime tracking for a set of user-generated content files.
            /// </summary>
            /// <param name="fileIds">An array of file IDs representing the user-generated content to track.</param>
            /// <param name="callback">The callback to invoke with the result of the playtime tracking operation and its success status.</param>
            public static void StartPlaytimeTracking(PublishedFileId_t[] fileIds,
                Action<StartPlaytimeTrackingResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _startPlaytimeTrackingResult ??= CallResult<StartPlaytimeTrackingResult_t>.Create();

                var call = SteamUGC.StartPlaytimeTracking(fileIds, (uint)fileIds.Length);
                _startPlaytimeTrackingResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Stops playtime tracking for the specified list of file IDs.
            /// </summary>
            /// <param name="fileIds">An array of file IDs for which playtime tracking should be stopped.</param>
            /// <param name="callback">
            /// A callback method invoked when the operation is complete. The callback provides
            /// the result of the playtime tracking stop operation and a boolean indicating success.
            /// </param>
            public static void StopPlaytimeTracking(PublishedFileId_t[] fileIds,
                Action<StopPlaytimeTrackingResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _stopPlaytimeTrackingResult ??= CallResult<StopPlaytimeTrackingResult_t>.Create();

                var call = SteamUGC.StopPlaytimeTracking(fileIds, (uint)fileIds.Length);
                _stopPlaytimeTrackingResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Stops playtime tracking for all items associated with the user.
            /// </summary>
            /// <param name="callback">A callback to handle the result of the operation, providing the outcome and success status.</param>
            public static void StopPlaytimeTrackingForAllItems(Action<StopPlaytimeTrackingResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _stopPlaytimeTrackingResult ??= CallResult<StopPlaytimeTrackingResult_t>.Create();

                var call = SteamUGC.StopPlaytimeTrackingForAllItems();
                _stopPlaytimeTrackingResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Submits an update for a user-generated content (UGC) item.
            /// </summary>
            /// <param name="handle">The handle that identifies the item update process.</param>
            /// <param name="changeNote">A description of the changes made in this update.</param>
            /// <param name="callback">The callback to invoke when the update submission is complete, providing the result of the operation.</param>
            public static void SubmitItemUpdate(UGCUpdateHandle_t handle, string changeNote,
                Action<SubmitItemUpdateResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _submitItemUpdateResult ??= CallResult<SubmitItemUpdateResult_t>.Create();

                var call = SteamUGC.SubmitItemUpdate(handle, changeNote);
                _submitItemUpdateResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Subscribes to a user-generated content item.
            /// </summary>
            /// <param name="fileId">The unique identifier of the published file to subscribe to.</param>
            /// <param name="callback">
            /// The callback to invoke once the subscription operation is completed.
            /// Returns the result of the subscription operation and a boolean indicating success or failure.
            /// </param>
            public static void SubscribeItem(PublishedFileId_t fileId,
                Action<RemoteStorageSubscribePublishedFileResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _remoteStorageSubscribePublishedFileResult ??= CallResult<RemoteStorageSubscribePublishedFileResult_t>.Create();

                var call = SteamUGC.SubscribeItem(fileId);
                _remoteStorageSubscribePublishedFileResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Suspends or resumes the download of Workshop items based on the specified parameter.
            /// </summary>
            /// <param name="suspend">If true, downloads are suspended; if false, downloads are resumed.</param>
            public static void SuspendDownloads(bool suspend) => SteamUGC.SuspendDownloads(suspend);

            /// <summary>
            /// Unsubscribes from a user-generated content item by its identifier.
            /// </summary>
            /// <param name="fileId">The identifier of the item to unsubscribe from.</param>
            /// <param name="callback">The callback that is invoked when the unsubscribe action is completed. The callback includes the result of the operation and a boolean indicating the success status.</param>
            public static void UnsubscribeItem(PublishedFileId_t fileId,
                Action<RemoteStorageUnsubscribePublishedFileResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _remoteStorageUnsubscribePublishedFileResult ??= CallResult<RemoteStorageUnsubscribePublishedFileResult_t>.Create();

                var call = SteamUGC.UnsubscribeItem(fileId);
                _remoteStorageUnsubscribePublishedFileResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Updates an existing preview file for an item during a UGC (User Generated Content) update process.
            /// </summary>
            /// <param name="handle">The update handle identifying the item update process.</param>
            /// <param name="index">The index of the preview file to update.</param>
            /// <param name="file">The new file path to set as the preview file.</param>
            /// <returns>Returns true if the preview file was successfully updated; otherwise, returns false.</returns>
            public static bool UpdateItemPreviewFile(UGCUpdateHandle_t handle, uint index, string file) =>
                SteamUGC.UpdateItemPreviewFile(handle, index, file);

            /// <summary>
            /// Updates the video preview of an item at a specified index.
            /// </summary>
            /// <param name="handle">The handle representing the update process for the workshop item.</param>
            /// <param name="index">The index of the preview video to update in the workshop item.</param>
            /// <param name="videoId">The unique identifier of the new video to set as the preview.</param>
            /// <returns>Returns true if the video preview is successfully updated; otherwise, returns false.</returns>
            public static bool UpdateItemPreviewVideo(UGCUpdateHandle_t handle, uint index, string videoId) =>
                SteamUGC.UpdateItemPreviewVideo(handle, index, videoId);

            #endregion

#if STEAM_LEGACY || STEAM_161
            /// <summary>
            /// Retrieves the file IDs of all UGC (User Generated Content) items the user is subscribed to, up to the specified maximum entries.
            /// </summary>
            /// <param name="fileIDs">An array to store the retrieved file IDs of subscribed items.</param>
            /// <param name="maxEntries">The maximum number of entries to fetch from the user's subscribed items.</param>
            /// <returns>The total number of subscribed items fetched and populated into the array.</returns>
            public static uint GetSubscribedItems(PublishedFileId_t[] fileIDs, uint maxEntries) =>
                SteamUGC.GetSubscribedItems(fileIDs, maxEntries);

            /// <summary>
            /// Retrieves the IDs of all workshop items the user is currently subscribed to.
            /// </summary>
            /// <returns>An array of <c>PublishedFileId_t</c> representing the IDs of the subscribed items. Returns an empty array if the user is not subscribed to any items.</returns>
            public static PublishedFileId_t[] GetSubscribedItems()
            {
                var count = GetNumSubscribedItems();
                if (count > 0)
                {
                    var fileIds = new PublishedFileId_t[count];
                    if (GetSubscribedItems(fileIds, count) > 0)
                    {
                        return fileIds;
                    }
                    else
                        return Array.Empty<PublishedFileId_t>();
                }
                else
                    return Array.Empty<PublishedFileId_t>();
            }

            /// <summary>
            /// Retrieves the list of workshop items the user is subscribed to and invokes the specified callback with the results.
            /// </summary>
            /// <param name="callback">A callback invoked with a list of <see cref="WorkshopItemDetails"/> objects representing the subscribed items.</param>
            public static void GetSubscribedItems(Action<List<WorkshopItemDetails>> callback)
            {
                var query = UgcQuery.GetSubscribed();
                if (query != null)
                {
                    query.Execute(r =>
                    {
                        callback?.Invoke(query.ResultsList);
                        query.Dispose();
                    });
                }
                else
                {
                    callback?.Invoke(new());
                }
            }

            /// <summary>
            /// Retrieves the total number of UGC (User-Generated Content) items that the user is subscribed to.
            /// </summary>
            /// <returns>Returns the number of subscribed UGC items. If called from a game server, it returns 0.</returns>
            public static uint GetNumSubscribedItems() => SteamUGC.GetNumSubscribedItems();
#endif

#if STEAM_162 || STEAM_163
            /// <summary>
            /// Sets the local load order for a list of subscribed workshop items. Any items not included in the specified list
            /// will automatically be sorted based on their subscription time.
            /// </summary>
            /// <param name="publishedFileIDs">An array of published file IDs representing the workshop items to set the load order for.</param>
            /// <param name="numPublishedFileIDs">The total number of published file IDs in the provided array.</param>
            /// <returns>True if the operation succeeds, otherwise false.</returns>
            public static bool
                SetSubscriptionsLoadOrder(PublishedFileId_t[] publishedFileIDs, uint numPublishedFileIDs) =>
                SteamUGC.SetSubscriptionsLoadOrder(publishedFileIDs, numPublishedFileIDs);

            /// <summary>
            /// Sets whether the specified items should be disabled locally. Disabled items will not be included in the results of GetSubscribedItems() by default.
            /// </summary>
            /// <param name="publishedFileIDs">An array of PublishedFileId_t values representing the items to be modified.</param>
            /// <param name="numPublishedFileIDs">The number of items in the PublishedFileIDs array.</param>
            /// <param name="disabledLocally">A boolean value indicating whether the items should be disabled locally (true) or not (false).</param>
            /// <returns>Returns true if the operation is successful; otherwise, false.</returns>
            public static bool SetItemsDisabledLocally(PublishedFileId_t[] publishedFileIDs, uint numPublishedFileIDs,
                bool disabledLocally) => SteamUGC.SetItemsDisabledLocally(publishedFileIDs, numPublishedFileIDs, disabledLocally);

            /// <summary>
            /// Retrieves the file IDs of all subscribed UGC (User Generated Content) items, up to the specified maximum number of entries.
            /// </summary>
            /// <param name="fileIDs">An array to store the published file IDs of the subscribed items. The size of the array determines how many IDs can be fetched.</param>
            /// <param name="maxEntries">The maximum number of subscribed item IDs to retrieve.</param>
            /// <param name="includeLocallyDisabled">Specifies whether to include items that are disabled locally in the results.</param>
            /// <returns>Returns the number of items successfully retrieved and stored in the provided array.</returns>
            public static uint GetSubscribedItems(PublishedFileId_t[] fileIDs, uint maxEntries,
                bool includeLocallyDisabled = false) =>
                SteamUGC.GetSubscribedItems(fileIDs, maxEntries, includeLocallyDisabled);

            /// <summary>
            /// Retrieves the list of file IDs that the user is subscribed to.
            /// </summary>
            /// <param name="includeLocallyDisabled">
            /// Whether to include locally disabled items in the result.
            /// </param>
            /// <returns>
            /// An array of <see cref="PublishedFileId_t"/> representing the IDs of the subscribed items.
            /// If no items are found, an empty array is returned.
            /// </returns>
            public static PublishedFileId_t[] GetSubscribedItems(bool includeLocallyDisabled = false)
            {
                var count = GetNumSubscribedItems(includeLocallyDisabled);
                if (count > 0)
                {
                    var fileIds = new PublishedFileId_t[count];
                    if (GetSubscribedItems(fileIds, count, includeLocallyDisabled) > 0)
                    {
                        return fileIds;
                    }
                    else
                        return Array.Empty<PublishedFileId_t>();
                }
                else
                    return Array.Empty<PublishedFileId_t>();
            }

            /// <summary>
            /// Retrieves a list of workshop items that the user is subscribed to and invokes the provided callback with the results.
            /// </summary>
            /// <param name="callback">A callback function that will receive the list of subscribed workshop item details.</param>
            /// <param name="includeLocallyDisabled">Determines whether locally disabled subscribed items should be included in the results.</param>
            public static void GetSubscribedItems(Action<List<WorkshopItemDetails>> callback,
                bool includeLocallyDisabled = false)
            {
                var query = UgcQuery.GetSubscribed(includeLocallyDisabled);
                if (query != null)
                {
                    query.Execute(_ =>
                    {
                        callback?.Invoke(query.ResultsList);
                        query.Dispose();
                    });
                }
                else
                {
                    callback?.Invoke(new());
                }
            }

            /// <summary>
            /// Retrieves the total number of user-generated content (UGC) items the user is subscribed to.
            /// </summary>
            /// <param name="includeLocallyDisabled">If true, includes items that are locally disabled.</param>
            /// <returns>Returns the total count of subscribed UGC items. Returns 0 if called from a game server.</returns>
            public static uint GetNumSubscribedItems(bool includeLocallyDisabled = false) =>
                SteamUGC.GetNumSubscribedItems(includeLocallyDisabled);
#endif
        }
    }
}
#endif