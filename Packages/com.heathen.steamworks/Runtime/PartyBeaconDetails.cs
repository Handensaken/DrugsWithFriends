#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct PartyBeaconDetails
    {
        public PartyBeaconID_t id;
        public UserData owner;
        public SteamPartyBeaconLocation_t Location;
        public string metadata;
    }
    //*/

}
#endif