#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Hex Inputs", nameof(input))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyHexIdInputField : MonoBehaviour
    {
        public TMPro.TMP_InputField input;

        private SteamLobbyData _inspector;

        private void Awake()
        {
            _inspector = GetComponent<SteamLobbyData>();
            _inspector.onChanged.AddListener(HandleOnChanged);
            if (input != null)
                input.text = _inspector.Data.ToString();
        }

        private void OnDestroy()
        {
            _inspector.onChanged.RemoveListener(HandleOnChanged);
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            if (_inspector.Data.IsValid)
            {
                if (input)
                    input.text = _inspector.Data.ToString();
            }
            else
            {
                if (input)
                    input.text = string.Empty;
            }
        }
    }
}
#endif