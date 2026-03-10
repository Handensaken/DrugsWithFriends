#if !DISABLESTEAMWORKS && STEAM_INSTALLED
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Proxies events from a <see cref="SteamInputControllerData"/> component.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamInputControllerData))]
    public class SteamInputControllerDataEvents : MonoBehaviour
    {
        /// <summary>
        /// Occurs when the controller data changes.
        /// </summary>
        [EventField]
        public UnityEvent onChange;
        /// <summary>
        /// Occurs when the controller data is updated.
        /// </summary>
        [EventField]
        public ControllerDataEvent onUpdate;

        private SteamInputControllerData _inspector;

        private void Awake()
        {
            _inspector = GetComponent<SteamInputControllerData>();
            _inspector.onChanged?.AddListener(onChange.Invoke);
            SteamTools.Events.OnInputDataChanged += HandleEvent;
        }

        private void OnDestroy()
        {
            if (_inspector != null)
                _inspector.onChanged?.RemoveListener(onChange.Invoke);

            SteamTools.Events.OnInputDataChanged -= HandleEvent;
        }

        private void HandleEvent(InputControllerStateData state)
        {
            if(_inspector.Data.HasValue
                && state.handle == _inspector.Data.Value)
            {
                onUpdate?.Invoke(state);
            }
        }
    }
}
#endif