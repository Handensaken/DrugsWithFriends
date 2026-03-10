#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("")]
    [ModularComponent(typeof(SteamLobbyData), "Hex Labels", nameof(label))]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyHexIdLabel : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamLobbyData _inspector;

        private void Awake()
        {
            _inspector = GetComponent<SteamLobbyData>();
            _inspector.onChanged.AddListener(HandleOnChanged);
            if (_inspector.Data.IsValid)
                label.text = _inspector.Data.ToString();
        }

        private void OnDestroy()
        {
            _inspector.onChanged.RemoveListener(HandleOnChanged);
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            if (_inspector.Data.IsValid)
            {
                if (label)
                    label.text = _inspector.Data.ToString();
            }
            else
            {
                if (label)
                    label.text = string.Empty;
            }
        }
    }
}
#endif