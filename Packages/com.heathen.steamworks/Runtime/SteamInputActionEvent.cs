#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// An event-based component that listens for Steam Input action changes.
    /// </summary>
    [ModularEvents(typeof(SteamInputActionData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamInputActionData))]
    public class SteamInputActionEvent : MonoBehaviour
    {
        /// <summary>
        /// This event is triggered when the associated Steam Input action is changed.
        /// </summary>
        [EventField]
        public ActionUpdateEvent onChanged;

        private SteamInputActionData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamInputActionData>();
            SteamTools.Events.OnInputDataChanged += HandleEvent;
        }

        private void OnDestroy()
        {
            SteamTools.Events.OnInputDataChanged -= HandleEvent;
        }

        private void HandleEvent(InputControllerStateData controller)
        {
            var actionData = controller.changes.FirstOrDefault(p => p.name == _mInspector.Action.Name);
            if (actionData.name == _mInspector.Action.Name)
                onChanged.Invoke(actionData);
        }
    }
}
#endif