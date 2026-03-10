#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Tools for working with the Steam Game Server Browser
    /// </summary>
    public class GameServerBrowserManager : MonoBehaviour
    {
        /// <summary>
        /// Simple serializable wrapper around UnityEvent { <see cref="GameServerSearchType"/>, List { <see cref="GameServerData"/> } }
        /// </summary>
        [Serializable]
        public class ResultsEvent : UnityEvent<ResultData>
        { }

        [Serializable]
        public class ResultData
        {
            public GameServerSearchType type;
            public List<GameServerData> entries;
            public bool hasIOFailure;

            public ResultData(GameServerSearchType type, List<GameServerData> entries, bool ioFailure)
            {
                this.type = type;
                this.entries = entries;
                hasIOFailure = ioFailure;
            }
        }

        private class Search
        {
            public HServerListRequest HRequest;
            /// <summary>
            /// <para>
            /// param 1:
            /// <code>List { <see cref="GameServerData"/> }</code>
            /// This is the list of servers found.
            /// </para>
            /// <para>
            /// Param 2:
            /// <code>bool</code>
            /// This indicates a failure e.g. true = failure; while false = no failure 
            /// </para>
            /// </summary>
            public Action<List<GameServerData>, bool> Callback;
            public Action Clear;
            public ISteamMatchmakingServerListResponse MServerListResponse;

            public Search()
            {
                MServerListResponse = new ISteamMatchmakingServerListResponse(OnServerResponded, OnServerFailedToRespond, OnRefreshComplete);
            }

            private void OnServerResponded(HServerListRequest hRequest, int iServer)
            {
                Debug.Log("OnServerResponded: " + hRequest + " - " + iServer);
            }

            private void OnServerFailedToRespond(HServerListRequest hRequest, int iServer)
            {
                Debug.Log("OnServerFailedToRespond: " + hRequest + " - " + iServer);
                if (Callback != null)
                    Callback.Invoke(null, true);
            }

            private void OnRefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response)
            {
                Debug.Log("OnRefreshComplete: " + hRequest + " - " + response);
                List<GameServerData> serverResults = new();
                var count = SteamMatchmakingServers.GetServerCount(hRequest);

                for (int i = 0; i < count; i++)
                {
                    var serverItem = SteamMatchmakingServers.GetServerDetails(hRequest, i);

                    if (serverItem.m_steamID.m_SteamID != 0 && serverItem.m_nAppID == API.App.Id)
                    {
                        GameServerData entry = new GameServerData(serverItem);
                        serverResults.Add(entry);
                    }
                }

                if (hRequest != HServerListRequest.Invalid)
                {
                    SteamMatchmakingServers.ReleaseRequest(hRequest);
                }

                if (Callback != null)
                    Callback(serverResults, false);

                Clear();
            }
        }

        private class PingQuery
        {
            public HServerQuery HQuery;
            public ISteamMatchmakingPingResponse MPingResponse;
            public GameServerData Target;
            public Action<GameServerData, bool> Callback;
            public Action Clear;

            public PingQuery()
            {
                MPingResponse = new ISteamMatchmakingPingResponse(OnServerRespondedPing, OnServerFailedToRespondPing);
            }

            private void OnServerFailedToRespondPing()
            {
                if (HQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(HQuery);
                }

                Callback?.Invoke(Target, true);

                Clear?.Invoke();
            }

            private void OnServerRespondedPing(gameserveritem_t server)
            {
                if (HQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(HQuery);
                }

                if (Target != null)
                {
                    Target.Update(server);
                    Target.evtDataUpdated.Invoke();
                    Callback?.Invoke(Target, false);
                }
                else
                {
                    Callback?.Invoke(new GameServerData(server), false);
                }

                Clear?.Invoke();
            }
        }

        private class PlayerQuery
        {
            public HServerQuery HQuery;
            public ISteamMatchmakingPlayersResponse MPlayersResponse;
            public GameServerData Target;
            public Action<GameServerData, bool> Callback;
            public Action Clear;

            public PlayerQuery()
            {
                MPlayersResponse = new ISteamMatchmakingPlayersResponse(OnAddPlayerToList, OnPlayersFailedToRespond, OnPlayersRefreshComplete);
            }

            private void OnPlayersRefreshComplete()
            {
                if (HQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(HQuery);
                }

                Target.evtDataUpdated.Invoke();

                Callback?.Invoke(Target, false);

                Clear?.Invoke();
            }

            private void OnPlayersFailedToRespond()
            {
                if (HQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(HQuery);
                }

                Callback?.Invoke(Target, true);

                Clear?.Invoke();
            }

            private void OnAddPlayerToList(string pchName, int nScore, float flTimePlayed)
            {
                Target.players.Add(new ServerPlayerEntry() { name = pchName, score = nScore, TimePlayed = new TimeSpan(0, 0, 0, (int)flTimePlayed, 0) });
            }
        }

        private class RulesQuery
        {
            public HServerQuery HQuery;
            public ISteamMatchmakingRulesResponse MRulesResponse;
            public GameServerData Target;
            /// <summary>
            /// <para>
            /// param 1:
            /// <code><see cref="GameServerData"/></code>
            /// This is the server to work against.
            /// </para>
            /// <para>
            /// Param 2:
            /// <code>bool</code>
            /// This indicates a falure e.g. true = failure; while false = no failure 
            /// </para>
            /// </summary>
            public Action<GameServerData, bool> Callback;
            public Action Clear;

            public RulesQuery()
            {
                MRulesResponse = new ISteamMatchmakingRulesResponse(OnAddRuleToList, OnRulesFailedToRespond, OnRulesRefreshComplete);
            }

            private void OnAddRuleToList(string pchRule, string pchValue)
            {
                Target.rules.Add(new StringKeyValuePair { key = pchRule, value = pchValue });
            }

            private void OnRulesRefreshComplete()
            {
                if (HQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(HQuery);
                }

                Target.evtDataUpdated.Invoke();

                if (Callback != null)
                    Callback(Target, false);

                if (Clear != null)
                    Clear();
            }

            private void OnRulesFailedToRespond()
            {
                if (HQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(HQuery);
                }

                Callback?.Invoke(Target, true);

                Clear?.Invoke();
            }
        }

        /// <summary>
        /// Simple dictionary wrapper to simplify the filter parameter of search methods.
        /// </summary>
        /// <remarks>
        /// Any parameter of <see cref="Filter"/> can be passed in simply via:
        /// <code>new GameServerBrowser.Filter{ {"key1", "value1"}, {"key2", "value2"} }</code>
        /// This is a simple Dictionary{string, string} so you can construct it in traditional ways as well e.g.
        /// <code>var filter = new GameServerBrowser.Filter();</code>
        /// <code>filter.Add("key1", "value1");</code>
        /// <code>filter.Add("key2", "value2");</code>
        /// </remarks>
        public class Filter : Dictionary<string, string>
        {
            public MatchMakingKeyValuePair_t[] Array
            {
                get
                {
                    var array = new MatchMakingKeyValuePair_t[Count];
                    int index = 0;
                    foreach (var pair in this)
                    {
                        array[index] = new MatchMakingKeyValuePair_t() { m_szKey = pair.Key, m_szValue = pair.Value };
                        index++;
                    }

                    return array;
                }
            }
        }

        private readonly List<Search> _searchList = new List<Search>();
        private readonly List<PingQuery> _pingList = new List<PingQuery>();
        private readonly List<PlayerQuery> _playerList = new List<PlayerQuery>();
        private readonly List<RulesQuery> _ruleList = new List<RulesQuery>();
        public ResultsEvent evtSearchCompleted = new ResultsEvent();

        public void GetAllFavorites() => GetServerList(API.App.Id, GameServerSearchType.Favorites, null, null);
        public void GetAllFriends() => GetServerList(API.App.Id, GameServerSearchType.Friends, null, null);
        public void GetAllHistory() => GetServerList(API.App.Id, GameServerSearchType.History, null, null);
        public void GetAllInternet() => GetServerList(API.App.Id, GameServerSearchType.Internet, null, null);
        public void GetAllLan() => GetServerList(API.App.Id, GameServerSearchType.Lan, null, null);
        public void GetAllSpectator() => GetServerList(API.App.Id, GameServerSearchType.Spectator, null, null);
        public void GetFavorites(Filter filter) => GetServerList(API.App.Id, GameServerSearchType.Favorites, null, filter);
        public void GetFriends(Filter filter) => GetServerList(API.App.Id, GameServerSearchType.Friends, null, filter);
        public void GetHistory(Filter filter) => GetServerList(API.App.Id, GameServerSearchType.History, null, filter);
        public void GetInternet(Filter filter) => GetServerList(API.App.Id, GameServerSearchType.Internet, null, filter);
        public void GetLan(Filter filter) => GetServerList(API.App.Id, GameServerSearchType.Lan, null, filter);
        public void GetSpectator(Filter filter) => GetServerList(API.App.Id, GameServerSearchType.Spectator, null, filter);
        /// <summary>
        /// Fetch a list of server data from Valve for this app id
        /// </summary>
        /// <param name="type">The type of servers to return</param>
        /// <param name="callback">The action to be called when the process is complete.
        /// param 1 of type <see cref="bool"/> indicates success or failure while param 2 is a list of <see cref="GameServerData"/> representing each server found</param>
        /// <param name="filter">a set of key value pairs representing the search filter</param>
        public void GetServerList(GameServerSearchType type, Action<List<GameServerData>, bool> callback = null, Filter filter = null)
        {
            GetServerList(API.App.Id, type, callback, filter);
        }
        /// <summary>
        /// Fetch a list of server data from Valve for the app id indicated by <paramref name="appId"/>
        /// </summary>
        /// <param name="appId">The app ID to search for</param>
        /// <param name="type">The type of servers to return</param>
        /// <param name="callback">The action to be called when the process is complete.
        /// param 1 of type <see cref="bool"/> indicates success or failure while param 2 is a list of <see cref="GameServerData"/> representing each server found</param>
        /// <param name="filter">a set of key value pairs representing the search filter</param>
        public void GetServerList(AppId_t appId, GameServerSearchType type, Action<List<GameServerData>, bool> callback = null, Filter filter = null)
        {
            var nSearch = new Search();
            nSearch.Clear = () => _searchList.Remove(nSearch);

            MatchMakingKeyValuePair_t[] filters = new MatchMakingKeyValuePair_t[0];

            if (filter != null)
                filters = filter.Array;

            switch (type)
            {
                case GameServerSearchType.Favorites:
                    nSearch.Callback = (r, e) =>
                        {
                            callback?.Invoke(r, e);
                            evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.Favorites, r, e));
                        };
                    API.Matchmaking.Client.RequestFavoritesServerList(appId, filters, nSearch.MServerListResponse);
                    break;
                case GameServerSearchType.Friends:
                    nSearch.Callback = (r, e) =>
                    {
                        callback?.Invoke(r, e);
                        evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.Friends, r, e));
                    };
                    API.Matchmaking.Client.RequestFriendsServerList(appId, filters, nSearch.MServerListResponse);
                    break;
                case GameServerSearchType.History:
                    nSearch.Callback = (r, e) =>
                    {
                        callback?.Invoke(r, e);
                        evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.History, r, e));
                    };
                    API.Matchmaking.Client.RequestHistoryServerList(appId, filters, nSearch.MServerListResponse);
                    break;
                case GameServerSearchType.Internet:
                    nSearch.Callback = (r, e) =>
                    {
                        callback?.Invoke(r, e);
                        evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.Internet, r, e));
                    };
                    API.Matchmaking.Client.RequestInternetServerList(appId, filters, nSearch.MServerListResponse);
                    break;
                case GameServerSearchType.Lan:
                    nSearch.Callback = (r, e) =>
                    {
                        callback?.Invoke(r, e);
                        evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.Lan, r, e));
                    };
                    API.Matchmaking.Client.RequestLanServerList(appId, nSearch.MServerListResponse);
                    break;
                case GameServerSearchType.Spectator:
                    nSearch.Callback = (r, e) =>
                    {
                        callback?.Invoke(r, e);
                        evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.Spectator, r, e));
                    };
                    API.Matchmaking.Client.RequestSpectatorServerList(appId, filters, nSearch.MServerListResponse);
                    break;
            }

            _searchList.Add(nSearch);
        }

        /// <summary>
        /// Ping the target server fetching updated data for it
        /// </summary>
        /// <param name="ipAddress">The address of the server to ping</param>
        /// <param name="port">The port of the server to ping</param>
        /// <param name="callback">The action to call when the process is complete</param>
        public void PingServer(string ipAddress, ushort port, Action<GameServerData, bool> callback)
        {
            PingServer(API.Utilities.IPStringToUint(ipAddress), port, callback);
        }

        /// <summary>
        /// Ping the target server fetching updated data for it
        /// </summary>
        /// <param name="ipAddress">The address of the server to ping</param>
        /// <param name="port">The port of the server to ping</param>
        /// <param name="callback">The action to call when the process is complete</param>
        public void PingServer(uint ipAddress, ushort port, Action<GameServerData, bool> callback)
        {
            var nQuery = new PingQuery();
            nQuery.Callback = callback;
            nQuery.HQuery = API.Matchmaking.Client.PingServer(ipAddress, port, nQuery.MPingResponse);
            nQuery.Clear = () => _pingList.Remove(nQuery);

            _pingList.Add(nQuery);
        }

        /// <summary>
        /// Ping the target server fetching updated data for it
        /// </summary>
        /// <param name="address">the server net address to ping</param>
        /// <param name="callback">The action to call when the process is complete</param>
        public void PingServer(servernetadr_t address, Action<GameServerData, bool> callback)
        {
            PingServer(address.GetIP(), address.GetQueryPort(), callback);
        }

        /// <summary>
        /// Ping the target server fetching updated data for it
        /// </summary>
        /// <param name="entry">the server to ping</param>
        /// <param name="callback">The action to call when the process is complete</param>
        public void PingServer(GameServerData entry, Action<GameServerData, bool> callback)
        {
            var nQuery = new PingQuery();
            nQuery.Callback = callback;
            nQuery.Target = entry;
            nQuery.HQuery = API.Matchmaking.Client.PingServer(entry.m_NetAdr.GetIP(), entry.m_NetAdr.GetQueryPort(), nQuery.MPingResponse);
            nQuery.Clear = () => _pingList.Remove(nQuery);

            _pingList.Add(nQuery);
        }

        /// <summary>
        /// Clears the player list then requests fresh player data from Valve
        /// </summary>
        /// <param name="entry">The server target to request the data for</param>
        /// <param name="callback"></param>
        public void PlayerDetails(GameServerData entry, Action<GameServerData, bool> callback)
        {
            var nQuery = new PlayerQuery();
            nQuery.Callback = callback;
            entry.players.Clear();
            nQuery.Target = entry;
            nQuery.HQuery = API.Matchmaking.Client.PlayerDetails(entry.m_NetAdr.GetIP(), entry.m_NetAdr.GetQueryPort(), nQuery.MPlayersResponse);
            nQuery.Clear = () => _playerList.Remove(nQuery);

            _playerList.Add(nQuery);
        }

        /// <summary>
        /// Clears the rules list then requests fresh rule data from Valve
        /// </summary>
        /// <param name="entry">The server target to request the data for</param>
        /// <param name="callback"></param>
        public void ServerRules(GameServerData entry, Action<GameServerData, bool> callback)
        {
            var nQuery = new RulesQuery();
            nQuery.Callback = callback;
            entry.rules.Clear();
            nQuery.Target = entry;
            nQuery.HQuery = API.Matchmaking.Client.ServerRules(entry.m_NetAdr.GetIP(), entry.m_NetAdr.GetQueryPort(), nQuery.MRulesResponse);
            nQuery.Clear = () => _ruleList.Remove(nQuery);

            _ruleList.Add(nQuery);
        }

        private void OnDestroy()
        {
            if (_searchList != null)
            {
                foreach (var search in _searchList)
                {
                    try
                    {
                        if (search.HRequest != HServerListRequest.Invalid)
                            SteamMatchmakingServers.ReleaseRequest(search.HRequest);
                    }
                    catch { }
                }
            }

            if(_pingList != null)
            {
                foreach(var ping in _pingList)
                {
                    try
                    {
                        if (ping.HQuery != HServerQuery.Invalid)
                            SteamMatchmakingServers.CancelServerQuery(ping.HQuery);
                    }
                    catch { }
                }
            }

            if(_playerList != null)
            {
                foreach (var player in _playerList)
                {
                    try
                    {
                        if (player.HQuery != HServerQuery.Invalid)
                            SteamMatchmakingServers.CancelServerQuery(player.HQuery);
                    }
                    catch { }
                }
            }

            if (_ruleList != null)
            {
                foreach (var rule in _ruleList)
                {
                    try
                    {
                        if (rule.HQuery != HServerQuery.Invalid)
                            SteamMatchmakingServers.CancelServerQuery(rule.HQuery);
                    }
                    catch { }
                }
            }
        }
    }
}
#endif