#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Manages Steam Input integration, including controller state updates and event dispatching.
    /// </summary>
    [HelpURL("")]
    public class SteamInputManager : MonoBehaviour
    {
        /// <summary>
        /// A reference to the current instance of the <see cref="SteamInputManager"/>.
        /// </summary>
        public static SteamInputManager Current;

        [Tooltip("If set to true then we will attempt to force Steam to use input for this app on start.\nThis is generally only needed in editor testing.")]
        [SerializeField]
        private bool forceInput = true;
        /// <summary>
        /// If set to true, the system will update every input action every frame for every controller found.
        /// </summary>
        [Tooltip("If set to true the system will update every input action every frame for every controller found")]
        public bool autoUpdate = true;
        /// <summary>
        /// Occurs when the input data for any controller has changed.
        /// </summary>
        [Header("Events")]
        public ControllerDataEvent onInputDataChanged;

        private bool _lastAutoUpdate;

        /// <summary>
        /// Gets or sets whether the system should automatically update input actions every frame.
        /// </summary>
        public static bool AutoUpdate
        {
            get => Current && Current.autoUpdate;
            set 
            { 
                if(Current != null)
                    Current.autoUpdate = value;
            }
        }

        /// <summary>
        /// A list of currently connected and tracked Steam Input controllers.
        /// </summary>
        public static List<InputControllerStateData> Controllers { get; private set; } = new List<InputControllerStateData>();

        private void Start()
        {
            Current = this;

            SteamTools.Events.OnInputDataChanged += onInputDataChanged.Invoke;
            API.Input.Client.IsAutoRefreshControllerState = autoUpdate;
            _lastAutoUpdate = autoUpdate;

            if (!SteamTools.Interface.IsReady)
                SteamTools.Interface.OnReady += HandleInitialization;
            else
                HandleInitialization();
        }

        private void HandleInitialization()
        {
            SteamTools.Interface.OnReady -= HandleInitialization;

            if (forceInput)
            {
                Application.OpenURL($"steam://forceinputappid/{API.App.Id}");
            }
        }

        private void OnDestroy()
        {
            if(Current == this)
                Current = null;

            SteamTools.Events.OnInputDataChanged -= onInputDataChanged.Invoke;

            if (forceInput)
                Application.OpenURL("steam://forceinputappid/0");
        }

        private void LateUpdate()
        {
            if (!SteamTools.Interface.IsReady)
                return;

            // Sync Unity inspector toggle → API
            if (API.Input.Client.IsAutoRefreshControllerState != autoUpdate)
                API.Input.Client.IsAutoRefreshControllerState = autoUpdate;

            // Optionally mirror API → manager (if other code changed it)
            if (_lastAutoUpdate != API.Input.Client.IsAutoRefreshControllerState)
            {
                _lastAutoUpdate = API.Input.Client.IsAutoRefreshControllerState;
                autoUpdate = _lastAutoUpdate;
            }

            if (autoUpdate)
                API.Input.Client.RunFrame();
        }

        /// <summary>
        /// Manually refreshes the Steam Input state.
        /// </summary>
        public void Refresh() => API.Input.Client.RunFrame();
    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamInputManager), true)]
    public class SteamInputManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif