#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Used in the <see cref="SteamLobbyData.evtUserLeft"/> event to indicate what user left the lobby
    /// </summary>
    [System.Serializable]
    public struct UserLobbyLeaveData
    {
        /// <summary>
        /// The user that left the lobby
        /// </summary>
        public UserData user;
        /// <summary>
        /// The state the user left under
        /// </summary>
        public Steamworks.EChatMemberStateChange state;
    }
}
#endif