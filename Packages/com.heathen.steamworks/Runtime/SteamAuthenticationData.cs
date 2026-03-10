#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Holds and manages authentication data and events.
    /// </summary>
    [AddComponentMenu("Steamworks/Authentication")]
    [HelpURL("https://kb.heathen.group/steam/features/authentication")]
    public class SteamAuthenticationData : MonoBehaviour
    {
        /// <summary>
        /// Defines the types of authentication events that can be managed.
        /// </summary>
        public enum ManagedEvents
        {
            /// <summary>
            /// Occurs when the ticket data has changed.
            /// </summary>
            Changed,
            /// <summary>
            /// Occurs when a ticket request has failed.
            /// </summary>
            TicketRequestErred,
            /// <summary>
            /// Occurs when an RPC has been invoked.
            /// </summary>
            RPCInvoked,
            /// <summary>
            /// Occurs when an invalid ticket is received.
            /// </summary>
            InvalidTicketReceived,
            /// <summary>
            /// Occurs when an invalid session is requested.
            /// </summary>
            InvalidSessionRequested,
            /// <summary>
            /// Occurs when a session has started.
            /// </summary>
            SessionStarted,
        }

        /// <summary>
        /// Gets or sets the current authentication ticket.
        /// </summary>
        public AuthenticationTicket Data
        {
            get => _mData; 
            set
            {
                _mData = value;
                if(_mEvents != null) 
                    _mEvents.onChange?.Invoke(_mData);
            }
        }

        private AuthenticationTicket _mData;
        private SteamAuthenticationEvents _mEvents;
        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private System.Collections.Generic.List<ManagedEvents> mDelegates;

        private void Awake()
        {
            _mEvents = GetComponent<SteamAuthenticationEvents>();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for <see cref="SteamAuthenticationData"/>.
    /// </summary>
    [CustomEditor(typeof(SteamAuthenticationData), true)]
    public class SteamAuthenticationDataEditor : Editor
    {
        private SteamToolsSettings _settings;
        private static readonly string[] SettingsOptions =
        {
            "Sessions",
            "Get Ticket",
            "Send RPC",
            "General Events"
        };

        // ReSharper disable UnusedMember.Local
        [System.Flags]
        private enum SettingsMask
        {
            None = 0,
            Sessions = 1 << 0,
            GetTicket = 1 << 1,
            SendRpc = 1 << 2,
            GeneralEvents = 1 << 3
        }
        // ReSharper restore UnusedMember.Local

        private SettingsMask _settingsMask = SettingsMask.None;

        private void OnEnable()
        {
            _settings = SteamToolsSettings.GetOrCreate();
        }

        // ReSharper disable Unity.PerformanceCriticalCodeInvocation
        /// <summary>
        /// Draws the inspector GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // === Header links ===
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Player/Steamworks");
            if (EditorGUILayout.LinkButton("Portal"))
                if (_settings.ActiveApp != null)
                    Application.OpenURL("https://partner.steamgames.com/apps/landing/" +
                                        _settings.Get(_settings.ActiveApp.Value).applicationId.ToString());
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam/features/friends");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            
            DrawFunctionSettings();
            DrawFunctionEvents();
            serializedObject.ApplyModifiedProperties();
        }

        private static System.Type GetTypeForFeature(string featureName)
        {
            return featureName switch
            {
                // Settings
                "Sessions" => typeof(SteamAuthenticationSessions),
                "Get Ticket" => typeof(SteamAuthenticationGetTicket),
                "Send RPC" => typeof(SteamAuthenticationRpcInvoke),
                "General Events" => typeof(SteamAuthenticationEvents),
                _ => null,
            };
        }

        private System.Collections.Generic.IEnumerable<(SerializedProperty prop, string header)> GetPropertiesWithAttribute<T>(SerializedObject so)
            where T : PropertyAttribute
        {
            var prop = so.GetIterator();
            if (!prop.NextVisible(true)) yield break;
            while (prop.NextVisible(false))
            {
                var field = so.targetObject.GetType().GetField(prop.name,
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                if (field == null) continue;
                if (System.Attribute.GetCustomAttribute(field, typeof(T)) is SettingsFieldAttribute attr)
                    yield return (so.FindProperty(prop.name), attr.Header);
            }
        }

        private void DrawFunctionSettings()
        {
            var data = (SteamAuthenticationData)target;
            var go = data.gameObject;
            // Refresh mask
            _settingsMask = SettingsMask.None;
            for (int i = 0; i < SettingsOptions.Length; i++)
            {
                var type = GetTypeForFeature(SettingsOptions[i]);
                if (type != null && go.GetComponent(type))
                    _settingsMask |= (SettingsMask)(1 << i);
            }

            EditorGUI.BeginChangeCheck();
            _settingsMask = (SettingsMask)EditorGUILayout.EnumFlagsField("Settings", _settingsMask);
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < SettingsOptions.Length; i++)
                {
                    var type = GetTypeForFeature(SettingsOptions[i]);
                    if (type == null) continue;

                    bool hasComp = go.GetComponent(type);
                    bool shouldHave = (_settingsMask & (SettingsMask)(1 << i)) != 0;

                    if (shouldHave && !hasComp)
                        go.AddComponent(type).hideFlags = HideFlags.HideInInspector;
                    else if (!shouldHave && hasComp)
                        DestroyImmediate(go.GetComponent(type));
                }
            }
            EditorGUI.indentLevel++;
            // Draw configuration for active settings
            foreach (var featureName in SettingsOptions)
            {
                var type = GetTypeForFeature(featureName);
                var comp = go.GetComponent(type);
                if (!comp) continue;

                var so = new SerializedObject(comp);
                so.Update();

                var targetProperties = GetPropertiesWithAttribute<SettingsFieldAttribute>(so).ToList();
                if (targetProperties.Count > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    // ReSharper disable once UnusedVariable
                    foreach (var (prop, header) in GetPropertiesWithAttribute<SettingsFieldAttribute>(so))
                        EditorGUILayout.PropertyField(prop, new GUIContent(ObjectNames.NicifyVariableName(prop.name)));
                    EditorGUILayout.EndVertical();
                }

                so.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;
        }

        private void DrawFunctionEvents()
        {
            var data = (SteamAuthenticationData)target;
            var soData = serializedObject;
            var delegatesProp = soData.FindProperty("m_Delegates");

            if (delegatesProp == null)
                return;

            var dataEvents = data.GetComponent<SteamAuthenticationEvents>();

            if (!dataEvents)
                return;

            if (dataEvents)
                dataEvents.hideFlags = HideFlags.HideInInspector;

            var soEvents = dataEvents ? new SerializedObject(dataEvents) : null;

            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            var removeIndex = -1;

            for (var i = 0; i < delegatesProp.arraySize; i++)
            {
                var delegateProp = delegatesProp.GetArrayElementAtIndex(i);
                var evt = (SteamAuthenticationData.ManagedEvents)delegateProp.enumValueIndex;

                var label = new GUIContent(ObjectNames.NicifyVariableName(evt.ToString()));

                // --- Match enum to actual event SerializedProperty ---
                var propToDraw = evt switch
                {
                    // === SteamLobbyDataEvents ===
                    SteamAuthenticationData.ManagedEvents.Changed => soEvents?.FindProperty(
                        nameof(SteamAuthenticationEvents.onChange)),
                    SteamAuthenticationData.ManagedEvents.RPCInvoked => soEvents?.FindProperty(
                        nameof(SteamAuthenticationEvents.onRpcInvoke)),
                    SteamAuthenticationData.ManagedEvents.TicketRequestErred => soEvents?.FindProperty(
                        nameof(SteamAuthenticationEvents.onError)),
                    SteamAuthenticationData.ManagedEvents.InvalidTicketReceived => soEvents?.FindProperty(
                        nameof(SteamAuthenticationEvents.onInvalidTicket)),
                    SteamAuthenticationData.ManagedEvents.InvalidSessionRequested => soEvents?.FindProperty(
                        nameof(SteamAuthenticationEvents.onInvalidSession)),
                    SteamAuthenticationData.ManagedEvents.SessionStarted => soEvents?.FindProperty(
                        nameof(SteamAuthenticationEvents.onSessionStart)),
                    _ => null
                };

                if (propToDraw != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(propToDraw, label, true);

                    if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(25)))
                        removeIndex = i;

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
                else
                {
                    EditorGUILayout.HelpBox($"Event {evt} is not available on current components.", MessageType.Info);
                }
            }

            if (removeIndex >= 0)
                delegatesProp.DeleteArrayElementAtIndex(removeIndex);

            // === Add New Event Menu ===
            if (GUILayout.Button("Add New Event Type"))
            {
                var menu = new GenericMenu();

                foreach (SteamAuthenticationData.ManagedEvents evt in System.Enum.GetValues(typeof(SteamAuthenticationData.ManagedEvents)))
                {
                    var alreadyAdded = false;
                    for (var i = 0; i < delegatesProp.arraySize; i++)
                    {
                        if ((SteamAuthenticationData.ManagedEvents)delegatesProp.GetArrayElementAtIndex(i)
                                .enumValueIndex != evt) continue;
                        alreadyAdded = true;
                        break;
                    }

                    if (alreadyAdded)
                        menu.AddDisabledItem(new GUIContent(evt.ToString()));
                    else
                        menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(evt.ToString())), false, () =>
                        {
                            delegatesProp.arraySize++;
                            delegatesProp.GetArrayElementAtIndex(delegatesProp.arraySize - 1).enumValueIndex = (int)evt;
                            soData.ApplyModifiedProperties();
                        });
                }

                menu.ShowAsContext();
            }

            EditorGUI.indentLevel--;
            soData.ApplyModifiedProperties();
            soEvents?.ApplyModifiedProperties();
        }
    }
#endif
}
#endif