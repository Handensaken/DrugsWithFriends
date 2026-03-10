#if !DISABLESTEAMWORKS && STEAM_INSTALLED
using Steamworks;
using System;
using System.IO;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A helper object that will expose the results of a <see cref="WorkshopItemEditorData"/>
    /// </summary>
    public class WorkshopUploadWorker
    {
        /// <summary>
        /// The resulting file ID
        /// </summary>
        public PublishedFileId_t? FileId => _itemData.PublishedFileId;
        /// <summary>
        /// The App ID related to the item
        /// </summary>
        public AppData AppId => _itemData.appId;
        /// <summary>
        /// The item's title
        /// </summary>
        public string Title => _itemData.title;
        /// <summary>
        /// The items description
        /// </summary>
        public string Description => _itemData.description;
        /// <summary>
        /// The content folder of the item's content
        /// </summary>
        public DirectoryInfo Content => _itemData.Content;
        /// <summary>
        /// The location of the item's preview image
        /// </summary>
        public FileInfo Preview => _itemData.Preview;
        /// <summary>
        /// The item's metadata
        /// </summary>
        public string Metadata => _itemData.metadata;
        /// <summary>
        /// The items tags
        /// </summary>
        public string[] Tags => _itemData.tags;
        /// <summary>
        /// The items visibility
        /// </summary>
        public ERemoteStoragePublishedFileVisibility Visibility => _itemData.visibility;
        /// <summary>
        /// An event that is invoked when the process is completed
        /// </summary>
        public event EventHandler<WorkshopItemDataCreateStatus> Completed;
        /// <summary>
        /// An event that is invoked when the update started
        /// </summary>
        public event EventHandler<UGCUpdateHandle_t> UpdateStarted;
        /// <summary>
        /// An event that is invoked when the item is created
        /// </summary>
        public event EventHandler<CreateItemResult_t> FileCreated;

        private WorkshopItemEditorData _itemData;
        private UGCUpdateHandle_t? _updateHandle;

        /// <summary>
        /// Gets a new <see cref="WorkshopUploadWorker"/> based on provided <see cref="WorkshopItemEditorData"/>
        /// </summary>
        /// <param name="data">The data to create the worker with</param>
        /// <returns>A new worker</returns>
        public static WorkshopUploadWorker Get(WorkshopItemEditorData data) => new WorkshopUploadWorker { _itemData = data };
        /// <summary>
        /// Run the create process
        /// </summary>
        /// <returns></returns>
        public bool RunCreate()
        {
            if (_itemData.IsValid)
                return _itemData.Create(CompletedHandler, UploadStartedHandler, FileCreatedHandler);
            else
                return false;
        }
        /// <summary>
        /// Run the create process
        /// </summary>
        /// <param name="additionalPreviews">A collection of additional preview images</param>
        /// <param name="additionalYouTubeIds">A collection of YouTube video IDs</param>
        /// <param name="additionalKeyValueTags">A collection of key value tags</param>
        /// <returns>True if the request was accepted</returns>
        public bool RunCreate(WorkshopItemPreviewFile[] additionalPreviews, string[] additionalYouTubeIds, WorkshopItemKeyValueTag[] additionalKeyValueTags)
        {
            if (_itemData.IsValid)
                return _itemData.Create(additionalPreviews, additionalYouTubeIds, additionalKeyValueTags, CompletedHandler, UploadStartedHandler, FileCreatedHandler);
            else
                return false;
        }
        /// <summary>
        /// Gets the current progress on the update process
        /// </summary>
        /// <param name="progress">the progress value from 0 to 1</param>
        /// <returns>The current update status</returns>
        public EItemUpdateStatus GetUpdateProgress(out float progress)
        {
            progress = 0;
            if (_updateHandle.HasValue)
                return API.UserGeneratedContent.Client.GetItemUpdateProgress(_updateHandle.Value, out progress);
            else
                return EItemUpdateStatus.k_EItemUpdateStatusInvalid;
        }

        private void CompletedHandler(WorkshopItemDataCreateStatus arg)
        {
            _updateHandle = null;
            Completed?.Invoke(this, arg);
        }
        private void UploadStartedHandler(UGCUpdateHandle_t arg)
        {
            _updateHandle = arg;
            UpdateStarted?.Invoke(this, arg);
        }
        private void FileCreatedHandler(CreateItemResult_t arg) => FileCreated?.Invoke(this, arg);
    }
}
#endif