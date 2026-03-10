#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Heathen.SteamworksIntegration.API;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Names", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyName : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamLobbyData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLobbyData>();
            _mInspector.onChanged.AddListener(HandleOnChanged);
            
            SteamTools.Events.OnLobbyDataUpdate += HandleLobbyDataChange;
            if (_mInspector.Data.IsValid)
                label.text = _mInspector.Data.Name;
        }

        private void OnDestroy()
        {
            _mInspector.onChanged.RemoveListener(HandleOnChanged);
            SteamTools.Events.OnLobbyDataUpdate -= HandleLobbyDataChange;
        }

        private void HandleLobbyDataChange(LobbyData lobby, LobbyMemberData? member)
        {
            if (lobby == _mInspector.Data)
                label.text = _mInspector.Data.Name;
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            if (_mInspector.Data.IsValid)
                label.text = _mInspector.Data.Name;
            else
                label.text = string.Empty;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyName), true)]
    public class SteamLobbyNameEditor : UnityEditor.Editor
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