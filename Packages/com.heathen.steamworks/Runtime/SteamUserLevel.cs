#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Heathen.SteamworksIntegration.API;
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamUserData), "Levels", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserLevel : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamUserData _mSteamUserData;

        private void Awake()
        {
            _mSteamUserData = GetComponent<SteamUserData>();
            if (_mSteamUserData.Data.IsValid && label != null)
            {
                label.text = _mSteamUserData.Data.Level.ToString();
            }
            _mSteamUserData.onChanged.AddListener(HandlePersonaStateChanged);
        }

        private void OnDestroy()
        {
            _mSteamUserData.onChanged.RemoveListener(HandlePersonaStateChanged);
        }

        private void HandlePersonaStateChanged(UserData user, EPersonaChange flag)
        {
            if (_mSteamUserData.Data.IsValid && label != null && Friends.Client.PersonaChangeHasFlag(flag, EPersonaChange.k_EPersonaChangeSteamLevel))
            {
                label.text = _mSteamUserData.Data.Level.ToString();
            }
        }
    }
}
#endif