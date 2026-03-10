#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct PersonaStateChange
    {
        public PersonaStateChange_t Data;
        public readonly CSteamID SubjectId => new CSteamID(Data.m_ulSteamID);
        public readonly EPersonaChange Flags => Data.m_nChangeFlags;

        public static implicit operator PersonaStateChange(PersonaStateChange_t native) => new PersonaStateChange { Data = native };
        public static implicit operator PersonaStateChange_t(PersonaStateChange heathen) => heathen.Data;
    }
}
#endif