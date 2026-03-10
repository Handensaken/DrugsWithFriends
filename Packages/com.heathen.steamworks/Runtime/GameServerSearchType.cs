#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Enumerator used in Game Server Browser searches
    /// </summary>
    public enum GameServerSearchType
    {
        Internet,
        Friends,
        Favorites,
        Lan,
        Spectator,
        History
    }
}
#endif