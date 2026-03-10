#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    [ModularEvents(typeof(SteamLeaderboardData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    public class SteamLeaderboardDataEvents : MonoBehaviour
    {
        [EventField]
        public UnityEvent onChange;
        [EventField]
        public UnityEvent onFindOrCreate;
        [EventField]
        public UnityEvent onFindOrCreateFailure;
        [EventField]
        public UnityEvent<LeaderboardScoreUploaded> onScoreUploaded;
        [EventField]
        public UnityEvent<LeaderboardScoreUploaded> onRankChanged;
        [FormerlySerializedAs("onUGCAttached")] [EventField]
        public UnityEvent<LeaderboardUgcSet> onUgcAttached;

        private SteamLeaderboardData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLeaderboardData>();
            API.Leaderboards.Client.OnScoreUploaded.AddListener(HandleScoreUpload);
            API.Leaderboards.Client.OnUgcAttached.AddListener(HandleUgcAttached);
        }

        private void OnDestroy()
        {
            API.Leaderboards.Client.OnScoreUploaded.RemoveListener(HandleScoreUpload);
            API.Leaderboards.Client.OnUgcAttached.RemoveListener(HandleUgcAttached);
        }

        private void HandleUgcAttached(LeaderboardUgcSet arg0, bool arg1)
        {
            if (!arg1
                && arg0.Leaderboard == _mInspector.Data)
            {
                onUgcAttached.Invoke(arg0);
            }
        }

        private void HandleScoreUpload(LeaderboardScoreUploaded arg0, bool arg1)
        {
            if (!arg1
                && arg0.Leaderboard == _mInspector.Data)
            {
                onScoreUploaded.Invoke(arg0);

                if (arg0.GlobalRankNew != arg0.GlobalRankPrevious)
                    onRankChanged?.Invoke(arg0);
            }
        }
    }
}
#endif