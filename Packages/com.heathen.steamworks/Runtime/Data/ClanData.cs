#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Clan (Group) and provides access to its related data and functionalities.
    /// </summary>
    /// <remarks>
    /// This structure is used to interact with Steam Clans, also referred to as Steam Groups.
    /// It extends the functionalities of <see cref="CSteamID"/> to include clan-specific features.
    /// <para>
    /// Provides properties and methods for accessing clan details, such as its name, tag, owner, officers, members,
    /// and other relevant information. Utility methods for creating, comparing, and interacting with clans
    /// are also included.
    /// </para>
    /// </remarks>
    [Serializable]
    public struct ClanData : IEquatable<CSteamID>, IEquatable<ClanData>, IEquatable<ulong>, IComparable<CSteamID>,
        IComparable<ClanData>, IComparable<ulong>
    {
        /// <summary>
        /// The unique 64-bit identifier representing the Steam clan.
        /// </summary>
        [SerializeField] private ulong id;

        /// <summary>
        /// Represents the Steam identifier (CSteamID) associated with this clan.
        /// </summary>
        public readonly CSteamID SteamId => new CSteamID(id);

        /// <summary>
        /// Represents the account identifier associated with the clan's Steam data.
        /// </summary>
        public readonly AccountID_t AccountId => SteamId.GetAccountID();

        /// <summary>
        /// The identifier representing the account ID of the friend associated with this clan.
        /// </summary>
        public readonly uint FriendId => SteamId.GetAccountID().m_AccountID;

        /// <summary>
        /// Indicates whether the current clan data instance is valid.
        /// A valid clan is represented by a non-nil Steam ID, belongs to the 'k_EAccountTypeClan' account type,
        /// and exists in the public universe.
        /// </summary>
        public readonly bool IsValid
        {
            get
            {
                var sID = SteamId;

                if (sID == CSteamID.Nil
                    || sID.GetEAccountType() != EAccountType.k_EAccountTypeClan
                    || sID.GetEUniverse() != EUniverse.k_EUniversePublic)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Provides the avatar image associated with the clan as a Texture2D.
        /// This property retrieves the preloaded avatar for the clan represented by this instance.
        /// </summary>
        public readonly Texture2D Icon => API.Friends.Client.GetLoadedAvatar(this);

        /// <summary>
        /// The display name of the Steam clan associated with this instance.
        /// </summary>
        public readonly string Name => API.Clans.Client.GetName(this);

        /// <summary>
        /// The tag or shorthand display name associated with the Steam clan.
        /// </summary>
        public readonly string Tag => API.Clans.Client.GetTag(this);

        /// <summary>
        /// The user who owns the clan represented by this instance.
        /// </summary>
        public readonly UserData Owner => API.Clans.Client.GetOwner(this);

        /// <summary>
        /// Provides access to the list of officers associated with the clan.
        /// Officers are users with administrative or managerial roles within the clan.
        /// </summary>
        public readonly UserData[] Officers => API.Clans.Client.GetOfficers(this);

        /// <summary>
        /// Gets the number of members currently in the clan's chat.
        /// </summary>
        public readonly int NumberOfMembersInChat => API.Clans.Client.GetChatMemberCount(this);

        /// <summary>
        /// Provides an array of users currently present in the clan's chat.
        /// </summary>
        public readonly UserData[] MembersInChat => API.Clans.Client.GetChatMembers(this);

        /// <summary>
        /// Indicates whether the current clan is the official game group of the associated application.
        /// </summary>
        public readonly bool IsOfficialGameGroup => API.Clans.Client.IsClanOfficialGameGroup(this);

        /// <summary>
        /// Indicates whether the clan is public.
        /// </summary>
        public readonly bool IsPublic => API.Clans.Client.IsClanPublic(this);

        /// <summary>
        /// Indicates whether the current user is the owner of the Steam clan.
        /// </summary>
        public readonly bool IsUserOwner => Owner == UserData.Me;

        /// <summary>
        /// Indicates whether the current user is one of the officers in the clan.
        /// </summary>
        public readonly bool IsUserOfficer => Officers.Any(p => p == UserData.Me);

        /// <summary>
        /// Retrieves all the clans that the user is currently a member of.
        /// </summary>
        /// <returns>An array of ClanData objects representing the user's clans.</returns>
        public static ClanData[] Get() => API.Clans.Client.GetClans();

        /// <summary>
        /// Retrieves the clan data associated with the specified account ID.
        /// </summary>
        /// <param name="accountId">The account ID representing the clan.</param>
        /// <returns>A ClanData object corresponding to the given account ID.</returns>
        public static ClanData Get(uint accountId) => new CSteamID(new AccountID_t(accountId),
            EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeClan);

        /// <summary>
        /// Retrieves the clan associated with the specified account ID.
        /// </summary>
        /// <param name="accountId">The account ID representing the clan.</param>
        /// <returns>A <see cref="ClanData"/> instance representing the specified clan.</returns>
        public static ClanData Get(AccountID_t accountId) =>
            new CSteamID(accountId, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeClan);

        /// <summary>
        /// Retrieves all clans available to the client.
        /// </summary>
        /// <returns>An array of ClanData objects representing all clans available to the client.</returns>
        public static ClanData Get(ulong id) => new ClanData { id = id };

        /// <summary>
        /// Retrieves all the clans that the user is currently a member of.
        /// </summary>
        /// <returns>An array of ClanData objects representing the user's clans.</returns>
        public static ClanData Get(CSteamID id) => new ClanData { id = id.m_SteamID };

        /// <summary>
        /// Allows the user to join a Steam group (clan) chat room directly from the game.
        /// </summary>
        /// <remarks>
        /// This connects to the legacy Steam group chat system and does not represent the channel-based system introduced in the Steam client in 2018-2019.
        /// It can be an effective way to provide in-game global chat functionality via the Steam API.
        /// </remarks>
        /// <param name="callback">A callback invoked with the resulting ChatRoom object and a boolean indicating success or failure.</param>
        public readonly void JoinChat(Action<ChatRoom, bool> callback) => API.Clans.Client.JoinChatRoom(id, callback);

        /// <summary>
        /// Requests the system to locate and load the icon for the clan.
        /// </summary>
        /// <param name="callback">A callback function that will be invoked with the loaded icon as a Texture2D object.</param>
        public readonly void LoadIcon(Action<Texture2D> callback) =>
            API.Friends.Client.GetFriendAvatar(SteamId, callback);

        #region Boilerplate

        public readonly int CompareTo(CSteamID other)
        {
            return id.CompareTo(other.m_SteamID);
        }

        public readonly int CompareTo(ClanData other)
        {
            return id.CompareTo(other.id);
        }

        public readonly int CompareTo(ulong other)
        {
            return id.CompareTo(other);
        }

        public readonly override string ToString()
        {
            return id.ToString();
        }

        public readonly bool Equals(CSteamID other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(ClanData other)
        {
            return id.Equals(other.id);
        }

        public readonly bool Equals(ulong other)
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

        public static bool operator ==(ClanData l, ClanData r) => l.id == r.id;
        public static bool operator !=(ClanData l, ClanData r) => l.id != r.id;
        public static bool operator ==(ClanData l, CSteamID r) => l.id == r.m_SteamID;
        public static bool operator !=(ClanData l, CSteamID r) => l.id != r.m_SteamID;
        public static bool operator ==(CSteamID l, ClanData r) => l.m_SteamID == r.id;
        public static bool operator !=(CSteamID l, ClanData r) => l.m_SteamID != r.id;
        public static bool operator <(ClanData l, ClanData r) => l.id < r.id;
        public static bool operator >(ClanData l, ClanData r) => l.id > r.id;
        public static bool operator <=(ClanData l, ClanData r) => l.id <= r.id;
        public static bool operator >=(ClanData l, ClanData r) => l.id >= r.id;
        public static implicit operator CSteamID(ClanData c) => c.SteamId;
        public static implicit operator ClanData(CSteamID id) => new ClanData { id = id.m_SteamID };
        public static implicit operator ulong(ClanData c) => c.id;
        public static implicit operator ClanData(ulong id) => new ClanData { id = id };
    #endregion
    }
}
#endif