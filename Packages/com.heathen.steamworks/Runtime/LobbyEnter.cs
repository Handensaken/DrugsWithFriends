#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the response to a lobby enter request.
    /// </summary>
    [Serializable]
    public struct LobbyEnter
    {
        /// <summary>
        /// The native Steamworks data.
        /// </summary>
        public LobbyEnter_t Data;
        /// <summary>
        /// The lobby that was entered.
        /// </summary>
        public LobbyData Lobby => Data.m_ulSteamIDLobby;
        /// <summary>
        /// The response to the enter request.
        /// </summary>
        public EChatRoomEnterResponse Response => (EChatRoomEnterResponse)Data.m_EChatRoomEnterResponse;
        /// <summary>
        /// Is the lobby locked?
        /// </summary>
        public bool Locked => Data.m_bLocked;

        /// <summary>
        /// Implicitly converts a native <see cref="LobbyEnter_t"/> to a <see cref="LobbyEnter"/>.
        /// </summary>
        /// <param name="native">The native data to convert.</param>
        public static implicit operator LobbyEnter(LobbyEnter_t native) => new LobbyEnter { Data = native };
        /// <summary>
        /// Implicitly converts a <see cref="LobbyEnter"/> to a native <see cref="LobbyEnter_t"/>.
        /// </summary>
        /// <param name="heathen">The Heathen data to convert.</param>
        public static implicit operator LobbyEnter_t(LobbyEnter heathen) => heathen.Data;
    }
}
#endif