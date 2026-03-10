#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public enum OverlayDialog
    {
        Friends,
        Community,
        Players,
        Settings,
        Officalgamegroup,
        Stats,
        Achievements,
    }
}
#endif