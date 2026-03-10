#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides static methods and properties for interacting with the Steam Remote Storage service,
    /// enabling functionality for managing files stored in the Steam Cloud.
    /// </summary>
    public static class RemoteStorage
    {
        /// <summary>
        /// Provides methods and properties for interacting with the Steam Remote Storage service from the client's perspective.
        /// Enables functionality for managing files stored in the Steam Cloud, including reading, writing, deleting,
        /// and sharing files, as well as handling file changes and stream operations.
        /// </summary>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                _remoteStorageFileReadAsyncCompleteT = null;
                _remoteStorageFileShareResultT = null;
                _remoteStorageFileWriteAsyncCompleteT = null;
                _remoteStorageDownloadUgcResultT = null;
            }

            /// <summary>
            /// Indicates whether Steam Cloud storage is enabled for the user's account.
            /// Returns true if Steam Cloud is enabled for the account, otherwise false.
            /// </summary>
            public static bool IsEnabledForAccount => SteamRemoteStorage.IsCloudEnabledForAccount();

            /// <summary>
            /// Indicates whether Steam Cloud storage is enabled for the application.
            /// Returns true if Steam Cloud is enabled for the application, otherwise false.
            /// Can also be set to enable or disable Steam Cloud for the application.
            /// </summary>
            public static bool IsEnabledForApp
            {
                get => SteamRemoteStorage.IsCloudEnabledForApp();
                set => SteamRemoteStorage.SetCloudEnabledForApp(value);
            }

            /// <summary>
            /// Determines whether Steam Cloud storage is currently enabled for both the user's account
            /// and the specific application. Returns true if both account-level and app-level
            /// Steam Cloud settings are enabled, otherwise false.
            /// </summary>
            public static bool IsEnabled => IsEnabledForAccount && IsEnabledForApp;

            private static CallResult<RemoteStorageFileReadAsyncComplete_t> _remoteStorageFileReadAsyncCompleteT;
            private static CallResult<RemoteStorageFileShareResult_t> _remoteStorageFileShareResultT;
            private static CallResult<RemoteStorageFileWriteAsyncComplete_t> _remoteStorageFileWriteAsyncCompleteT;
            private static CallResult<RemoteStorageDownloadUGCResult_t> _remoteStorageDownloadUgcResultT;

            /// <summary>
            /// Deletes a file from the local disk and propagates that delete to the cloud.
            /// </summary>
            /// <remarks>
            /// This is meant to be used when a user actively deletes a file. Use FileForget if you want to remove a file from the Steam Cloud but retain it on the user's local disk.
            /// </remarks>
            /// <param name="file">The name or path of the file to delete.</param>
            /// <returns>Returns true if the file was successfully deleted; otherwise, false.</returns>
            public static bool FileDelete(string file) => SteamRemoteStorage.FileDelete(file);

            /// <summary>
            /// Checks if a specified file exists in the Steam Cloud storage.
            /// </summary>
            /// <param name="file">The name or path of the file to check.</param>
            /// <returns>Returns true if the file exists; otherwise, false.</returns>
            public static bool FileExists(string file) => SteamRemoteStorage.FileExists(file);

            /// <summary>
            /// Removes a file from remote storage while keeping it on the local disk for continued access.
            /// The file will no longer be synchronised to the Cloud unless explicitly rewritten.
            /// </summary>
            /// <remarks>
            /// This method is useful for freeing up Cloud storage space without requiring user intervention.
            /// It enables continued use of FileWrite functionality even when Cloud storage is full.
            /// Rewriting the file via FileWrite is necessary to persist it back to the Cloud after forgetting it.
            /// </remarks>
            /// <param name="file">The name or path of the file to be forgotten from remote storage.</param>
            /// <returns>Returns true if the file was successfully forgotten from remote storage; otherwise, false.</returns>
            public static bool FileForget(string file) => SteamRemoteStorage.FileForget(file);

            /// <summary>
            /// Reads the contents of a binary file into a byte array and then closes the file.
            /// </summary>
            /// <remarks>
            /// This method is used for retrieving the binary data of a file stored in the Steam Cloud.
            /// </remarks>
            /// <param name="file">The name or path of the file to be read.</param>
            /// <returns>Returns a byte array containing the contents of the file, or an empty array if the file could not be read.</returns>
            public static byte[] FileRead(string file)
            {
                var size = SteamRemoteStorage.GetFileSize(file);
                var results = new byte[size];
                SteamRemoteStorage.FileRead(file, results, size);
                return results;
            }

            /// <summary>
            /// Reads the contents of a file from the remote storage as a string using the specified text encoding.
            /// </summary>
            /// <param name="fileName">The name of the file to read.</param>
            /// <param name="encoding">The text encoding to be used for decoding the file content (e.g. UTF8).</param>
            /// <returns>Returns the content of the file as a string.</returns>
            public static string FileReadString(string fileName, System.Text.Encoding encoding)
            {
                var size = SteamRemoteStorage.GetFileSize(fileName);

                var buffer = new byte[size];
                SteamRemoteStorage.FileRead(fileName, buffer, buffer.Length);
                return encoding.GetString(buffer);
            }

            /// <summary>
            /// Reads the contents of a specified file and deserializes it into a JSON object of the specified type.
            /// </summary>
            /// <typeparam name="T">The type into which the JSON data should be deserialized.</typeparam>
            /// <param name="fileName">The name of the file to be read.</param>
            /// <param name="encoding">The encoding used to interpret the file's contents, typically <see cref="System.Text.Encoding.UTF8"/>.</param>
            /// <returns>Returns an object of type T if the file is read and deserialized successfully; otherwise, returns the default value of type T.</returns>
            public static T FileReadJson<T>(string fileName, System.Text.Encoding encoding)
            {
                var size = SteamRemoteStorage.GetFileSize(fileName);

                if (size <= 0)
                    return default;

                var buffer = new byte[size];
                SteamRemoteStorage.FileRead(fileName, buffer, buffer.Length);
                var jsonString = encoding.GetString(buffer);

                return JsonUtility.FromJson<T>(jsonString);
            }

            /// <summary>
            /// Initiates an asynchronous read operation for a specified file.
            /// </summary>
            /// <remarks>
            /// This method reads the contents of the file asynchronously and processes the result through a user-provided callback.
            /// It is useful for retrieving file data without blocking the main thread.
            /// </remarks>
            /// <param name="file">The name or path of the file to read.</param>
            /// <param name="callback">A callback invoked with the file data and a status indicator once the read operation completes.</param>
            public static void FileReadAsync(string file, Action<byte[], bool> callback)
            {
                if (callback == null)
                    return;

                _remoteStorageFileReadAsyncCompleteT ??= CallResult<RemoteStorageFileReadAsyncComplete_t>.Create();

                var size = SteamRemoteStorage.GetFileSize(file);
                var handle = SteamRemoteStorage.FileReadAsync(file, 0, (uint)size);
                _remoteStorageFileReadAsyncCompleteT.Set(handle, (r, e) =>
                {
                    if (!e && r.m_eResult == EResult.k_EResultOK)
                    {
                        var results = new byte[size];
                        SteamRemoteStorage.FileReadAsyncComplete(r.m_hFileReadAsync, results, r.m_cubRead);
                        callback.Invoke(results, false);
                    }
                    else
                    {
                        callback.Invoke(Array.Empty<byte>(), e);
                    }
                });
            }

            /// <summary>
            /// Shares a file from the local disk to the Steam Cloud and provides the share result through a callback.
            /// </summary>
            /// <remarks>
            /// This function allows the user to share a file with the Steam Cloud. The result of the operation is supplied via the provided callback function.
            /// </remarks>
            /// <param name="file">The name or path of the file to be shared.</param>
            /// <param name="callback">The callback to invoke when the file share operation completes. It provides the result of the operation and a success flag.</param>
            public static void FileShare(string file, Action<RemoteStorageFileShareResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _remoteStorageFileShareResultT ??= CallResult<RemoteStorageFileShareResult_t>.Create();

                var handle = SteamRemoteStorage.FileShare(file);
                _remoteStorageFileShareResultT.Set(handle, callback.Invoke);
            }

            /// <summary>
            /// Creates a new file, writes the specified bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
            /// </summary>
            /// <param name="file">The name of the file to write to.</param>
            /// <param name="data">The bytes to be written to the file.</param>
            /// <returns>Returns true if the file was successfully written; otherwise, false.</returns>
            public static bool FileWrite(string file, byte[] data) =>
                SteamRemoteStorage.FileWrite(file, data, data.Length);

            /// <summary>
            /// Writes a file to the Steam Remote Storage, encoding the provided text using the specified encoding. If the target file already exists, it is overwritten.
            /// </summary>
            /// <remarks>
            /// This method may return false if any of the following conditions are met:
            /// - The file exceeds the maximum allowed size for Steam Remote Storage (e.g. 100 MiB).
            /// - The data to write is null or empty.
            /// - An invalid path or filename is specified. File names must be valid across all supported platforms and file systems.
            /// - The user's Steamworks Cloud storage quota is exceeded (too many files or insufficient available space).
            /// - The destination storage location is read-only or otherwise inaccessible.
            /// </remarks>
            /// <param name="file">The path or name of the file to write to Steam Remote Storage.</param>
            /// <param name="body">The text content to encode and save to the file.</param>
            /// <param name="encoding">The encoding to use for converting the text content to bytes, commonly <see cref="System.Text.Encoding.UTF8"/>.</param>
            /// <returns>Returns true if the file was successfully written; otherwise, false.</returns>
            public static bool FileWrite(string file, string body, System.Text.Encoding encoding)
            {
                var data = encoding.GetBytes(body);
                return FileWrite(file, data);
            }

            /// <summary>
            /// Writes data to a specified file in the Steam Cloud.
            /// </summary>
            /// <remarks>
            /// This method is used to save data to a file that is synchronised with the Steam Cloud storage,
            /// allowing the data to be accessed across multiple devices or sessions.
            /// </remarks>
            /// <param name="file">The name or path of the file to write to.</param>
            /// <param name="body">The byte array containing the data to be written to the file.</param>
            /// <returns>Returns true if the file was successfully written; otherwise, false.</returns>
            public static bool FileWrite(string file, string body)
            {
                var data = System.Text.Encoding.UTF8.GetBytes(body);
                return FileWrite(file, data);
            }

            /// <summary>
            /// Writes the specified object serialized as a JSON string to a file and saves it to the target location. If the file already exists, it is overwritten.
            /// </summary>
            /// <remarks>
            /// The method may fail under certain conditions such as
            /// - The file exceeds the maximum allowed size of 100MiB defined by k_unMaxCloudFileChunkSize.
            /// - An invalid path or filename is provided. File names must be valid on all supported operating systems and file systems.
            /// - The user's Steam Cloud storage quota is exceeded due to insufficient space or too many files.
            /// - Steam is unable to write to the disk, possibly due to read-only restrictions on the target location.
            /// </remarks>
            /// <param name="fileName">The name or path of the file where the object will be written.</param>
            /// <param name="jsonObject">The object to be serialized into JSON format and saved to the file. It must be a type supported by UnityEngine.JsonUtility.</param>
            /// <param name="encoding">The text encoding to use, typically <see cref="System.Text.Encoding.UTF8"/>.</param>
            /// <returns>Returns true if the write operation is successful; otherwise, false.</returns>
            public static bool FileWrite(string fileName, object jsonObject, System.Text.Encoding encoding)
            {
                return FileWrite(fileName, JsonUtility.ToJson(jsonObject), encoding);
            }


            /// <summary>
            /// Writes data to a file in the Steam Cloud or local storage.
            /// </summary>
            /// <remarks>
            /// This method allows you to save data to a specified file, managing Steam Cloud storage where applicable.
            /// It can write both raw byte data and encoded text or JSON representations, depending on the overload used.
            /// </remarks>
            /// <param name="fileName">The name or path of the file to write to.</param>
            /// <param name="jsonObject">The byte array containing the data to write.</param>
            /// <returns>Returns true if the file was successfully written; otherwise, false.</returns>
            public static bool FileWrite(string fileName, object jsonObject)
            {
                return FileWrite(fileName, JsonUtility.ToJson(jsonObject), System.Text.Encoding.UTF8);
            }

            /// <summary>
            /// Creates or overwrites a file in the Steam Cloud and writes the provided raw byte data to it asynchronously.
            /// </summary>
            /// <param name="file">The name or path of the file to create or overwrite.</param>
            /// <param name="data">The raw byte data to write to the file.</param>
            /// <param name="callback">A callback function invoked when the asynchronous operation completes, providing operation results and a success flag.</param>
            public static void FileWriteAsync(string file, byte[] data,
                Action<RemoteStorageFileWriteAsyncComplete_t, bool> callback)
            {
                if (callback == null)
                    return;

                _remoteStorageFileWriteAsyncCompleteT ??= CallResult<RemoteStorageFileWriteAsyncComplete_t>.Create();

                var handle = SteamRemoteStorage.FileWriteAsync(file, data, (uint)data.Length);
                _remoteStorageFileWriteAsyncCompleteT.Set(handle, callback.Invoke);
            }

            /// <summary>
            /// Writes the specified file to the Steam Cloud asynchronously with the provided content and encoding.
            /// </summary>
            /// <remarks>
            /// This method asynchronously writes a file to the Steam Cloud storage. A callback is invoked upon the completion of the operation to indicate success or failure.
            /// </remarks>
            /// <param name="file">The name or path of the file to write.</param>
            /// <param name="body">The content to write to the file.</param>
            /// <param name="encoding">The encoding used to convert the content to bytes.</param>
            /// <param name="callback">The callback that is invoked when the async operation completes, providing a status result and a success flag.</param>
            public static void FileWriteAsync(string file, string body, System.Text.Encoding encoding, Action<RemoteStorageFileWriteAsyncComplete_t, bool> callback)
            {
                var data = encoding.GetBytes(body);
                FileWriteAsync(file, data, callback);
            }

            /// <summary>
            /// Asynchronously writes data to a file in Steam Cloud storage.
            /// </summary>
            /// <remarks>
            /// This method is used to save data to a file in the Steam Cloud. It supports writing data
            /// in JSON format and allows specifying the encoding. A callback is invoked upon completion
            /// to notify whether the operation was successful.
            /// </remarks>
            /// <param name="fileName">The name or path of the file to write.</param>
            /// <param name="jsonObject">The object to serialize into JSON and write to the file.</param>
            /// <param name="encoding">The encoding format to use for the file content.</param>
            /// <param name="callback">A callback that receives the result of the write operation and a success flag.</param>
            public static void FileWriteAsync(string fileName, object jsonObject, System.Text.Encoding encoding, Action<RemoteStorageFileWriteAsyncComplete_t, bool> callback)
            {
                FileWriteAsync(fileName, JsonUtility.ToJson(jsonObject), encoding, callback);
            }
            /// <summary>
            /// Cancels a file write stream started by FileWriteStreamOpen.
            /// </summary>
            /// <param name="handle"></param>
            /// <returns></returns>
            public static bool FileWriteStreamCancel(UGCFileWriteStreamHandle_t handle) => SteamRemoteStorage.FileWriteStreamCancel(handle);
            /// <summary>
            /// Closes a file write stream started by FileWriteStreamOpen.
            /// </summary>
            /// <param name="handle"></param>
            /// <returns></returns>
            public static bool FileWriteStreamClose(UGCFileWriteStreamHandle_t handle) => SteamRemoteStorage.FileWriteStreamClose(handle);
            /// <summary>
            /// Creates a new file output stream allowing you to stream out data to the Steam Cloud file in chunks. If the target file already exists, it is not overwritten until FileWriteStreamClose has been called.
            /// </summary>
            /// <remarks>
            /// To write data out to this stream, you can use FileWriteStreamWriteChunk, and then to close or cancel you use FileWriteStreamClose and FileWriteStreamCancel respectively.
            /// </remarks>
            /// <param name="file"></param>
            /// <returns></returns>
            public static UGCFileWriteStreamHandle_t FileWriteStreamOpen(string file) => SteamRemoteStorage.FileWriteStreamOpen(file);
            /// <summary>
            /// Writes a blob of data to the file write stream.
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public static bool FileWriteStreamWriteChunk(UGCFileWriteStreamHandle_t handle, byte[] data) => SteamRemoteStorage.FileWriteStreamWriteChunk(handle, data, data.Length);

            /// <summary>
            /// Retrieves the number of cached UGC (User Generated Content) items available for the current user.
            /// </summary>
            /// <returns>Returns the total count of cached UGC items as an integer.</returns>
            public static int GetCachedUgcCount() => SteamRemoteStorage.GetCachedUGCCount();

            /// <summary>
            /// Retrieves the cached UGC (User-Generated Content) handle stored at the specified index.
            /// </summary>
            /// <remarks>
            /// This method is used to access a cached UGC handle previously stored by the Steam Remote Storage system.
            /// It allows retrieval of specific UGC content when indexed caching is used.
            /// </remarks>
            /// <param name="index">The index of the cached UGC content to retrieve.</param>
            /// <returns>Returns a <see cref="UGCHandle_t"/> representing the handle to the cached UGC content.</returns>
            public static UGCHandle_t GetCachedUgcHandle(int index) => SteamRemoteStorage.GetCachedUGCHandle(index);

            /// <summary>
            /// Retrieves all cached UGC (User Generated Content) handles stored locally.
            /// </summary>
            /// <remarks>
            /// This method queries the number of cached UGC items available and retrieves their handles.
            /// These handles can be used for further processing or operations on the cached UGC items.
            /// </remarks>
            /// <returns>An array of UGCHandle_t representing the cached UGC handles.</returns>
            public static UGCHandle_t[] GetCashedUgcHandles()
            {
                var count = SteamRemoteStorage.GetCachedUGCCount();
                var results = new UGCHandle_t[count];
                for (int i = 0; i < count; i++)
                {
                    results[i] = SteamRemoteStorage.GetCachedUGCHandle(i);
                }

                return results;
            }
            /// <summary>
            /// Gets the total number of local files synchronised by Steam Cloud.
            /// </summary>
            /// <returns></returns>
            public static int GetFileCount() => SteamRemoteStorage.GetFileCount();
            /// <summary>
            /// Gets a collection containing information about all the files stored on the Steam Cloud system
            /// </summary>
            /// <returns></returns>
            public static RemoteStorageFile[] GetFiles()
            {
                var count = SteamRemoteStorage.GetFileCount();
                var results = new RemoteStorageFile[count];
                for (int i = 0; i < count; i++)
                {
                    var name = SteamRemoteStorage.GetFileNameAndSize(i, out int size);
                    var time = new DateTime(1970, 1, 1).AddSeconds(SteamRemoteStorage.GetFileTimestamp(name));

                    results[i] = new RemoteStorageFile
                    {
                        name = name,
                        size = size,
                        Timestamp = time
                    };
                }
                return results;
            }
            /// <summary>
            /// Returns the subset of files found on the user's Steam Cloud that end with the specified value
            /// </summary>
            /// <param name="extension"></param>
            /// <returns></returns>
            public static RemoteStorageFile[] GetFiles(string extension)
            {
                var count = SteamRemoteStorage.GetFileCount();
                var results = new RemoteStorageFile[count];
                int found = 0;
                for (int i = 0; i < count; i++)
                {
                    var name = SteamRemoteStorage.GetFileNameAndSize(i, out int size);

                    if (name.ToLower().EndsWith(extension))
                    {
                        var time = new DateTime(1970, 1, 1).AddSeconds(SteamRemoteStorage.GetFileTimestamp(name));

                        results[found] = new RemoteStorageFile
                        {
                            name = name,
                            size = size,
                            Timestamp = time
                        };

                        found++;
                    }
                }
                Array.Resize(ref results, found);
                return results;
            }
            /// <summary>
            /// Gets the specified file's last modified timestamp
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public static DateTime GetFileTimestamp(string name) => new DateTime(1970, 1, 1).AddSeconds(SteamRemoteStorage.GetFileTimestamp(name));
            /// <summary>
            /// Note: only applies to applications flagged as supporting dynamic Steam Cloud sync.
            /// </summary>
            /// <returns></returns>
            public static int GetLocalFileChangeCount() => SteamRemoteStorage.GetLocalFileChangeCount();
            /// <summary>
            /// Note: only applies to applications flagged as supporting dynamic Steam Cloud sync.
            /// </summary>
            /// <remarks>
            /// <para>
            /// After calling GetLocalFileChangeCount, use this method to iterate over the changes. The changes described have already been made to local files. Your application should take appropriate action to reload state from the disk and possibly notify the user.
            /// </para>
            /// <para>
            /// For example, The local system had been suspended, during which time the user played elsewhere and uploaded changes to the Steam Cloud. On resume, Steam downloads those changes to the local system before resuming the application. The application receives a RemoteStorageLocalFileChange_t and uses GetLocalFileChangeCount and GetLocalFileChange to iterate those changes. Depending on the application structure and the nature of the changes, the application could:
            /// </para>
            /// <list type="bullet">
            /// <item>
            /// Re-load game progress to resume at exactly the point where the user was when they exited the game on the other device
            /// </item>
            /// <item>
            /// Notify the user of any synchronised changes that don't require reloading
            /// </item>
            /// <item>
            /// etc
            /// </item>
            /// </list>
            /// </remarks>
            /// <param name="index"></param>
            /// <param name="changeType"></param>
            /// <param name="pathType"></param>
            /// <returns></returns>
            public static string GetLocalFileChange(int index, out ERemoteStorageLocalFileChange changeType, out ERemoteStorageFilePathType pathType) => SteamRemoteStorage.GetLocalFileChange(index, out changeType, out pathType);
            /// <summary>
            /// Gets the number of bytes available and used on the users' Steam Cloud storage.
            /// </summary>
            /// <param name="totalBytes"></param>
            /// <param name="remainingBytes"></param>
            /// <returns></returns>
            public static bool GetQuota(out ulong totalBytes, out ulong remainingBytes) => SteamRemoteStorage.GetQuota(out totalBytes, out remainingBytes);

            /// <summary>
            /// Retrieves the synchronisation platforms for a specific file in the Steam Cloud.
            /// </summary>
            /// <remarks>
            /// This method is useful to determine which platforms are set to synchronise a particular file.
            /// The synchronisation platforms could include Windows, OSX, Linux, and others supported by the Steam Cloud.
            /// </remarks>
            /// <param name="file">The name or path of the file for which to retrieve the synchronisation platforms.</param>
            /// <returns>Returns an <see cref="ERemoteStoragePlatform"/> enum value indicating the platforms where the file is synchronised.</returns>
            public static ERemoteStoragePlatform GetSyncPlatforms(string file) => SteamRemoteStorage.GetSyncPlatforms(file);

            /// <summary>
            /// Retrieves details about a specific User Generated Content (UGC) item.
            /// </summary>
            /// <remarks>
            /// This method fetches metadata about a UGC item, such as its associated App ID, name, file size, and owner.
            /// </remarks>
            /// <param name="handle">The handle identifying the UGC item.</param>
            /// <param name="appId">The App ID associated with the UGC item.</param>
            /// <param name="name">The name of the UGC item.</param>
            /// <param name="size">The size of the UGC item in bytes.</param>
            /// <param name="owner">The Steam ID of the owner of the UGC item.</param>
            /// <returns>Returns true if the details were successfully retrieved; otherwise, false.</returns>
            public static bool GetUgcDetails(UGCHandle_t handle, out AppId_t appId, out string name, out int size, out CSteamID owner) => SteamRemoteStorage.GetUGCDetails(handle, out appId, out name, out size, out owner);

            /// <summary>
            /// Retrieves the download progress of a specific UGC (User-Generated Content) file.
            /// </summary>
            /// <remarks>
            /// This method provides information about the current download progress of a UGC file, including the number of bytes downloaded and the total expected bytes.
            /// </remarks>
            /// <param name="handle">The handle representing the UGC file.</param>
            /// <param name="downloaded">Outputs the number of bytes downloaded so far.</param>
            /// <param name="expected">Outputs the total number of bytes expected for the file.</param>
            /// <returns>Returns true if the download progress was successfully retrieved; otherwise, false.</returns>
            public static bool GetUgcDownloadProgress(UGCHandle_t handle, out int downloaded, out int expected) => SteamRemoteStorage.GetUGCDownloadProgress(handle, out downloaded, out expected);

            /// <summary>
            /// Sets the platform synchronisation options for a specified file.
            /// </summary>
            /// <remarks>
            /// This method allows the user to specify which platforms the file will synchronise with the Steam Cloud.
            /// </remarks>
            /// <param name="file">The name or path of the file whose synchronisation settings are being modified.</param>
            /// <param name="platform">The platform or combination of platforms to set for synchronisation, represented as <c>ERemoteStoragePlatform</c> flags.</param>
            /// <returns>Returns true if the synchronisation platforms were successfully set; otherwise, false.</returns>
            public static bool SetSyncPlatforms(string file, ERemoteStoragePlatform platform) => SteamRemoteStorage.SetSyncPlatforms(file, platform);

            /// <summary>
            /// Downloads a UGC (User Generated Content) item from the Steam Cloud.
            /// </summary>
            /// <remarks>
            /// This method initiates a download of the specified UGC item and invokes the provided callback upon completion.
            /// </remarks>
            /// <param name="handle">The unique handle identifying the UGC item to download.</param>
            /// <param name="priority">The priority level of the download. A lower value indicates a higher priority.</param>
            /// <param name="callback">
            /// A callback function to handle the result of the download operation. The callback receives the result as a
            /// <see cref="RemoteStorageDownloadUGCResult_t"/> and a boolean indicating success or failure.
            /// </param>
            public static void UgcDownload(UGCHandle_t handle, uint priority, Action<RemoteStorageDownloadUGCResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (_remoteStorageDownloadUgcResultT != null)
                {
                }
                else
                    _remoteStorageDownloadUgcResultT = CallResult<RemoteStorageDownloadUGCResult_t>.Create();

                var callbackHandle = SteamRemoteStorage.UGCDownload(handle, priority);
                _remoteStorageDownloadUgcResultT.Set(callbackHandle, callback.Invoke);
            }

            /// <summary>
            /// Downloads a Steam Workshop item to a specified local storage location.
            /// </summary>
            /// <remarks>
            /// This method is used to retrieve a subscribed Workshop item and save it to a specific location on the local disk. It allows you to prioritise the download operation and specify a callback to handle the result.
            /// </remarks>
            /// <param name="handle">The handle to the Workshop item (UGC) to download.</param>
            /// <param name="location">The file path where the Workshop item will be saved.</param>
            /// <param name="priority">The priority of the download operation. Higher values indicate higher priority.</param>
            /// <param name="callback">Callback to execute upon completion of the download, providing the result and success status.</param>
            public static void UgcDownloadToLocation(UGCHandle_t handle, string location, uint priority, Action<RemoteStorageDownloadUGCResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                _remoteStorageDownloadUgcResultT ??= CallResult<RemoteStorageDownloadUGCResult_t>.Create();

                var callbackHandle = SteamRemoteStorage.UGCDownloadToLocation(handle, location, priority);
                _remoteStorageDownloadUgcResultT.Set(callbackHandle, callback.Invoke);
            }

            /// <summary>
            /// Reads data from the given UGC (User Generated Content) handle.
            /// </summary>
            /// <remarks>
            /// The method retrieves the details of the specified UGC handle and reads its contents into a byte array.
            /// </remarks>
            /// <param name="handle">The handle of the UGC to read.</param>
            /// <returns>Returns a byte array containing the data read from the UGC handle.</returns>
            public static byte[] UgcRead(UGCHandle_t handle)
            {
                SteamRemoteStorage.GetUGCDetails(handle, out _, out _, out int size, out _);
                var results = new byte[size];
                SteamRemoteStorage.UGCRead(handle, results, size, 0, EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);
                return results;
            }
        }
    }
}
#endif