#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public class LeaderboardScoresDownloadedEvent : UnityEvent<LeaderboardScores>
    { }
}
#endif
