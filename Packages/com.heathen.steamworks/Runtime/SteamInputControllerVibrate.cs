#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A component used to trigger vibration on a Steam Input controller.
    /// </summary>
    [RequireComponent(typeof(SteamInputControllerData))]
    public class SteamInputControllerVibrate : MonoBehaviour
    {
        /// <summary>
        /// The intensity of the vibration on the left motor, between 0 and 1.
        /// </summary>
        [SettingsField(0, false, "Vibrate")]
        [Range(0, 1)] 
        public float left = 0f;
        /// <summary>
        /// The intensity of the vibration on the right motor, between 0 and 1.
        /// </summary>
        [SettingsField(0, false, "Vibrate")]
        [Range(0, 1)]
        public float right = 0f;

        private SteamInputControllerData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamInputControllerData>();
        }

        private void Update()
        {
            if(_mInspector.Data.HasValue)
            API.Input.Client.TriggerVibration(_mInspector.Data.Value, (ushort)Mathf.Lerp(0, ushort.MaxValue, left), (ushort)Mathf.Lerp(0, ushort.MaxValue, right));
        }
    }
}
#endif