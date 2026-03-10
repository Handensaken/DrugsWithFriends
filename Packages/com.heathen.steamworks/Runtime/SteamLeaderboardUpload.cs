#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Features for uploading and attaching data to a leaderboard
    /// </summary>
    [ModularComponent(typeof(SteamLeaderboardData), "Upload", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    public class SteamLeaderboardUpload : MonoBehaviour
    {
        public enum Mode
        {
            KeepBest,
            ForceUpdate
        }

        [SettingsField(0,false,"Upload")]
        public Mode mode = Mode.KeepBest;
        [SettingsField(0,false, "Upload")]
        public int score = 0;
        [SettingsField(0,false, "Upload")]
        public List<int> details = new();

        private SteamLeaderboardData _mInspector;
        private SteamLeaderboardDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLeaderboardData>();
            _mEvents = GetComponent<SteamLeaderboardDataEvents>();
        }

        public void Upload()
        {
            if(_mInspector.Data.IsValid)
            {
                if (_mEvents == null)
                {
                    if (mode == Mode.KeepBest)
                        _mInspector.Data.UploadScoreKeepBest(score, details.ToArray());
                    else
                        _mInspector.Data.UploadScoreForceUpdate(score, details.ToArray());
                }
            }
        }

        public void Upload<T>(T attachment)
        {
            if (!typeof(T).IsSerializable)
                throw new InvalidOperationException($"{typeof(T)} must be [Serializable]");

            if (_mInspector.Data.IsValid)
            {
                if (mode == Mode.KeepBest)
                    _mInspector.Data.UploadScoreKeepBest(score, details.ToArray());
                else
                    _mInspector.Data.UploadScoreForceUpdate(score, details.ToArray());
            }
        }
    }
}
#endif