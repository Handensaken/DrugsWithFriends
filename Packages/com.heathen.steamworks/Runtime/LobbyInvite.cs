#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct LobbyInvite : IEquatable<LobbyInvite>, IComparable<LobbyInvite>
    {
        public LobbyInvite_t Data;

        public readonly UserData FromUser => Data.m_ulSteamIDUser;
        public readonly LobbyData ToLobby => Data.m_ulSteamIDLobby;
        public readonly GameData ForGame => Data.m_ulGameID;

        public static implicit operator LobbyInvite(LobbyInvite_t native) => new LobbyInvite { Data = native };
        public static implicit operator LobbyInvite_t(LobbyInvite heathen) => heathen.Data;

        public readonly bool Equals(LobbyInvite other) =>
            Data.m_ulSteamIDLobby == other.Data.m_ulSteamIDLobby &&
            Data.m_ulSteamIDUser == other.Data.m_ulSteamIDUser &&
            Data.m_ulGameID == other.Data.m_ulGameID;

        public override readonly bool Equals(object obj) => obj is LobbyInvite other && Equals(other);

        public override readonly int GetHashCode() =>
            HashCode.Combine(Data.m_ulSteamIDLobby, Data.m_ulSteamIDUser, Data.m_ulGameID);

        public int CompareTo(LobbyInvite other) =>
            Data.m_ulSteamIDLobby.CompareTo(other.Data.m_ulSteamIDLobby);

        public static bool operator ==(LobbyInvite left, LobbyInvite right) => left.Equals(right);
        public static bool operator !=(LobbyInvite left, LobbyInvite right) => !left.Equals(right);
        public static bool operator <(LobbyInvite left, LobbyInvite right) => left.CompareTo(right) < 0;
        public static bool operator >(LobbyInvite left, LobbyInvite right) => left.CompareTo(right) > 0;
        public static bool operator <=(LobbyInvite left, LobbyInvite right) => left.CompareTo(right) <= 0;
        public static bool operator >=(LobbyInvite left, LobbyInvite right) => left.CompareTo(right) >= 0;

        public override string ToString() =>
            $"LobbyInvite from {Data.m_ulSteamIDUser} to {Data.m_ulSteamIDLobby} for {Data.m_ulGameID}";
    }
}
#endif