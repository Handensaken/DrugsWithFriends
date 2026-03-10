#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.IO;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a structure for managing downloadable content (DLC) within the Steamworks integration.
    /// <para>This structure provides information and operations related to a specific DLC, such as its ID, availability, installation status, subscription status, and more. It allows developers to retrieve and interact with DLC content configured in the Steam Developer Portal.</para>
    /// </summary>
    [Serializable]
    public struct DlcData : IEquatable<AppId_t>, IEquatable<uint>, IEquatable<AppData>, IComparable<AppData>,
        IComparable<AppId_t>, IComparable<uint>
    {
        /// <summary>
        /// The unique identifier representing the downloadable content (DLC).
        /// </summary>
        [SerializeField] private uint id;

        /// <summary>
        /// Represents the unique application identifier (App ID) for a downloadable content (DLC).
        /// </summary>
        public readonly AppId_t AppId => new AppId_t(id);

        /// <summary>
        /// Gets the unique identifier of the downloadable content (DLC).
        /// </summary>
        public readonly uint Id => id;

        /// <summary>
        /// Indicates whether the downloadable content (DLC) is currently available for use or not.
        /// </summary>
        public readonly bool Available
        {
            get
            {
                if (API.App.DlcAppCash.TryGetValue(Id, out var cash))
                    return cash.available;
                else
                {
                    bool available = false;
                    var count = SteamApps.GetDLCCount();
                    for (int i = 0; i < count; i++)
                    {
                        if (SteamApps.BGetDLCDataByIndex(i, out var pAppID, out var pAvailable, out var pName, 512))
                        {
                            API.App.DlcAppCash[pAppID.m_AppId] = (pName, pAvailable);

                            if (pAppID.m_AppId == id)
                                available = pAvailable;
                        }
                    }
                    return available;
                }
            }
        }

        /// <summary>
        /// The name of the downloadable content (DLC) associated with this instance.
        /// If the name is not cached, it retrieves the name from Steam data and updates the cache.
        /// </summary>
        public readonly string Name
        {
            get
            {
                if (API.App.DlcAppCash.TryGetValue(Id, out var cash))
                    return cash.name;
                else
                {
                    string name = "None Found";
                    var count = SteamApps.GetDLCCount();
                    for (int i = 0; i < count; i++)
                    {
                        if (SteamApps.BGetDLCDataByIndex(i, out var pAppID, out var pAvailable, out var pName, 512))
                        {
                            API.App.DlcAppCash[pAppID.m_AppId] = (pName, pAvailable);

                            if (pAppID.m_AppId == id)
                                name = pName;
                        }
                    }
                    return name;
                }
            }
        }

        /// <summary>
        /// Indicates whether the user currently subscribes to the downloadable content (DLC).
        /// </summary>
        public readonly bool IsSubscribed => SteamApps.BIsSubscribedApp(this);

        /// <summary>
        /// Indicates whether the downloadable content (DLC) associated with this instance is currently installed.
        /// </summary>
        public readonly bool IsInstalled => SteamApps.BIsDlcInstalled(this);

        /// <summary>
        /// The directory where the downloadable content (DLC) is installed on the local system.
        /// </summary>
        public readonly DirectoryInfo InstallDirectory
        {
            get
            {
                if (SteamApps.GetAppInstallDir(this, out var path, 2048) > 0)
                {
                    return new DirectoryInfo(path.Trim());
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Represents the current download progress of the downloadable content (DLC) as a value between 0 and 1.
        /// Returns 0 if the DLC is not being downloaded.
        /// </summary>
        public readonly float DownloadProgress
        {
            get
            {
                var isDownloading = SteamApps.GetDlcDownloadProgress(this, out ulong current, out ulong total);
                return isDownloading ? Convert.ToSingle(current / (double)total) : 0f;
            }
        }

        /// <summary>
        /// Represents the earliest recorded time at which the associated downloadable content (DLC) was purchased.
        /// </summary>
        public readonly DateTime EarliestPurchaseTime
        {
            get
            {
                var val = SteamApps.GetEarliestPurchaseUnixTime(this);
                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(val);
                return dateTime;
            }
        }

        /// <summary>
        /// Initiates the installation process for the specified DLC.
        /// </summary>
        public readonly void Install()
        {
            SteamApps.InstallDLC(this);
        }

        /// <summary>
        /// Initiates the uninstallation process for the specified DLC.
        /// </summary>
        public readonly void Uninstall()
        {
            SteamApps.UninstallDLC(this);
        }

        /// <summary>
        /// Opens the Steam overlay to the store page for the current DLC,
        /// using the specified overlay behaviour flags.
        /// </summary>
        /// <param name="flag">The <see cref="EOverlayToStoreFlag"/> determining how the store page will be displayed.</param>
        public readonly void OpenStore(EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) =>
            SteamFriends.ActivateGameOverlayToStore(this, flag);

        /// <summary>
        /// Represents downloadable content (DLC) data, including its unique identifier, availability status, and name.
        /// </summary>
        public DlcData(AppId_t id, bool available, string name)
        {
            this.id = id.m_AppId;
            API.App.DlcAppCash[id.m_AppId] = (name, available);
        }

        /// <summary>
        /// Retrieves a <see cref="DlcData"/> instance based on a specified application identifier.
        /// </summary>
        /// <param name="appId">The unique identifier of the application associated with the DLC.</param>
        /// <returns>A <see cref="DlcData"/> structure representing the specified DLC.</returns>
        public static DlcData Get(uint appId) => appId;

        /// <summary>
        /// Retrieves a <see cref="DlcData"/> instance using the specified <see cref="AppId_t"/>.
        /// </summary>
        /// <param name="appId">The Steam App ID associated with the desired DLC.</param>
        /// <returns>A <see cref="DlcData"/> instance representing the specified DLC.</returns>
        public static DlcData Get(AppId_t appId) => appId;

        /// <summary>
        /// Retrieves the <see cref="DlcData"/> associated with the given <see cref="AppData"/> object.
        /// </summary>
        /// <param name="appData">The <see cref="AppData"/> object representing the Steam application.</param>
        /// <returns>The corresponding <see cref="DlcData"/> for the provided <see cref="AppData"/>.</returns>
        public static DlcData Get(AppData appData) => appData.AppId;

        #region Boilerplate

        public override string ToString()
        {
            return id.ToString();
        }

        public readonly int CompareTo(AppData other)
        {
            return id.CompareTo(other.AppId.m_AppId);
        }

        public readonly int CompareTo(AppId_t other)
        {
            return id.CompareTo(other.m_AppId);
        }

        public readonly int CompareTo(uint other)
        {
            return id.CompareTo(other);
        }

        public readonly bool Equals(uint other)
        {
            return id.Equals(other);
        }

        public readonly override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public readonly bool Equals(AppId_t other)
        {
            return id.Equals(other.m_AppId);
        }

        public readonly bool Equals(AppData other)
        {
            return id.Equals(other.AppId.m_AppId);
        }

        public static bool operator ==(DlcData l, DlcData r) => l.id == r.id;
        public static bool operator ==(DlcData l, AppData r) => l.id == r.Id;
        public static bool operator ==(DlcData l, AppId_t r) => l.id == r.m_AppId;
        public static bool operator ==(AppId_t l, DlcData r) => l.m_AppId == r.id;
        public static bool operator !=(DlcData l, DlcData r) => l.id != r.id;
        public static bool operator !=(DlcData l, AppData r) => l.id != r.AppId.m_AppId;
        public static bool operator !=(DlcData l, AppId_t r) => l.id != r.m_AppId;
        public static bool operator !=(AppId_t l, DlcData r) => l.m_AppId != r.id;

        public static implicit operator uint(DlcData c) => c.id;
        public static implicit operator DlcData(uint id) => new DlcData { id = id };
        public static implicit operator AppId_t(DlcData c) => c.AppId;
        public static implicit operator DlcData(AppId_t id) => new DlcData { id = id.m_AppId };
        public static implicit operator DlcData(AppData id) => new DlcData { id = id.Id };
        public static implicit operator AppData(DlcData id) => AppData.Get(id.id);
        #endregion
    }
}
#endif