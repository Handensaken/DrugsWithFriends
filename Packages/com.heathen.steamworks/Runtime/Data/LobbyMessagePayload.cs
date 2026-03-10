#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Used with the <see cref="LobbyData.Authenticate(LobbyMessagePayload)"/> feature to authenticate a lobby member with the host of the lobby optionally validating inventory ownership as well.
    /// </summary>
    [Serializable]
    public struct LobbyMessagePayload
    {
        /// <summary>
        /// The Steam ID of the user.
        /// </summary>
        public ulong id;
        /// <summary>
        /// The authentication ticket data.
        /// </summary>
        public byte[] data;
        /// <summary>
        /// The inventory ownership validation data.
        /// </summary>
        public byte[] inventory;
    }
}
#endif