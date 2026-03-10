#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Structure of the player entry data returned by the <see cref="GameServerBrowserManager.PlayerDetails(GameServerData, Action{GameServerData, bool})"/> method
    /// </summary>
    [Serializable]
    public class ServerPlayerEntry
    {
        public string name;
        public int score;
        public TimeSpan TimePlayed;
    }
}
#endif