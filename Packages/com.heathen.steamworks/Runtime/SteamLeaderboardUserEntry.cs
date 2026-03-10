#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLeaderboardData), "User Entries", nameof(entryUI))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    [RequireComponent(typeof(SteamLeaderboardDataEvents))]
    public class SteamLeaderboardUserEntry : MonoBehaviour
    {
        public SteamLeaderboardEntryUI entryUI;

        private SteamLeaderboardData _mInspector;
        private SteamLeaderboardDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLeaderboardData>();
            _mEvents = GetComponent<SteamLeaderboardDataEvents>();

            if (_mEvents != null)
            {
                _mEvents.onChange?.AddListener(Refresh);
                _mEvents.onRankChanged?.AddListener(HandleRankChange);
            }
        }

        private void OnDestroy()
        {
            if (_mEvents != null)
            {
                _mEvents.onChange?.RemoveListener(Refresh);
                _mEvents.onRankChanged?.RemoveListener(HandleRankChange);
            }
        }

        public void Refresh()
        {
            if (_mInspector.Data.IsValid && entryUI != null)
            {
                _mInspector.Data.GetUserEntry(0, (entry, ioError) =>
                {
                    if (!ioError && entry != null)
                        entryUI.Entry = entry;
                    else
                        entryUI.Entry = new()
                        {
                            Entry = new Steamworks.LeaderboardEntry_t()
                            {
                                m_steamIDUser = UserData.Me,
                                m_nGlobalRank = 0,
                                m_nScore = 0
                            },
                            Details = new int[0],
                        };
                });
            }
        }

        private void HandleRankChange(LeaderboardScoreUploaded arg0)
        {
            Refresh();
        }
    }
}
#endif