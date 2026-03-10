#if !DISABLESTEAMWORKS && STEAM_INSTALLED
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Initialise Steam")]
    [HelpURL("https://kb.heathen.group/steamworks/initialization/unity-initialization#component")]
    [DisallowMultipleComponent]
    public class InitializeSteamworks : MonoBehaviour
    {
        private void Start()
        {
            #if UNITY_EDITOR
                SteamTools.Interface.IsDebugging = true;
            #endif
            SteamTools.Interface.Initialise();
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(InitializeSteamworks), true)]
        public class InitializeSteamworksEditor : Editor
        {
            private SteamToolsSettings _settings;

            private void OnEnable()
            {
                _settings = SteamToolsSettings.GetOrCreate();
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.LinkButton("Settings"))
                    SettingsService.OpenProjectSettings("Project/Player/Steamworks");
                if (EditorGUILayout.LinkButton("Portal"))
                    Application.OpenURL("https://partner.steamgames.com/apps/landing/" + _settings.Get(_settings.ActiveApp.Value).applicationId);
                if (EditorGUILayout.LinkButton("Guide"))
                    Application.OpenURL("https://kb.heathen.group/steam");
                if (EditorGUILayout.LinkButton("Support"))
                    Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
                EditorGUILayout.EndHorizontal();
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
#endif