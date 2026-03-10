#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLeaderboardData), "Ranks", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    [RequireComponent(typeof(SteamLeaderboardDataEvents))]
    public class SteamLeaderboardRank : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamLeaderboardData _inspector;
        private SteamLeaderboardDataEvents _events;

        private void Awake()
        {
            _inspector = GetComponent<SteamLeaderboardData>();
            _events = GetComponent<SteamLeaderboardDataEvents>();

            if (_events != null)
            {
                _events.onChange?.AddListener(HandleOnChanged);
                _events.onRankChanged?.AddListener(HandleRankChange);
            }

            if (_inspector.Data.IsValid)
            {
                _inspector.Data.GetUserEntry(0, (entry, ioError) =>
                {
                    if (!ioError && entry != null)
                        label.text = entry.Rank.ToString();
                    else
                        label.text = string.Empty;
                });
            }
        }

        private void OnDestroy()
        {
            if (!_events) return;
            _events.onChange?.RemoveListener(HandleOnChanged);
            _events.onRankChanged?.RemoveListener(HandleRankChange);
        }

        private void HandleRankChange(LeaderboardScoreUploaded arg0)
        {
            label.text = arg0.GlobalRankNew.ToString();
        }

        private void HandleOnChanged()
        {
            label.text = _inspector.Data.IsValid ? _inspector.Data.DisplayName : string.Empty;
        }
    }
}
#endif