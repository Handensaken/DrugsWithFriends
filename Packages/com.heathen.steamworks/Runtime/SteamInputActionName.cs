#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Displays the name of the indicated action
    /// </summary>
    [ModularComponent(typeof(SteamInputActionData), "Names", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamInputActionData))]
    public class SteamInputActionName : MonoBehaviour
    {
        /// <summary>
        /// The label used to display the name of the action
        /// </summary>
        public TMPro.TextMeshProUGUI label;

        private SteamInputActionData _mInspector;

        private void Start()
        {
            _mInspector = GetComponent<SteamInputActionData>();
            SteamTools.Interface.WhenReady(HandleInitialization);
        }

        private void HandleInitialization()
        {
            RefreshName();
        }

        private void OnEnable()
        {
            RefreshName();
        }

        /// <summary>
        /// Refreshes the display name of the action based on the current controller and action state.
        /// </summary>
        public void RefreshName()
        {
            if (_mInspector != null && !string.IsNullOrEmpty(_mInspector.Action.Name) && label != null)
            {
                if (_mInspector.Set.Handle > 0)
                {
                    if (API.Input.Client.ConnectedControllers.Count > 0)
                    {
                        var names = _mInspector.Action.GetInputNames(API.Input.Client.ConnectedControllers[0], _mInspector.Set);
                        if (names.Length > 0)
                        {
                            label.text = names[0];
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(_mInspector.Layer.LayerName))
                {
                    if (API.Input.Client.ConnectedControllers.Count > 0)
                    {
                        var names = _mInspector.Action.GetInputNames(API.Input.Client.ConnectedControllers[0], _mInspector.Layer);
                        if (names.Length > 0)
                        {
                            label.text = names[0];
                        }
                    }
                }
            }
        }
    }
}
#endif