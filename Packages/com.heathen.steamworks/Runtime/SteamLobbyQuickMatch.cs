#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Quick Match", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyQuickMatch : MonoBehaviour
    {
        public enum SteamLobbyType
        {
            Private = 0,        // the only way to join the lobby is to invite someone else
            FriendsOnly = 1,    // shows for friends or invitees, but not in the lobby list
            Public = 2,         // visible for friends and in the lobby list
            Invisible = 3,      // returned by search, but not visible to other friends
        }

        public enum LobbyDistanceFilter
        {
            Close,        // only lobbies in the same immediate region will be returned
            Default,      // only lobbies in the same region or nearby regions
            Far,          // for games that have few latency requirements, will return lobbies about half-way around the globe
            Worldwide,    // no filtering, will match lobbies as far as India to NY (not recommended, expect multiple seconds of latency between the clients)
        }

        /// <summary>
        /// If true, the search will check if there is currently a party lobby if so, it will search for a lobby enough slots for each party member, else it will obey the slots field.
        /// </summary>
        [SettingsField(0, true)]
        [Tooltip("If true the search will check if there is currently a party lobby if so it will search for a lobby enough slots for each party member, else it will obey the slots field.")]
        public bool partyWise;
        /// <summary>
        /// The type of lobby to create
        /// </summary>
        [SettingsField(0, false, "Quick Match")]
        [Tooltip("The type of lobby to create")]
        public SteamLobbyType type = SteamLobbyType.Public;
        /// <summary>
        /// The type of lobby to create
        /// </summary>
        [SettingsField(0,false, "Quick Match")]
        [Tooltip("The number of slots to create the lobby with if created.")]
        public int slotsOnCreate = 1;
        /// <summary>
        /// The distance from the searching user that should be considered when searching
        /// </summary>
        [Header("Search Arguments")]
        [SettingsField(0, false,"Quick Match")]
        [Tooltip("The distance from the searching user that should be considered when searching")]
        public LobbyDistanceFilter distance = LobbyDistanceFilter.Default;
        /// <summary>
        /// Metadata values that should be used to sort the results e.g. values `closer` to these values will be weighted higher in the results
        /// </summary>
        [SettingsField(0, false, "Quick Match")]
        [Tooltip("Metadata values that should be used to sort the results e.g. values `closer` to these values will be weighted higher in the results")]
        public List<NearFilter> nearValues = new();
        /// <summary>
        /// Metadata values, which should be compared as numeric values e.g., should follow typical maths rules for concepts such as less than, greater than, etc.
        /// </summary>
        [SettingsField(0, false,"Quick Match")]
        [Tooltip("Metadata values that should be compared as numeric values e.g., should follow typical maths rules for concepts such as less than, greater than, etc.")]
        public List<NumericFilter> numericFilters = new();
        /// <summary>
        /// Metadata values that should be compared as strings
        /// </summary>
        [SettingsField(0, false,"Quick Match")]
        [Tooltip("Metadata values that should be compared as strings")]
        public List<StringFilter> stringFilters = new();

        private int _slots = 1;
        private SteamLobbyData _inspector;
        private SteamLobbyDataEvents _events;

        private void Awake()
        {
            _inspector = GetComponent<SteamLobbyData>();
            _events = GetComponent<SteamLobbyDataEvents>();
        }

        public void Match()
        {
            LobbyData partyLobby = CSteamID.Nil;

            if (partyWise && LobbyData.PartyLobby(out partyLobby))
            {
                if (!partyLobby.IsOwner)
                {
                    Debug.LogWarning("Only a party lobby leader can create or join lobbies");
                    return;
                }
                else
                    _slots = partyLobby.MemberCount;
            }
            else
                _slots = 1;

            API.Matchmaking.Client.AddRequestLobbyListDistanceFilter((ELobbyDistanceFilter)distance);

            if (_slots > 0)
                API.Matchmaking.Client.AddRequestLobbyListFilterSlotsAvailable(_slots);

            foreach (var near in nearValues)
                API.Matchmaking.Client.AddRequestLobbyListNearValueFilter(near.key, near.value);

            foreach (var numeric in numericFilters)
                API.Matchmaking.Client.AddRequestLobbyListNumericalFilter(numeric.key, numeric.value, numeric.comparison);

            foreach (var text in stringFilters)
                API.Matchmaking.Client.AddRequestLobbyListStringFilter(text.key, text.value, text.comparison);

            API.Matchmaking.Client.AddRequestLobbyListResultCountFilter(1);

            API.Matchmaking.Client.RequestLobbyList((r, e) =>
            {
                if (!e)
                {
                    if (r.Length > 0)
                    {
                        // We found a lobby ... try and join it
                        r[0].Join((enterLobby, enterIoError) =>
                        {
                            if (!enterIoError && enterLobby.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                            {
                                _inspector.Data = enterLobby.Lobby;
                                if (_events != null)
                                    _events.onEnterSuccess?.Invoke(enterLobby.Lobby);

                                if (partyLobby.IsValid)
                                {
                                    partyLobby[LobbyData.DataSessionLobby] = enterLobby.Lobby.ToString();
                                }
                            }
                            else
                            {
                                // We failed to join the said lobby ... so create one instead
                                slotsOnCreate = Convert.ToInt32(Mathf.Max(1, slotsOnCreate));
                                LobbyData.Create((ELobbyType)type, SteamLobbyModeType.Session, slotsOnCreate, (createResult, createdLobby, createIoError) =>
                                {
                                    if (!createIoError && createResult == EResult.k_EResultOK)
                                    {
                                        _inspector.Data = createdLobby;

                                        if (_events != null)
                                            _events.onCreate?.Invoke(createdLobby);

                                        if (partyLobby.IsValid)
                                        {
                                            partyLobby[LobbyData.DataSessionLobby] = createdLobby.ToString();
                                        }
                                    }
                                    else
                                    {
                                        if (_events != null)
                                            _events.onCreationFailure?.Invoke(createResult);
                                    }
                                });
                            }
                        });
                    }
                    else
                    {
                        // No lobby found create a new one
                        slotsOnCreate = Convert.ToInt32(Mathf.Max(1, slotsOnCreate));
                        LobbyData.Create((ELobbyType)type, SteamLobbyModeType.Session, slotsOnCreate, (createResult, createdLobby, createIoError) =>
                        {
                            if (!createIoError && createResult == EResult.k_EResultOK)
                            {
                                _inspector.Data = createdLobby;

                                if (_events != null)
                                    _events.onCreate?.Invoke(createdLobby);

                                if (partyLobby.IsValid)
                                {
                                    partyLobby[LobbyData.DataSessionLobby] = createdLobby.ToString();
                                }
                            }
                            else
                            {
                                if (_events != null)
                                    _events.onCreationFailure?.Invoke(createResult);
                            }
                        });
                    }
                }
                else
                {
                    // We failed to search for a lobby ... try and create one
                    slotsOnCreate = Convert.ToInt32(Mathf.Max(1, slotsOnCreate));
                    LobbyData.Create((ELobbyType)type, SteamLobbyModeType.Session, slotsOnCreate, (createResult, createdLobby, createIoError) =>
                    {
                        if (!createIoError && createResult == EResult.k_EResultOK)
                        {
                            _inspector.Data = createdLobby;

                            if (_events != null)
                                _events.onCreate?.Invoke(createdLobby);

                            if (partyLobby.IsValid)
                            {
                                partyLobby[LobbyData.DataSessionLobby] = createdLobby.ToString();
                            }
                        }
                        else
                        {
                            if (_events != null)
                                _events.onCreationFailure?.Invoke(createResult);
                        }
                    });
                }
            });
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyQuickMatch), true)]
    public class SteamLobbyQuickMatchEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif