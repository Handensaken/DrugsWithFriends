#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLeaderboardData), "Names", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    public class SteamLeaderboardName : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamLeaderboardData _mInspector;
        private SteamLeaderboardDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLeaderboardData>();
            _mEvents = GetComponent<SteamLeaderboardDataEvents>();
            if(_mEvents != null)
                _mEvents.onChange?.AddListener(HandleOnChanged);

            if (_mInspector.Data.IsValid)
                label.text = _mInspector.Data.DisplayName;
        }

        private void OnDestroy()
        {
            if (_mEvents != null)
                _mEvents.onChange?.RemoveListener(HandleOnChanged);
        }

        private void HandleOnChanged()
        {
            if (_mInspector.Data.IsValid)
                label.text = _mInspector.Data.DisplayName;
            else
                label.text = string.Empty;
        }
    }
}
#endif