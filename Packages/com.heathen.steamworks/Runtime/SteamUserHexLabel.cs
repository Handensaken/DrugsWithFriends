#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Heathen.SteamworksIntegration.API;
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamUserData), "Hex Labels", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserHexLabel : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamUserData _mSteamUserData;

        private void Awake()
        {
            _mSteamUserData = GetComponent<SteamUserData>();
            if (_mSteamUserData.Data.IsValid)
            {
                label.text = _mSteamUserData.Data.HexId;
            }
            _mSteamUserData.onChanged.AddListener(HandlePersonaStateChanged);
        }

        private void OnDestroy()
        {
            _mSteamUserData.onChanged.RemoveListener(HandlePersonaStateChanged);
        }

        private void HandlePersonaStateChanged(UserData user, EPersonaChange flag)
        {
            if (_mSteamUserData.Data.IsValid)
            {
                if (Friends.Client.PersonaChangeHasFlag(flag, EPersonaChange.k_EPersonaChangeName))
                {
                    if (label != null)
                        label.text = _mSteamUserData.Data.HexId;
                }
            }
            else
            {
                if (label != null)
                    label.text = string.Empty;
            }
        }
    }
}
#endif