#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Heathen.SteamworksIntegration.API;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the data related to a Steamworks lobby. Provides properties and methods to manage and interact with a Steam lobby, including its members, metadata, and configuration.
    /// A lobby is a communication and matchmaking structure in Steamworks, providing a way for players to form groups, exchange data, and prepare for multiplayer sessions.
    /// </summary>
    /// <remarks>
    /// This structure includes data such as the lobby's owner, type, members, metadata, and game server information. It also exposes functionality for checking lobby-related properties and managing specific lobby settings.
    /// Additionally, the struct implements equality checks with <c>CSteamID</c>, <c>ulong</c>, and other <c>LobbyData</c> instances for convenience.
    /// </remarks>
    [Serializable]
    public struct LobbyData : IEquatable<CSteamID>, IEquatable<ulong>, IEquatable<LobbyData>
    {
        /// <summary>
        /// Represents the unique identifier for a Steam lobby. This value is backed by an unsigned 64-bit integer
        /// and is used to uniquely distinguish the lobby in Steamworks services.
        /// </summary>
        private ulong _id;

        /// <summary>
        /// Provides access to the unique Steam identifier of the lobby, represented as a <see cref="CSteamID"/> value.
        /// This identifier is used to interact with Steamworks services associated with the lobby.
        /// </summary>
        public readonly CSteamID SteamId => new(_id);

        /// <summary>
        /// Represents the unique account identifier associated with a Steam user involved in the lobby.
        /// This value is derived from the SteamID and is encapsulated within the <see cref="AccountID_t"/> structure.
        /// </summary>
        public readonly AccountID_t AccountId => SteamId.GetAccountID();

        /// <summary>
        /// Represents the unique identifier for a friend account associated with the lobby. This is derived from the
        /// Steam account's numeric AccountID and is represented as an unsigned 32-bit integer.
        /// </summary>
        public readonly uint FriendId => AccountId.m_AccountID;

        /// <summary>
        /// Represents the hexadecimal string representation of the Friend ID associated with this lobby.
        /// The value is derived by converting the underlying Friend ID to its hexadecimal format.
        /// </summary>
        public readonly string HexId => FriendId.ToString("X");

        /// <summary>
        /// Indicates whether the current lobby data represents a valid Steam lobby.
        /// This value is determined by checking if the underlying Steam ID is not
        /// null, if it corresponds to a chat account type, and if it exists within
        /// the public Steam universe. Returns <c>true</c> if all conditions are
        /// satisfied; otherwise, returns <c>false</c>.
        /// </summary>
        public readonly bool IsValid
        {
            get
            {
                var sId = SteamId;
                if (sId == CSteamID.Nil
                    || sId.GetEAccountType() != EAccountType.k_EAccountTypeChat
                    || sId.GetEUniverse() != EUniverse.k_EUniversePublic)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Represents the name of the Steam lobby. This value is part of the lobby's metadata and is used
        /// to denote the displayed name of the lobby in Steamworks services and user interfaces.
        /// It can be retrieved or set based on the current lobby data.
        /// </summary>
        public readonly string Name
        {
            get => this[DataName];
            set => this[DataName] = value;
        }

        /// <summary>
        /// Represents the owner of the lobby. This property provides access to the lobby's owner data
        /// and allows for getting or setting the owner of the Steam lobby.
        /// </summary>
        public readonly LobbyMemberData Owner
        {
            get => new LobbyMemberData { lobby = this, user = Matchmaking.Client.GetLobbyOwner(_id) };
            set => Matchmaking.Client.SetLobbyOwner(_id, value.user);
        }

        /// <summary>
        /// Represents the data for the current user as a member of the associated Steam lobby.
        /// This property provides information about the user's relationship with the specified lobby,
        /// such as their metadata and interaction within the lobby context.
        /// </summary>
        public readonly LobbyMemberData Me => new LobbyMemberData { lobby = this, user = User.Client.Id };

        /// <summary>
        /// Provides a list of members currently in the Steam lobby associated with this instance.
        /// This property retrieves an array of <see cref="LobbyMemberData"/> objects,
        /// each representing a member of the lobby.
        /// </summary>
        public readonly LobbyMemberData[] Members => Matchmaking.Client.GetLobbyMembers(_id);

        /// <summary>
        /// Indicates whether the type of the lobby has been explicitly set.
        /// Returns <c>true</c> if the lobby type data is defined; otherwise, <c>false</c>.
        /// This property is primarily used to check if the lobby's type information
        /// has been configured in Steamworks.
        /// </summary>
        public readonly bool IsTypeSet => !string.IsNullOrEmpty(Matchmaking.Client.GetLobbyData(_id, DataType));

        /// <summary>
        /// Represents the type of the Steam lobby. This property utilises the <see cref="ELobbyType"/>
        /// enumeration provided by Steamworks and determines the visibility and access level of the lobby,
        /// such as public, friends-only, or private.
        /// </summary>
        public readonly ELobbyType Type
        {
            get
            {
                if (int.TryParse(Matchmaking.Client.GetLobbyData(_id, DataType), out var enumVal))
                {
                    return (ELobbyType)enumVal;
                }
                else
                    return ELobbyType.k_ELobbyTypePrivate;
            }
            set => Matchmaking.Client.SetLobbyType(_id, value);
        }

        /// <summary>
        /// Represents the version of the game associated with the Steam lobby. This value is stored
        /// as a string within the lobby metadata and is commonly used to ensure version compatibility
        /// for all participants in the lobby.
        /// </summary>
        public readonly string GameVersion
        {
            get => this[DataVersion];
            set => this[DataVersion] = value;
        }

        /// <summary>
        /// Indicates whether the current user is the owner of the lobby.
        /// This property compares the Steam ID of the current user with the Steam ID
        /// of the lobby owner to determine ownership status.
        /// </summary>
        public readonly bool IsOwner => SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(this);

        /// <summary>
        /// Represents the operational mode of the Steam lobby, defining how it functions
        /// and interacts with players. The mode can be set to General, Session, or Party,
        /// each indicating a specific purpose or behaviour for the lobby. This property
        /// provides mechanisms to retrieve and assign the lobby's mode based on predefined
        /// SteamLobbyModeType enumeration values.
        /// </summary>
        public readonly SteamLobbyModeType Mode
        {
            get
            {
                var modeString = this[DataMode];
                if (modeString == DataModeParty)
                    return SteamLobbyModeType.Party;
                else if (modeString == DataModeSession)
                    return SteamLobbyModeType.Session;
                else
                    return SteamLobbyModeType.General;
            }
            set
            {
                switch (value)
                {
                    case SteamLobbyModeType.Party:
                        this[DataMode] = DataModeParty;
                        break;
                    case SteamLobbyModeType.Session:
                        this[DataMode] = DataModeSession;
                        break;
                    default:
                        this[DataMode] = DataModeGeneral;
                        break;
                }
            }
        }

        /// <summary>
        /// Indicates whether the lobby is configured as a party. A party lobby is characterised
        /// by its specific mode being set to <c>SteamLobbyModeType.Party</c>.
        /// </summary>
        public readonly bool IsParty => Mode == SteamLobbyModeType.Party;

        /// <summary>
        /// Indicates whether the lobby is considered a group. This property is marked as obsolete
        /// and should be replaced with the use of the <see cref="IsParty"/> property.
        /// It internally maps to the <see cref="IsParty"/> property and returns the same value.
        /// </summary>
        [Obsolete("Use IsParty instead")]
        public readonly bool IsGroup => IsParty;

        /// <summary>
        /// Indicates whether the lobby is operating in session mode.
        /// This property returns true if the lobby's mode is set to <c>SteamLobbyModeType.Session</c>,
        /// signifying it is configured for session-based functionality.
        /// </summary>
        public readonly bool IsSession => Mode == SteamLobbyModeType.Session;

        /// <summary>
        /// Indicates whether the lobby is of the general type. This property evaluates to
        /// <c>true</c> if the lobby's mode is set to <see cref="SteamLobbyModeType.General"/>,
        /// and <c>false</c> otherwise.
        /// </summary>
        public readonly bool IsGeneral => Mode == SteamLobbyModeType.General;

        /// <summary>
        /// Indicates whether the lobby is currently associated with a game server.
        /// This property fetches information from Steamworks matchmaking services to
        /// determine if a game server is linked to the specific lobby instance.
        /// </summary>
        public readonly bool HasServer => SteamMatchmaking.GetLobbyGameServer(this, out _, out _, out _);

        /// <summary>
        /// Provides access to the game server details associated with the lobby.
        /// This property retrieves a <see cref="LobbyGameServer"/> structure containing information
        /// about the server hosting the game for the current lobby.
        /// </summary>
        public readonly LobbyGameServer GameServer => Matchmaking.Client.GetLobbyGameServer(_id);

        /// <summary>
        /// Indicates whether all players in the lobby are ready. This property evaluates the readiness
        /// state of all members in the lobby and returns true if every player is marked as ready; otherwise, false.
        /// </summary>
        public readonly bool AllPlayersReady
        {
            get
            {
                //If we have any that are not ready then return false ... else return true
                return Members.All(p => p.IsReady);
            }
        }

        /// <summary>
        /// Indicates whether all players in the lobby are not ready.
        /// This property evaluates to true only when none of the lobby members have their readiness status set to ready.
        /// </summary>
        public readonly bool AllPlayersNotReady
        {
            get
            {
                //If we have any that are not ready then return false ... else return true
                return !Members.Any(p => p.IsReady);
            }
        }

        /// <summary>
        /// Indicates whether the local user is marked as "ready" within the lobby.
        /// This property retrieves or sets a value in the lobby's member data to reflect the readiness status.
        /// </summary>
        public readonly bool IsReady
        {
            get => Matchmaking.Client.GetLobbyMemberData(_id, User.Client.Id, DataReady) == "true";
            set => Matchmaking.Client.SetLobbyMemberData(_id, DataReady, value.ToString().ToLower());
        }

        /// <summary>
        /// Indicates whether the lobby is full, based on the current number of members and the lobby's member limit.
        /// Returns true if the lobby is valid and the number of members is greater than or equal to the member limit; otherwise, false.
        /// </summary>
        public readonly bool Full => IsValid &&
                                     Matchmaking.Client.GetLobbyMemberLimit(_id) <=
                                     SteamMatchmaking.GetNumLobbyMembers(this);

        /// <summary>
        /// Gets or sets the maximum number of members that can join the lobby.
        /// This property interacts with Steamworks services to retrieve or update the member limit
        /// for the lobby identified by its unique ID.
        /// </summary>
        public readonly int MaxMembers
        {
            get => Matchmaking.Client.GetLobbyMemberLimit(_id);
            set => Matchmaking.Client.SetLobbyMemberLimit(_id, value);
        }

        /// <summary>
        /// Represents the total number of members currently in the Steam lobby.
        /// This value is provided by the Steamworks matchmaking service and
        /// dynamically reflects the current number of users in the lobby.
        /// </summary>
        public readonly int MemberCount => SteamMatchmaking.GetNumLobbyMembers(this);
        /// <summary>
        /// Read and write metadata values to the lobby
        /// </summary>
        /// <param name="metadataKey">The key of the value to be read or written</param>
        /// <returns>The value of the key, if any otherwise, returns an empty string.</returns>
        public readonly string this[string metadataKey]
        {
            get => Matchmaking.Client.GetLobbyData(_id, metadataKey);
            set => Matchmaking.Client.SetLobbyData(_id, metadataKey, value);
        }
        /// <summary>
        /// If this user is a member of this lobby get the related <see cref="LobbyMemberData"/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public readonly LobbyMemberData this[UserData user]
        {
            get
            {
                if (GetMember(user, out var member))
                    return member;
                else
                    return default;
            }
        }

        /// <summary>
        /// Retrieves the LobbyMember object for a given user within the lobby.
        /// </summary>
        /// <param name="user">The user for whom the LobbyMember object is to be fetched.</param>
        /// <param name="member">The resulting LobbyMember object if the user is a member of the lobby.</param>
        /// <returns>True if the user is a member of the lobby; otherwise, false.</returns>
        public readonly bool GetMember(UserData user, out LobbyMemberData member) =>
            Matchmaking.Client.GetMember(this, user, out member);

        /// <summary>
        /// Determines whether the specified user is a member of this lobby.
        /// </summary>
        /// <param name="id">The user to check for membership in the lobby.</param>
        /// <returns>True if the user is a member of the lobby; otherwise, false.</returns>
        public readonly bool IsAMember(UserData id) => Matchmaking.Client.IsAMember(this, id);

        /// <summary>
        /// Sets the type of the lobby to the specified lobby type.
        /// </summary>
        /// <param name="type">The desired type of the lobby, specified as an <see cref="ELobbyType"/> value.</param>
        /// <returns>True if the lobby type was successfully updated; otherwise, false.</returns>
        public readonly bool SetType(ELobbyType type) => Matchmaking.Client.SetLobbyType(_id, type);

        /// <summary>
        /// Marks the lobby as joinable or not.
        /// </summary>
        /// <param name="makeJoinable">A boolean value indicating whether the lobby should be joinable. Pass true to make the lobby joinable, or false to prevent others from joining.</param>
        /// <returns>True if the operation to set the joinable state was successful; otherwise, false.</returns>
        public readonly bool SetJoinable(bool makeJoinable) => Matchmaking.Client.SetLobbyJoinable(_id, makeJoinable);

        /// <summary>
        /// Retrieves all metadata associated with a given lobby as a dictionary of key-value pairs.
        /// </summary>
        /// <returns>A dictionary containing the metadata keys and values for the lobby.</returns>
        public readonly Dictionary<string, string> GetMetadata()
        {
            Dictionary<string, string> result = new();

            var count = SteamMatchmaking.GetLobbyDataCount(this);

            for (int i = 0; i < count; i++)
            {
                SteamMatchmaking.GetLobbyDataByIndex(this, i, out string key, Constants.k_nMaxLobbyKeyLength, out string value, Constants.k_cubChatMetadataMax);
                result.Add(key, value);
            }

            return result;
        }

        /// <summary>
        /// Creates a new lobby with the specified parameters and invokes a callback upon completion.
        /// </summary>
        /// <param name="createArguments">The parameters for creating the lobby, including type, usage hint, slot count, and metadata.</param>
        /// <param name="callback">
        /// A callback to be invoked when the lobby creation is completed. The callback provides the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </param>
        public static void Create(CreateArguments createArguments, Action<EResult, LobbyData, bool> callback)
        {
            Create(createArguments.type, createArguments.usageHint, createArguments.slots, (eResult, lobby, ioError) =>
            {
                if (!ioError && eResult == EResult.k_EResultOK)
                {
                    foreach (var metadata in createArguments.metadata)
                    {
                        lobby[metadata.key] = metadata.value;
                    }
                    callback?.Invoke(eResult, lobby, false);
                }
            });
        }

        /// <summary>
        /// Creates a new lobby with the specified parameters asynchronously.
        /// </summary>
        /// <param name="createArguments">The parameters for creating the lobby, including type, usage hint, slot count, and metadata.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static Task<(EResult result, LobbyData lobby, bool ioError)> CreateTask(CreateArguments createArguments)
        {
            var tcs = new TaskCompletionSource<(EResult, LobbyData, bool)>();

            Create(createArguments, (eResult, lobby, ioError) =>
            {
                tcs.SetResult((eResult, lobby, ioError));
            });

            return tcs.Task;
        }
        
#if UNITASK_INSTALLED
        /// <summary>
        /// Creates a new lobby with the specified parameters asynchronously using UniTask.
        /// </summary>
        /// <param name="createArguments">The parameters for creating the lobby, including type, usage hint, slot count, and metadata.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(EResult result, LobbyData lobby, bool ioError)> CreateUniTask(CreateArguments createArguments)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(EResult, LobbyData, bool)>();

            Create(createArguments, (eResult, lobby, ioError) =>
            {
                tcs.TrySetResult((eResult, lobby, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Creates a new lobby with the specified parameters.
        /// </summary>
        /// <param name="type">Specifies the type of lobby to create. The value is typically <see cref="ELobbyType.k_ELobbyTypePublic"/> and should not be <see cref="ELobbyType.k_ELobbyTypePrivateUnique"/>.</param>
        /// <param name="mode">The SteamLobbyModeType defining additional settings or context for the lobby (e.g., matchmaking or specific lobby behaviour).</param>
        /// <param name="slots">Defines the maximum number of members (including the owner) that the lobby can accommodate.</param>
        /// <param name="callback">A delegate that is invoked upon completion, providing the result of the operation, the created lobby data, and a flag indicating whether an I/O error occurred.</param>
        public static void Create(ELobbyType type, SteamLobbyModeType mode, int slots,
            Action<EResult, LobbyData, bool> callback) => Matchmaking.Client.CreateLobby(type, mode, slots, callback);

        /// <summary>
        /// Creates a new lobby with the specified parameters asynchronously.
        /// </summary>
        /// <param name="type">Specifies the type of lobby to create. The value is typically <see cref="ELobbyType.k_ELobbyTypePublic"/> and should not be <see cref="ELobbyType.k_ELobbyTypePrivateUnique"/>.</param>
        /// <param name="mode">The SteamLobbyModeType defining additional settings or context for the lobby (e.g., matchmaking or specific lobby behaviour).</param>
        /// <param name="slots">Defines the maximum number of members (including the owner) that the lobby can accommodate.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static Task<(EResult result, LobbyData lobby, bool ioError)> CreateTask(ELobbyType type, SteamLobbyModeType mode, int slots)
        {
            var tcs = new TaskCompletionSource<(EResult, LobbyData, bool)>();

            Create(type, mode, slots, (eResult, lobby, ioError) =>
            {
                tcs.SetResult((eResult, lobby, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Creates a new lobby with the specified parameters asynchronously using UniTask.
        /// </summary>
        /// <param name="type">Specifies the type of lobby to create. The value is typically <see cref="ELobbyType.k_ELobbyTypePublic"/> and should not be <see cref="ELobbyType.k_ELobbyTypePrivateUnique"/>.</param>
        /// <param name="mode">The SteamLobbyModeType defining additional settings or context for the lobby (e.g., matchmaking or specific lobby behaviour).</param>
        /// <param name="slots">Defines the maximum number of members (including the owner) that the lobby can accommodate.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(EResult result, LobbyData lobby, bool ioError)> CreateUniTask(ELobbyType type, SteamLobbyModeType mode, int slots)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(EResult, LobbyData, bool)>();

            Create(type, mode, slots, (eResult, lobby, ioError) =>
            {
                tcs.TrySetResult((eResult, lobby, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Creates a new lobby designated as a party, setting it to an invisible lobby type.
        /// </summary>
        /// <param name="slots">The maximum number of participants that the party lobby can accommodate.</param>
        /// <param name="callback">A delegate that will be invoked upon completion of the process. The delegate provides the result of the operation, the created LobbyData object, and a flag indicating if an I/O error occurred.</param>
        public static void CreateParty(int slots, Action<EResult, LobbyData, bool> callback)
        {
            Matchmaking.Client.CreateLobby(ELobbyType.k_ELobbyTypeInvisible, SteamLobbyModeType.Party, slots,
                (r, l, e) =>
                {
                    if (!e && r == EResult.k_EResultOK)
                        l.Mode = SteamLobbyModeType.Party;

                    callback?.Invoke(r, l, e);
                });
        }

        /// <summary>
        /// Creates a new lobby designated as a party, setting it to an invisible lobby type asynchronously.
        /// </summary>
        /// <param name="slots">The maximum number of participants that the party lobby can accommodate.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static Task<(EResult result, LobbyData lobby, bool ioError)> CreatePartyTask(int slots)
        {
            var tcs = new TaskCompletionSource<(EResult, LobbyData, bool)>();

            CreateParty(slots, (eResult, lobby, ioError) =>
            {
                tcs.SetResult((eResult, lobby, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Creates a new lobby designated as a party, setting it to an invisible lobby type asynchronously using UniTask.
        /// </summary>
        /// <param name="slots">The maximum number of participants that the party lobby can accommodate.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(EResult result, LobbyData lobby, bool ioError)> CreatePartyUniTask(int slots)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(EResult, LobbyData, bool)>();

            CreateParty(slots, (eResult, lobby, ioError) =>
            {
                tcs.TrySetResult((eResult, lobby, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Creates a new Steam lobby and designates it as a session lobby, typically used for matchmaking game sessions.
        /// </summary>
        /// <param name="type">The type of lobby to be created. Typically, this would be <c>ELobbyType.k_ELobbyTypePublic</c>. It should not be <c>ELobbyType.k_ELobbyTypePrivateUnique</c>.</param>
        /// <param name="slots">The maximum number of members that can join the lobby, including its owner.</param>
        /// <param name="callback">A delegate invoked when the lobby creation process completes. The delegate provides an <c>EResult</c> indicating the result of the operation, a <c>LobbyData</c> object representing the created lobby, and a <c>bool</c> indicating if an I/O error occurred.</param>
        public static void CreateSession(ELobbyType type, int slots, Action<EResult, LobbyData, bool> callback)
        {
            Matchmaking.Client.CreateLobby(type, SteamLobbyModeType.Session, slots, (r, l, e) =>
            {
                if (!e && r == EResult.k_EResultOK)
                    l.Mode = SteamLobbyModeType.Session;

                callback?.Invoke(r, l, e);
            });
        }

        /// <summary>
        /// Creates a new Steam lobby designated as a session lobby asynchronously.
        /// </summary>
        /// <param name="type">The type of lobby to be created. Typically, this would be <c>ELobbyType.k_ELobbyTypePublic</c>. It should not be <c>ELobbyType.k_ELobbyTypePrivateUnique</c>.</param>
        /// <param name="slots">The maximum number of members that can join the lobby, including its owner.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static Task<(EResult result, LobbyData lobby, bool ioError)> CreateSessionTask(ELobbyType type, int slots)
        {
            var tcs = new TaskCompletionSource<(EResult, LobbyData, bool)>();

            CreateSession(type, slots, (eResult, lobby, ioError) =>
            {
                tcs.SetResult((eResult, lobby, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Creates a new Steam lobby designated as a session lobby asynchronously using UniTask.
        /// </summary>
        /// <param name="type">The type of lobby to be created. Typically, this would be <c>ELobbyType.k_ELobbyTypePublic</c>. It should not be <c>ELobbyType.k_ELobbyTypePrivateUnique</c>.</param>
        /// <param name="slots">The maximum number of members that can join the lobby, including its owner.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(EResult result, LobbyData lobby, bool ioError)> CreateSessionUniTask(ELobbyType type, int slots)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(EResult, LobbyData, bool)>();

            CreateSession(type, slots, (eResult, lobby, ioError) =>
            {
                tcs.TrySetResult((eResult, lobby, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Creates a new public lobby intended for use as a session.
        /// A session lobby is designed for matchmaking game sessions,
        /// as opposed to lobbies used for party-based functionality.
        /// </summary>
        /// <param name="slots">The maximum number of members, including the owner, allowed in the lobby.</param>
        /// <param name="callback">A callback delegate that is invoked upon completion.
        /// It provides the result of the operation as an <see cref="EResult"/> value,
        /// the created <see cref="LobbyData"/> instance, and a boolean indicating if an I/O error occurred.</param>
        public static void CreatePublicSession(int slots, Action<EResult, LobbyData, bool> callback)
        {
            Matchmaking.Client.CreateLobby(ELobbyType.k_ELobbyTypePublic, SteamLobbyModeType.Session, slots,
                (r, l, e) =>
                {
                    if (!e && r == EResult.k_EResultOK)
                        l.Mode = SteamLobbyModeType.Session;

                    callback?.Invoke(r, l, e);
                });
        }

        /// <summary>
        /// Creates a new public lobby intended for use as a session asynchronously.
        /// A session lobby is designed for matchmaking game sessions,
        /// as opposed to lobbies used for party-based functionality.
        /// </summary>
        /// <param name="slots">The maximum number of members, including the owner, allowed in the lobby.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static Task<(EResult result, LobbyData lobby, bool ioError)> CreatePublicSessionTask(int slots)
        {
            var tcs = new TaskCompletionSource<(EResult, LobbyData, bool)>();

            CreatePublicSession(slots, (eResult, lobby, ioError) =>
            {
                tcs.SetResult((eResult, lobby, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Creates a new public lobby intended for use as a session asynchronously using UniTask.
        /// A session lobby is designed for matchmaking game sessions,
        /// as opposed to lobbies used for party-based functionality.
        /// </summary>
        /// <param name="slots">The maximum number of members, including the owner, allowed in the lobby.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(EResult result, LobbyData lobby, bool ioError)> CreatePublicSessionUniTask(int slots)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(EResult, LobbyData, bool)>();

            CreatePublicSession(slots, (eResult, lobby, ioError) =>
            {
                tcs.TrySetResult((eResult, lobby, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Creates a new private lobby designated as a session for matchmaking purposes.
        /// A session lobby is intended for organising a game session and differs from lobbies used for party groups.
        /// </summary>
        /// <param name="slots">The maximum number of members, including the owner, that the lobby can accommodate.</param>
        /// <param name="callback">A delegate invoked upon completion of the operation, providing the result, the created lobby data, and an indicator of any I/O error.</param>
        public static void CreatePrivateSession(int slots, Action<EResult, LobbyData, bool> callback)
        {
            Matchmaking.Client.CreateLobby(ELobbyType.k_ELobbyTypePrivate, SteamLobbyModeType.Session, slots,
                (r, l, e) =>
                {
                    if (!e && r == EResult.k_EResultOK)
                        l.Mode = SteamLobbyModeType.Session;

                    callback?.Invoke(r, l, e);
                });
        }

        /// <summary>
        /// Creates a new private lobby designated as a session for matchmaking purposes asynchronously.
        /// A session lobby is intended for organising a game session and differs from lobbies used for party groups.
        /// </summary>
        /// <param name="slots">The maximum number of members, including the owner, that the lobby can accommodate.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static Task<(EResult result, LobbyData lobby, bool ioError)> CreatePrivateSessionTask(int slots)
        {
            var tcs = new TaskCompletionSource<(EResult, LobbyData, bool)>();

            CreatePrivateSession(slots, (eResult, lobby, ioError) =>
            {
                tcs.SetResult((eResult, lobby, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Creates a new private lobby designated as a session for matchmaking purposes asynchronously using UniTask.
        /// A session lobby is intended for organising a game session and differs from lobbies used for party groups.
        /// </summary>
        /// <param name="slots">The maximum number of members, including the owner, that the lobby can accommodate.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(EResult result, LobbyData lobby, bool ioError)> CreatePrivateSessionUniTask(int slots)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(EResult, LobbyData, bool)>();

            CreatePrivateSession(slots, (eResult, lobby, ioError) =>
            {
                tcs.TrySetResult((eResult, lobby, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Creates a friend-only lobby and designates it as a session for matchmaking purposes.
        /// </summary>
        /// <param name="slots">The maximum number of members, including the owner, that the lobby can accommodate.</param>
        /// <param name="callback">A delegate invoked when the lobby creation process is complete. It provides the result of the operation, the created lobby data, and an I/O error flag.</param>
        public static void CreateFriendOnlySession(int slots, Action<EResult, LobbyData, bool> callback)
        {
            Matchmaking.Client.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, SteamLobbyModeType.Session, slots,
                (r, l, e) =>
                {
                    if (!e && r == EResult.k_EResultOK)
                        l.Mode = SteamLobbyModeType.Session;

                    callback?.Invoke(r, l, e);
                });
        }

        /// <summary>
        /// Creates a friend-only lobby designated as a session for matchmaking purposes asynchronously.
        /// </summary>
        /// <param name="slots">The maximum number of members, including the owner, that the lobby can accommodate.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static Task<(EResult result, LobbyData lobby, bool ioError)> CreateFriendOnlySessionTask(int slots)
        {
            var tcs = new TaskCompletionSource<(EResult, LobbyData, bool)>();

            CreateFriendOnlySession(slots, (eResult, lobby, ioError) =>
            {
                tcs.SetResult((eResult, lobby, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Creates a friend-only lobby designated as a session for matchmaking purposes asynchronously using UniTask.
        /// </summary>
        /// <param name="slots">The maximum number of members, including the owner, that the lobby can accommodate.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the created lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(EResult result, LobbyData lobby, bool ioError)> CreateFriendOnlySessionUniTask(int slots)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(EResult, LobbyData, bool)>();

            CreateFriendOnlySession(slots, (eResult, lobby, ioError) =>
            {
                tcs.TrySetResult((eResult, lobby, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Joins the specified lobby.
        /// </summary>
        /// <param name="callback">A delegate that receives the result of the lobby join operation, including the <see cref="LobbyEnter"/> result and a <see cref="bool"/> indicating if an I/O error occurred.</param>
        public readonly void Join(Action<LobbyEnter, bool> callback)
        {
            Matchmaking.Client.JoinLobby(this, callback);
        }

        /// <summary>
        /// Joins the specified lobby asynchronously.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the lobby enter result
        /// and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public readonly Task<(LobbyEnter lobbyEnter, bool ioError)> JoinTask()
        {
            var tcs = new TaskCompletionSource<(LobbyEnter, bool)>();

            Join((lobbyEnter, ioError) =>
            {
                tcs.SetResult((lobbyEnter, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Joins the specified lobby asynchronously using UniTask.
        /// </summary>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the lobby enter result
        /// and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public readonly async Cysharp.Threading.Tasks.UniTask<(LobbyEnter lobbyEnter, bool ioError)> JoinUniTask()
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(LobbyEnter, bool)>();

            Join((lobbyEnter, ioError) =>
            {
                tcs.TrySetResult((lobbyEnter, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Leaves the current lobby associated with this instance, if applicable.
        /// </summary>
        public readonly void Leave()
        {
            if (SteamId == CSteamID.Nil)
                return;

            Matchmaking.Client.LeaveLobby(this);
        }

        /// <summary>
        /// Removes the specified metadata field from the lobby data.
        /// </summary>
        /// <param name="dataKey">The key of the metadata field to be removed.</param>
        /// <returns>True if the request to delete the metadata field was accepted; otherwise, false.</returns>
        public readonly bool DeleteLobbyData(string dataKey) => Matchmaking.Client.DeleteLobbyData(_id, dataKey);

        /// <summary>
        /// Sends an invitation to the specified user to join the lobby.
        /// </summary>
        /// <param name="targetUser">The user to be invited to the lobby.</param>
        /// <returns>True if the invitation was successfully sent; otherwise, false.</returns>
        public readonly bool InviteUserToLobby(UserData targetUser) =>
            Matchmaking.Client.InviteUserToLobby(_id, targetUser);

        /// <summary>
        /// Sends a chat message to all members of the lobby.
        /// </summary>
        /// <param name="message">The message to be sent to the lobby chat.</param>
        /// <returns>True if the message was sent successfully; otherwise, false.</returns>
        public readonly bool SendChatMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            byte[] msgBody = System.Text.Encoding.UTF8.GetBytes(message);
            return SteamMatchmaking.SendLobbyChatMsg(this, msgBody, msgBody.Length);
        }

        /// <summary>
        /// Sends a chat message as binary data to all members of the lobby.
        /// </summary>
        /// <param name="data">The binary data to be sent as a chat message.</param>
        /// <returns>True if the message was successfully sent; otherwise, false.</returns>
        public readonly bool SendChatMessage(byte[] data)
        {
            if (data == null || data.Length < 1)
                return false;

            return SteamMatchmaking.SendLobbyChatMsg(this, data, data.Length);
        }

        /// <summary>
        /// Sends a chat message to the lobby as a serialized object.
        /// </summary>
        /// <param name="jsonObject">The object to be serialized and sent as a chat message. It must be serializable, as the system will use JsonUtility to serialize and encode it as a UTF8 byte array.</param>
        /// <returns>True if the chat message was successfully sent; otherwise, false.</returns>
        public readonly bool SendChatMessage(object jsonObject)
        {
            return SendChatMessage(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(jsonObject)));
        }

        /// <summary>
        /// Sets the game server associated with the lobby. This can only be set by the owner of the lobby.
        /// </summary>
        /// <param name="address">The IP address of the game server to set for the lobby.</param>
        /// <param name="port">The port number of the game server to set for the lobby.</param>
        /// <param name="gameServerId">The Steamworks ID of the game server to set for the lobby.</param>
        public readonly void SetGameServer(string address, ushort port, CSteamID gameServerId)
        {
            Matchmaking.Client.SetLobbyGameServer(_id, address, port, gameServerId);
        }

        /// <summary>
        /// Associates a game server with the lobby. This action is restricted to the lobby owner.
        /// Members in the lobby will receive a LobbyGameCreated_t callback once the server is set, enabling them to connect to the specified game server.
        /// </summary>
        /// <param name="address">The IP address of the game server to be set for the lobby.</param>
        /// <param name="port">The port number associated with the game server.</param>
        public readonly void SetGameServer(string address, ushort port)
        {
            Matchmaking.Client.SetLobbyGameServer(_id, address, port, CSteamID.Nil);
        }

        /// <summary>
        /// Sets the game server associated with the lobby. This operation can only be performed by the lobby owner.
        /// </summary>
        /// <param name="gameServerId">The Steamworks ID of the game server to associate with the lobby.</param>
        public readonly void SetGameServer(CSteamID gameServerId)
        {
            Matchmaking.Client.SetLobbyGameServer(_id, 0, 0, gameServerId);
        }

        /// <summary>
        /// Sets the game server associated with the lobby using the lobby owner's Steam ID as the server ID.
        /// This is typically used for peer-to-peer (P2P) sessions.
        /// </summary>
        /// <remarks>
        /// This method can only be called by the owner of the lobby. When invoked,
        /// it associates the lobby with a game server and notifies all members in the lobby
        /// via a LobbyGameCreated_t callback. Clients will usually connect to the specified game server at this point.
        /// </remarks>
        public readonly void SetGameServer()
        {
            Matchmaking.Client.SetLobbyGameServer(_id, 0, 0, User.Client.Id);
        }

        /// <summary>
        /// Marks a specified member for removal from the lobby by adding their ID to the lobby's kick list.
        /// </summary>
        /// <param name="memberId">The ID of the user to be removed from the lobby.</param>
        /// <returns>True if the member was successfully marked for removal; otherwise, false.</returns>
        public readonly bool KickMember(UserData memberId)
        {
            if (!IsOwner)
                return false;

            var kickList = Matchmaking.Client.GetLobbyData(_id, DataKick) ?? string.Empty;

            if (!kickList.Contains("[" + memberId.ToString() + "]"))
                kickList += "[" + memberId.ToString() + "]";

            return Matchmaking.Client.SetLobbyData(_id, DataKick, kickList);
        }

        /// <summary>
        /// Determines whether the specified user is present in the list of members marked to leave the lobby.
        /// </summary>
        /// <param name="memberId">The user data to be checked against the list of members to be removed from the lobby.</param>
        /// <returns>True if the specified user is in the kick list; otherwise, false.</returns>
        public readonly bool KickListContains(UserData memberId)
        {
            var kickList = Matchmaking.Client.GetLobbyData(_id, DataKick);
            return kickList.Contains("[" + memberId.ToString() + "]");
        }

        /// <summary>
        /// Removes the specified member from the lobby's kick list.
        /// </summary>
        /// <param name="memberId">The UserData object representing the member to be removed from the kick list.</param>
        /// <returns>True if the member was successfully removed from the kick list; otherwise, false.</returns>
        public readonly bool RemoveFromKickList(UserData memberId)
        {
            if (!IsOwner)
                return false;

            var kickList = Matchmaking.Client.GetLobbyData(_id, DataKick);

            kickList = kickList.Replace("[" + memberId.ToString() + "]", string.Empty);

            return Matchmaking.Client.SetLobbyData(_id, DataKick, kickList);
        }

        /// <summary>
        /// Clears the kick list of a lobby. This operation can only be performed by the owner of the lobby.
        /// </summary>
        /// <returns>True if the kick list was successfully cleared; otherwise, false.</returns>
        public readonly bool ClearKickList()
        {
            if (!IsOwner)
                return false;

            return Matchmaking.Client.DeleteLobbyData(_id, DataKick);
        }

        /// <summary>
        /// Retrieves a list of users marked for removal from the lobby, based on the lobby's kick list data.
        /// </summary>
        /// <returns>An array of UserData objects representing the users in the kick list. If no users are marked for removal, returns an empty array.</returns>
        public readonly UserData[] GetKickList()
        {
            var list = Matchmaking.Client.GetLobbyData(_id, DataKick);
            if (!string.IsNullOrEmpty(list))
            {
                var sArray = list.Split(new[] { "][" }, StringSplitOptions.RemoveEmptyEntries);
                var resultList = new List<UserData>();
                for (int i = 0; i < sArray.Length; i++)
                {
                    var user = UserData.Get(sArray[i].Replace("[", string.Empty).Replace("]", string.Empty));
                    if (user.IsValid)
                        resultList.Add(user);
                }

                return resultList.ToArray();
            }
            else
                return Array.Empty<UserData>();
        }

        /// <summary>
        /// Sets metadata for a specific player within the lobby.
        /// </summary>
        /// <param name="key">The key representing the metadata field to set.</param>
        /// <param name="value">The value to assign to the specified metadata field.</param>
        public readonly void SetMemberMetadata(string key, string value)
        {
            Matchmaking.Client.SetLobbyMemberData(_id, key, value);
        }

        /// <summary>
        /// Sets a metadata key-value pair on the specified lobby. This operation can only be performed by the lobby owner.
        /// </summary>
        /// <param name="key">The unique identifier for the metadata to be set on the lobby.</param>
        /// <param name="value">The value associated with the metadata key to store in the lobby.</param>
        public readonly void SetLobbyMetadata(string key, string value)
        {
            Matchmaking.Client.SetLobbyData(_id, key, value);
        }

        /// <summary>
        /// Retrieves the metadata field value for the local user in the lobby.
        /// </summary>
        /// <param name="key">The metadata key whose value is to be retrieved.</param>
        /// <returns>The metadata value associated with the given key for the local user in the lobby.</returns>
        public readonly string GetMemberMetadata(string key)
        {
            return Matchmaking.Client.GetLobbyMemberData(_id, User.Client.Id, key);
        }

        /// <summary>
        /// Retrieves the metadata value associated with the specified key for a given lobby member.
        /// </summary>
        /// <param name="memberId">The <see cref="UserData"/> object representing the lobby member whose metadata is to be retrieved.</param>
        /// <param name="key">The key identifying the metadata field to retrieve.</param>
        /// <returns>The metadata value associated with the specified key for the given lobby member.</returns>
        public readonly string GetMemberMetadata(UserData memberId, string key)
        {
            return Matchmaking.Client.GetLobbyMemberData(_id, memberId, key);
        }

        /// <summary>
        /// Retrieves the metadata value associated with a specified key for a given lobby member.
        /// </summary>
        /// <param name="member">The lobby member for whom the metadata value is being retrieved.</param>
        /// <param name="key">The key identifying the specific metadata field.</param>
        /// <returns>The value of the metadata associated with the specified key for the provided lobby member.</returns>
        public readonly string GetMemberMetadata(LobbyMemberData member, string key)
        {
            return Matchmaking.Client.GetLobbyMemberData(_id, member.user, key);
        }

        /// <summary>
        /// Requests a refresh of all metadata for a lobby that the user is not currently a member of.
        /// </summary>
        /// <remarks>
        /// Use this to refresh metadata for lobbies that were obtained via methods like RequestLobbyList or those accessible through friends.
        /// Metadata for lobbies the user is a member of is always up to date without requiring this request.
        /// </remarks>
        /// <returns>True if the request to refresh lobby metadata was successfully initiated; otherwise, false.</returns>
        public readonly bool RequestData() => Matchmaking.Client.RequestLobbyData(_id);

        /// <summary>
        /// Represents a collection of lobbies that the local user is currently a member of.
        /// Each lobby in the collection is represented by a <see cref="LobbyData"/> object,
        /// providing detailed information about the respective lobby.
        /// </summary>
        public static List<LobbyData> MemberOfLobbies => Matchmaking.Client.MemberOfLobbies;

        /// <summary>
        /// Retrieves a lobby based on a provided account ID represented as a hexadecimal string.
        /// </summary>
        /// <param name="accountId">The hexadecimal string representing the account ID of the lobby.</param>
        /// <returns>The corresponding LobbyData object if the account ID is valid; otherwise, a default invalid LobbyData instance.</returns>
        public static LobbyData Get(string accountId)
        {
            try
            {
                var id = Convert.ToUInt32(accountId, 16);
                if (id > 0)
                    return Get(id);
                else
                    return CSteamID.Nil;
            }
            catch(Exception ex)
            {
                Debug.LogWarning("Failed to parse account Id: " + ex.Message);
                return CSteamID.Nil;
            }
        }

        /// <summary>
        /// Retrieves the lobby data associated with the specified account ID.
        /// </summary>
        /// <param name="accountId">The account ID of the desired lobby.</param>
        /// <returns>An instance of LobbyData representing the lobby identified by the given account ID.</returns>
        public static LobbyData Get(uint accountId) => new CSteamID(new AccountID_t(accountId), 393216,
            EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);

        /// <summary>
        /// Retrieves the lobby associated with the specified account ID.
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to retrieve.</param>
        /// <returns>A LobbyData object representing the specified lobby.</returns>
        public static LobbyData Get(AccountID_t accountId) => new CSteamID(accountId, 393216,
            EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);

        /// <summary>
        /// Retrieves a LobbyData instance corresponding to the specified lobby identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the lobby.</param>
        /// <returns>A LobbyData structure representing the specified lobby.</returns>
        public static LobbyData Get(ulong id) => new LobbyData { _id = id };

        /// <summary>
        /// Retrieves a <see cref="LobbyData"/> object for a given CSteamID.
        /// </summary>
        /// <param name="id">The CSteamID representing the Steam lobby.</param>
        /// <returns>A <see cref="LobbyData"/> object that represents the specified lobby.</returns>
        public static LobbyData Get(CSteamID id) => new LobbyData { _id = id.m_SteamID };

        /// <summary>
        /// Retrieves the first lobby within the matchmaking client's list of lobbies where the lobby's party status is set to true.
        /// </summary>
        /// <param name="lobby">The resulting LobbyData object representing the first party lobby, if found.</param>
        /// <returns>True if a valid party lobby is retrieved; otherwise, false.</returns>
        public static bool PartyLobby(out LobbyData lobby)
        {
            lobby = Matchmaking.Client.MemberOfLobbies.FirstOrDefault(p => p.IsParty);
            return lobby.IsValid;
        }

        /// <summary>
        /// Retrieves the "Group" lobby if one exists, for the current user.
        /// </summary>
        /// <param name="lobby">The resulting LobbyData object representing the group lobby if one is found.</param>
        /// <returns>True if a group lobby is available; otherwise, false.</returns>
        [Obsolete("Use " + nameof(PartyLobby) + " instead")]
        public static bool GroupLobby(out LobbyData lobby) => PartyLobby(out lobby);

        /// <summary>
        /// Finds and retrieves the first lobby where the "IsSession" property is set to true.
        /// </summary>
        /// <param name="lobby">The output parameter that will hold the resulting session lobby if found.</param>
        /// <returns>True if a session lobby is found; otherwise, false.</returns>
        public static bool SessionLobby(out LobbyData lobby)
        {
            lobby = Matchmaking.Client.MemberOfLobbies.FirstOrDefault(p => p.IsSession);
            return lobby.IsValid;
        }

        /// <summary>
        /// Joins the lobby identified by the specified account ID.
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to join. This should be a valid string representation of an uint, which can also include hexadecimal values supported by System.Convert.ToUInt.</param>
        /// <param name="callback">The callback that will be invoked upon completion of the join process. It provides a LobbyEnter result and a boolean indicating if an IO error occurred.</param>
        public static void Join(string accountId, Action<LobbyEnter, bool> callback) =>
            Matchmaking.Client.JoinLobby(Get(accountId), callback);

        /// <summary>
        /// Joins the lobby identified by the specified account ID asynchronously.
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to join. This should be a valid string representation of an uint, which can also include hexadecimal values supported by System.Convert.ToUInt.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the lobby enter result
        /// and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static Task<(LobbyEnter lobbyEnter, bool ioError)> JoinTask(string accountId)
        {
            var tcs = new TaskCompletionSource<(LobbyEnter, bool)>();

            Join(accountId, (lobbyEnter, ioError) =>
            {
                tcs.SetResult((lobbyEnter, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Joins the lobby identified by the specified account ID asynchronously using UniTask.
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to join. This should be a valid string representation of an uint, which can also include hexadecimal values supported by System.Convert.ToUInt.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the lobby enter result
        /// and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(LobbyEnter lobbyEnter, bool ioError)> JoinUniTask(string accountId)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(LobbyEnter, bool)>();

            Join(accountId, (lobbyEnter, ioError) =>
            {
                tcs.TrySetResult((lobbyEnter, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Attempts to join the specified lobby.
        /// </summary>
        /// <param name="lobby">The lobby data representing the lobby to join.</param>
        /// <param name="callback">A callback that is invoked upon completion of the join process.
        /// The handler function receives a <see cref="LobbyEnter"/> result and a boolean indicating whether an IO error occurred.</param>
        public static void Join(LobbyData lobby, Action<LobbyEnter, bool> callback) =>
            Matchmaking.Client.JoinLobby(lobby, callback);

        /// <summary>
        /// Attempts to join the specified lobby asynchronously.
        /// </summary>
        /// <param name="lobby">The lobby data representing the lobby to join.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the lobby enter result
        /// and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static Task<(LobbyEnter lobbyEnter, bool ioError)> JoinTask(LobbyData lobby)
        {
            var tcs = new TaskCompletionSource<(LobbyEnter, bool)>();

            Join(lobby, (lobbyEnter, ioError) =>
            {
                tcs.SetResult((lobbyEnter, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Attempts to join the specified lobby asynchronously using UniTask.
        /// </summary>
        /// <param name="lobby">The lobby data representing the lobby to join.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the lobby enter result
        /// and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(LobbyEnter lobbyEnter, bool ioError)> JoinUniTask(LobbyData lobby)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(LobbyEnter, bool)>();

            Join(lobby, (lobbyEnter, ioError) =>
            {
                tcs.TrySetResult((lobbyEnter, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Attempts to join a lobby associated with the specified account ID.
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to join. Must be formatted as a valid unsigned integer (uint) in string form.</param>
        /// <param name="callback">A callback method to execute upon completion of the join operation.
        /// The callback takes two parameters: a LobbyEnter structure which contains information about the lobby entrance,
        /// and a boolean indicating whether an I/O error occurred.</param>
        public static void Join(AccountID_t accountId, Action<LobbyEnter, bool> callback) =>
            Matchmaking.Client.JoinLobby(Get(accountId), callback);

        /// <summary>
        /// Attempts to join a lobby associated with the specified account ID asynchronously.
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to join. Must be formatted as a valid unsigned integer (uint) in string form.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the lobby enter result
        /// and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static Task<(LobbyEnter lobbyEnter, bool ioError)> JoinTask(AccountID_t accountId)
        {
            var tcs = new TaskCompletionSource<(LobbyEnter, bool)>();

            Join(accountId, (lobbyEnter, ioError) =>
            {
                tcs.SetResult((lobbyEnter, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Attempts to join a lobby associated with the specified account ID asynchronously using UniTask.
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to join. Must be formatted as a valid unsigned integer (uint) in string form.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the lobby enter result
        /// and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(LobbyEnter lobbyEnter, bool ioError)> JoinUniTask(AccountID_t accountId)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(LobbyEnter, bool)>();

            Join(accountId, (lobbyEnter, ioError) =>
            {
                tcs.TrySetResult((lobbyEnter, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Requests a list of lobbies based on the specified filters and criteria.
        /// </summary>
        /// <param name="distanceFilter">The distance filter to apply when searching for lobbies.</param>
        /// <param name="openSlotsRequired">The minimum number of open slots required in the lobby.</param>
        /// <param name="maxResultsToReturn">The maximum number of lobby search results to return.</param>
        /// <param name="stringFilters">A collection of string-based filters to apply when searching for lobbies.</param>
        /// <param name="nearFilters">A collection of proximity-based filters to apply when searching for lobbies.</param>
        /// <param name="numericFilters">A collection of numeric filters to apply when searching for lobbies.</param>
        /// <param name="callback">The callback function to invoke with the search results and a success flag.</param>
        public static void Request(ELobbyDistanceFilter distanceFilter, int openSlotsRequired, int maxResultsToReturn,
            IEnumerable<StringFilter> stringFilters, IEnumerable<NearFilter> nearFilters,
            IEnumerable<NumericFilter> numericFilters, Action<LobbyData[], bool> callback)
        {
            Matchmaking.Client.AddRequestLobbyListDistanceFilter(distanceFilter);
            Matchmaking.Client.AddRequestLobbyListFilterSlotsAvailable(openSlotsRequired);
            var enumerable = stringFilters as StringFilter[] ?? stringFilters.ToArray();
            if (enumerable.Any())
            {
                foreach (var filter in enumerable)
                    Matchmaking.Client.AddRequestLobbyListStringFilter(filter.key, filter.value, filter.comparison);
            }

            var filters = nearFilters as NearFilter[] ?? nearFilters.ToArray();
            if (filters.Any())
            {
                foreach (var filter in filters)
                    Matchmaking.Client.AddRequestLobbyListNearValueFilter(filter.key, filter.value);
            }

            var numericFilters1 = numericFilters as NumericFilter[] ?? numericFilters.ToArray();
            if (numericFilters1.Any())
            {
                foreach (var filter in numericFilters1)
                    Matchmaking.Client.AddRequestLobbyListNumericalFilter(filter.key, filter.value, filter.comparison);
            }

            Matchmaking.Client.AddRequestLobbyListResultCountFilter(maxResultsToReturn);
            Matchmaking.Client.RequestLobbyList(callback);
        }

        /// <summary>
        /// Requests a list of lobbies based on the specified filters and criteria asynchronously.
        /// </summary>
        /// <param name="distanceFilter">The distance filter to apply when searching for lobbies.</param>
        /// <param name="openSlotsRequired">The minimum number of open slots required in the lobby.</param>
        /// <param name="maxResultsToReturn">The maximum number of lobby search results to return.</param>
        /// <param name="stringFilters">A collection of string-based filters to apply when searching for lobbies.</param>
        /// <param name="nearFilters">A collection of proximity-based filters to apply when searching for lobbies.</param>
        /// <param name="numericFilters">A collection of numeric filters to apply when searching for lobbies.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with an array of matching lobbies
        /// and a boolean indicating whether an error occurred.
        /// </returns>
        public static Task<(LobbyData[] lobbies, bool error)> RequestTask(ELobbyDistanceFilter distanceFilter, int openSlotsRequired, int maxResultsToReturn,
            IEnumerable<StringFilter> stringFilters, IEnumerable<NearFilter> nearFilters,
            IEnumerable<NumericFilter> numericFilters)
        {
            var tcs = new TaskCompletionSource<(LobbyData[], bool)>();

            Request(distanceFilter, openSlotsRequired, maxResultsToReturn, stringFilters, nearFilters, numericFilters, (lobbies, error) =>
            {
                tcs.SetResult((lobbies, error));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Requests a list of lobbies based on the specified filters and criteria asynchronously using UniTask.
        /// </summary>
        /// <param name="distanceFilter">The distance filter to apply when searching for lobbies.</param>
        /// <param name="openSlotsRequired">The minimum number of open slots required in the lobby.</param>
        /// <param name="maxResultsToReturn">The maximum number of lobby search results to return.</param>
        /// <param name="stringFilters">A collection of string-based filters to apply when searching for lobbies.</param>
        /// <param name="nearFilters">A collection of proximity-based filters to apply when searching for lobbies.</param>
        /// <param name="numericFilters">A collection of numeric filters to apply when searching for lobbies.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with an array of matching lobbies
        /// and a boolean indicating whether an error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(LobbyData[] lobbies, bool error)> RequestUniTask(ELobbyDistanceFilter distanceFilter, int openSlotsRequired, int maxResultsToReturn,
            IEnumerable<StringFilter> stringFilters, IEnumerable<NearFilter> nearFilters,
            IEnumerable<NumericFilter> numericFilters)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(LobbyData[], bool)>();

            Request(distanceFilter, openSlotsRequired, maxResultsToReturn, stringFilters, nearFilters, numericFilters, (lobbies, error) =>
            {
                tcs.TrySetResult((lobbies, error));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Sends a request to search for lobbies based on the specified search criteria.
        /// </summary>
        /// <param name="searchArguments">An object containing the search criteria, including filters for distance, slots, and various key-value pairs.</param>
        /// <param name="maxResultsToReturn">The maximum number of lobbies to return in the search results.</param>
        /// <param name="callback">A delegate to handle the results of the search operation. The callback provides an array of matching lobbies and a boolean indicating whether there was an error.</param>
        public static void Request(SearchArguments searchArguments, int maxResultsToReturn, Action<LobbyData[], bool> callback)
        {
            Matchmaking.Client.AddRequestLobbyListDistanceFilter(searchArguments.distance);
            Matchmaking.Client.AddRequestLobbyListFilterSlotsAvailable(searchArguments.slots);
            if (searchArguments.stringFilters != null && searchArguments.stringFilters.Any())
            {
                foreach (var filter in searchArguments.stringFilters)
                    Matchmaking.Client.AddRequestLobbyListStringFilter(filter.key, filter.value, filter.comparison);
            }
            if (searchArguments.nearValues != null && searchArguments.nearValues.Any())
            {
                foreach (var filter in searchArguments.nearValues)
                    Matchmaking.Client.AddRequestLobbyListNearValueFilter(filter.key, filter.value);
            }
            if (searchArguments.numericFilters != null && searchArguments.numericFilters.Any())
            {
                foreach (var filter in searchArguments.numericFilters)
                    Matchmaking.Client.AddRequestLobbyListNumericalFilter(filter.key, filter.value, filter.comparison);
            }
            Matchmaking.Client.AddRequestLobbyListResultCountFilter(maxResultsToReturn);
            Matchmaking.Client.RequestLobbyList(callback);
        }

        /// <summary>
        /// Sends a request to search for lobbies based on the specified search criteria asynchronously.
        /// </summary>
        /// <param name="searchArguments">An object containing the search criteria, including filters for distance, slots, and various key-value pairs.</param>
        /// <param name="maxResultsToReturn">The maximum number of lobbies to return in the search results.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with an array of matching lobbies
        /// and a boolean indicating whether an error occurred.
        /// </returns>
        public static Task<(LobbyData[] lobbies, bool error)> RequestTask(SearchArguments searchArguments, int maxResultsToReturn)
        {
            var tcs = new TaskCompletionSource<(LobbyData[], bool)>();

            Request(searchArguments, maxResultsToReturn, (lobbies, error) =>
            {
                tcs.SetResult((lobbies, error));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Sends a request to search for lobbies based on the specified search criteria asynchronously using UniTask.
        /// </summary>
        /// <param name="searchArguments">An object containing the search criteria, including filters for distance, slots, and various key-value pairs.</param>
        /// <param name="maxResultsToReturn">The maximum number of lobbies to return in the search results.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with an array of matching lobbies
        /// and a boolean indicating whether an error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(LobbyData[] lobbies, bool error)> RequestUniTask(SearchArguments searchArguments, int maxResultsToReturn)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(LobbyData[], bool)>();

            Request(searchArguments, maxResultsToReturn, (lobbies, error) =>
            {
                tcs.TrySetResult((lobbies, error));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Performs a quick match by searching for a lobby that matches the specified search arguments and joining if found.
        /// If no matching lobby is found, a new lobby is created using the provided creation arguments.
        /// </summary>
        /// <param name="searchArguments">The parameters used to search for an existing lobby.</param>
        /// <param name="createArguments">The parameters used to create a new lobby if no suitable lobby is found.</param>
        /// <param name="callback">A callback delegate that is invoked upon completion of the operation, providing the result of the operation, the lobby data, and an error flag if applicable.</param>
        public static void QuickMatch(SearchArguments searchArguments, CreateArguments createArguments,
            Action<EResult, LobbyData, bool> callback)
        {
            Request(searchArguments, 1, (results, error) =>
            {
                if (error)
                {
                    callback?.Invoke(EResult.k_EResultIOFailure, default, true);
                }
                else
                {
                    if (results == null
                        || results.Length < 1)
                    {
                        CreateSession(createArguments.type, createArguments.slots, (eResult, lobby, ioError) =>
                        {
                            if (!ioError && eResult == EResult.k_EResultOK)
                            {
                                foreach(var metadata in createArguments.metadata)
                                {
                                    lobby[metadata.key] = metadata.value;
                                }

                                if (App.IsDebugging)
                                    Debug.Log("Quick match failed to find a lobby so it created one.");

                                callback?.Invoke(eResult, lobby, false);
                            }
                        });
                    }
                    else
                    {
                        results[0].Join((lEnter, joinError) =>
                        {
                            if (joinError)
                                callback?.Invoke(EResult.k_EResultIOFailure, default, true);
                            else
                            {
                                switch(lEnter.Response)
                                {
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseBanned:
                                        if (App.IsDebugging)
                                            Debug.LogWarning("You are banned from this chat room and may not join.");
                                        callback?.Invoke(EResult.k_EResultBanned, lEnter.Lobby, false);
                                        break;
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseCommunityBan:
                                        if (App.IsDebugging)
                                            Debug.LogWarning("Attempt to join a chat when the user has a community lock on their account.");
                                        callback?.Invoke(EResult.k_EResultBanned, lEnter.Lobby, false);
                                        break;
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseMemberBlockedYou:
                                        if (App.IsDebugging)
                                            Debug.LogWarning("Join failed - a user that is in the chat has blocked you from joining.");
                                        callback?.Invoke(EResult.k_EResultBanned, lEnter.Lobby, false);
                                        break;
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseYouBlockedMember:
                                        if (App.IsDebugging)
                                            Debug.LogWarning("Join failed - you have blocked a user that is already in the chat.");
                                        callback?.Invoke(EResult.k_EResultBanned, lEnter.Lobby, false);
                                        break;
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseFull:
                                        if (App.IsDebugging)
                                            Debug.LogWarning("Chat room has reached its maximum size.");
                                        callback?.Invoke(EResult.k_EResultLimitExceeded, lEnter.Lobby, false);
                                        break;
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited:
                                        if (App.IsDebugging)
                                            Debug.LogWarning("Joining this chat is not allowed because you are a limited user (no value on account).");
                                        callback?.Invoke(EResult.k_EResultLimitedUserAccount, lEnter.Lobby, false);
                                        break;
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess:
                                        if (App.IsDebugging)
                                            Debug.Log("Join Success.");
                                        callback?.Invoke(EResult.k_EResultOK, lEnter.Lobby, false);
                                        break;
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseRatelimitExceeded:
                                        if (App.IsDebugging)
                                            Debug.LogWarning("Join failed - too many join attempts in a very short period of time.");
                                        callback?.Invoke(EResult.k_EResultRateLimitExceeded, lEnter.Lobby, false);
                                        break;
                                    default:
                                        if (App.IsDebugging)
                                            Debug.LogWarning("Join failed - unexpected response code");
                                        callback?.Invoke(EResult.k_EResultFail, lEnter.Lobby, false);
                                        break;
                                }
                                
                            }
                        });
                    }
                }
            });
        }

        /// <summary>
        /// Performs a quick match by searching for a lobby that matches the specified search arguments and joining if found asynchronously.
        /// If no matching lobby is found, a new lobby is created using the provided creation arguments.
        /// </summary>
        /// <param name="searchArguments">The parameters used to search for an existing lobby.</param>
        /// <param name="createArguments">The parameters used to create a new lobby if no suitable lobby is found.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static Task<(EResult result, LobbyData lobby, bool ioError)> QuickMatchTask(SearchArguments searchArguments, CreateArguments createArguments)
        {
            var tcs = new TaskCompletionSource<(EResult, LobbyData, bool)>();

            QuickMatch(searchArguments, createArguments, (eResult, lobby, ioError) =>
            {
                tcs.SetResult((eResult, lobby, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Performs a quick match by searching for a lobby that matches the specified search arguments and joining if found asynchronously using UniTask.
        /// If no matching lobby is found, a new lobby is created using the provided creation arguments.
        /// </summary>
        /// <param name="searchArguments">The parameters used to search for an existing lobby.</param>
        /// <param name="createArguments">The parameters used to create a new lobby if no suitable lobby is found.</param>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the result of the operation,
        /// the lobby data, and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public static async Cysharp.Threading.Tasks.UniTask<(EResult result, LobbyData lobby, bool ioError)> QuickMatchUniTask(SearchArguments searchArguments, CreateArguments createArguments)
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(EResult, LobbyData, bool)>();

            QuickMatch(searchArguments, createArguments, (eResult, lobby, ioError) =>
            {
                tcs.TrySetResult((eResult, lobby, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Sends a newly generated authentication ticket to the lobby for verification.
        /// </summary>
        /// <param name="callback">A callback function that receives the generated authentication ticket and a boolean indicating whether an IO error occurred.</param>
        public readonly void Authenticate(Action<AuthenticationTicket, bool> callback)
        {
            var owningUser = Owner.user;
            var thisLobby = this;
            Authentication.GetAuthSessionTicket(owningUser, (ticket, ioError) =>
            {
                thisLobby.Authenticate(new LobbyMessagePayload()
                {
                    id = UserData.Me,
                    data = ticket.Data,
                    inventory = null
                });

                callback?.Invoke(ticket, ioError);
            });
        }

        /// <summary>
        /// Sends a newly generated authentication ticket to the lobby for verification asynchronously.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a tuple with the authentication ticket
        /// and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public readonly Task<(AuthenticationTicket ticket, bool ioError)> AuthenticateTask()
        {
            var tcs = new TaskCompletionSource<(AuthenticationTicket, bool)>();

            Authenticate((ticket, ioError) =>
            {
                tcs.SetResult((ticket, ioError));
            });

            return tcs.Task;
        }

#if UNITASK_INSTALLED
        /// <summary>
        /// Sends a newly generated authentication ticket to the lobby for verification asynchronously using UniTask.
        /// </summary>
        /// <returns>
        /// A UniTask that represents the asynchronous operation. The task result contains a tuple with the authentication ticket
        /// and a boolean indicating whether an I/O error occurred.
        /// </returns>
        public readonly async Cysharp.Threading.Tasks.UniTask<(AuthenticationTicket ticket, bool ioError)> AuthenticateUniTask()
        {
            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource<(AuthenticationTicket, bool)>();

            Authenticate((ticket, ioError) =>
            {
                tcs.TrySetResult((ticket, ioError));
            });

            return await tcs.Task;
        }
#endif

        /// <summary>
        /// Authenticates the user with the lobby, sending an authentication request to the lobby chat system.
        /// </summary>
        /// <param name="data">The payload containing the authentication data, including user identity and optional inventory information.</param>
        /// <returns>True if the authentication request message is successfully sent to the lobby; otherwise, false.</returns>
        public readonly bool Authenticate(LobbyMessagePayload data)
        {
            return Matchmaking.Client.SendLobbyChatMsg(this,
                System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)));
        }

        #region Constants

        /// <summary>
        /// Represents the key used to store or retrieve a lobby's name in its metadata.
        /// This field is commonly used for identifying or displaying the name associated with the lobby.
        /// </summary>
        public const string DataName = "name";

        /// <summary>
        /// Represents a standard metadata key used to store the version of the game in both lobby and member metadata.
        /// This value is commonly used to track or verify the game version across connected lobby members.
        /// </summary>
        public const string DataVersion = "z_heathenGameVersion";

        /// <summary>
        /// Represents a standard metadata field used to indicate that a user is ready to play.
        /// This field is primarily used in the context of lobby member metadata within the Steamworks integration.
        /// </summary>
        public const string DataReady = "z_heathenReady";

        /// <summary>
        /// Represents a metadata field used to track Steam users that should leave the lobby.
        /// This field contains a string formatted as a list of CSteamIDs, where each ID is enclosed in brackets,
        /// e.g., [123456789][987654321]. When present, this indicates that the specified users should not join
        /// this lobby or must leave if they are already members.
        /// </summary>
        public const string DataKick = "z_heathenKick";

        /// <summary>
        /// Represents the Heathen standard metadata field used to indicate the mode of the lobby,
        /// such as party, session, or general. If this value is not set, the mode is assumed to be general.
        /// </summary>
        public const string DataMode = "z_heathenMode";

        /// <summary>
        /// Represents the metadata field used to define the type of lobby in the Heathen Steamworks Integration.
        /// This value determines the lobby's visibility and accessibility settings such as private, friends-only, public, or invisible.
        /// </summary>
        public const string DataType = "z_heathenType";

        /// <summary>
        /// A constant string representing the metadata key used to identify session lobbies in Heathen's Steamworks integration.
        /// This key is primarily used in the context of party lobbies to distinguish lobbies that represent active game sessions.
        /// </summary>
        public const string DataSessionLobby = "z_heathenSessionLobby";

        /// <summary>
        /// Represents the lobby mode set to "General." This constant is used as a marker to categorise a lobby
        /// where no specific session or party constraints are applied, allowing for a general-purpose lobby setup.
        /// </summary>
        public const string DataModeGeneral = "General";

        /// <summary>
        /// Represents a constant string used to denote that the lobby operates in "Session" mode.
        /// This mode is typically associated with sessions where the lobby functions as a temporary environment
        /// for collaborative or multiplayer interactions.
        /// </summary>
        public const string DataModeSession = "Session";

        /// <summary>
        /// Represents the data mode value for a Steam lobby that designates the lobby as a "Party."
        /// This constant is used internally to identify and manage lobbies operating in the Party context.
        /// </summary>
        public const string DataModeParty = "Party";
    #endregion

    #region Boilerplate
        public int CompareTo(CSteamID other)
        {
            return _id.CompareTo(other);
        }

        public int CompareTo(ulong other)
        {
            return _id.CompareTo(other);
        }

        public override string ToString()
        {
            return HexId;
        }

        public bool Equals(CSteamID other)
        {
            return _id.Equals(other.m_SteamID);
        }

        public bool Equals(ulong other)
        {
            return _id.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return _id.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public bool Equals(LobbyData other)
        {
            return _id.Equals(other._id);
        }

        public static bool operator ==(LobbyData l, LobbyData r) => l._id == r._id;
        public static bool operator ==(CSteamID l, LobbyData r) => l.m_SteamID == r._id;
        public static bool operator ==(LobbyData l, CSteamID r) => l._id == r.m_SteamID;
        public static bool operator ==(LobbyData l, ulong r) => l._id == r;
        public static bool operator ==(ulong l, LobbyData r) => l == r._id;
        public static bool operator !=(LobbyData l, LobbyData r) => l._id != r._id;
        public static bool operator !=(CSteamID l, LobbyData r) => l.m_SteamID != r._id;
        public static bool operator !=(LobbyData l, CSteamID r) => l._id != r.m_SteamID;
        public static bool operator !=(LobbyData l, ulong r) => l._id != r;
        public static bool operator !=(ulong l, LobbyData r) => l != r._id;

        public static implicit operator CSteamID(LobbyData c) => c.SteamId;
        public static implicit operator LobbyData(CSteamID id) => new LobbyData { _id = id.m_SteamID };
        public static implicit operator ulong(LobbyData id) => id._id;
        public static implicit operator LobbyData(ulong id) => new LobbyData { _id = id };
        public static implicit operator LobbyData(AccountID_t id) => Get(id);
        public static implicit operator LobbyData(uint id) => Get(id);
        public static implicit operator LobbyData(string id) => Get(id);

    #endregion
    }
}
#endif