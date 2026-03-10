#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/steam/features/lobby/unity-lobby/steam-lobby-input-validator")]
    [RequireComponent(typeof(TMPro.TMP_InputField))]
    public class SteamLobbyIdInputValidator : MonoBehaviour
    {
        [Header("Configuration")]
        public int minimalIdLength = 8;

        [FormerlySerializedAs("OnValid")] [Header("Events")]
        public UnityEvent onValid;

        private TMPro.TMP_InputField _mInputField;

        private void Awake()
        {
            _mInputField = GetComponent<TMPro.TMP_InputField>();
            _mInputField.onValueChanged.AddListener(HandleValueChanged);
        }

        private void OnDestroy()
        {
            _mInputField.onValueChanged.RemoveListener(HandleValueChanged);
        }

        private void HandleValueChanged(string arg0)
        {
            if (arg0.Length >= minimalIdLength)
            {
                var lobby = LobbyData.Get(arg0);
                if (lobby.IsValid)
                    onValid?.Invoke();
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyIdInputValidator), true)]
    public class SteamLobbyIdInputValidatorEditor : UnityEditor.Editor
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