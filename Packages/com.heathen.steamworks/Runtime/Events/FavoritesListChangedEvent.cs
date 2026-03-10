#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public class FavoritesListChangedEvent : UnityEvent<FavoritesListChanged> { }

    [Serializable]
    public struct FavoritesListChanged
    {
        public string ip;
        public uint queryPort;
        public uint connectionPort;
        public AppData app;
        public uint flags;
        public bool add;
        public AccountID_t accountId;
        
        public FavoritesListChanged(string ip, uint queryPort, uint connectionPort, AppData app, uint flags, bool add, AccountID_t accountId) : this()
        {
            this.ip = ip;
            this.queryPort = queryPort;
            this.connectionPort = connectionPort;
            this.app = app;
            this.flags = flags;
            this.add = add;
            this.accountId = accountId;
        }
    }
}
#endif