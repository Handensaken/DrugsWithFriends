#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Heathen.SteamworksIntegration.API;
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamUserData), "Hex Inputs", nameof(input))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserHexInput : MonoBehaviour
    {
        public TMPro.TMP_InputField input;

        private SteamUserData _mSteamUserData;

        private void Awake()
        {
            _mSteamUserData = GetComponent<SteamUserData>();
            if (_mSteamUserData.Data.IsValid)
            {
                input.text = _mSteamUserData.Data.HexId;
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
                    if (input != null)
                        input.text = _mSteamUserData.Data.HexId;
                }
            }
            else
            {
                if (input != null)
                    input.text = string.Empty;
            }
        }
    }
}
#endif