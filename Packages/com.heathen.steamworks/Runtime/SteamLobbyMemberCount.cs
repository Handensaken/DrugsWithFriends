#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Heathen.SteamworksIntegration.API;
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Member Count", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyMemberCount : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamLobbyData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLobbyData>();
            _mInspector.onChanged.AddListener(HandleOnChanged);
            
            SteamTools.Events.OnLobbyChatUpdate += HandleChatUpdate;
            if (_mInspector.Data.IsValid)
                label.text = _mInspector.Data.MemberCount.ToString();
        }

        private void OnDestroy()
        {
            _mInspector.onChanged.RemoveListener(HandleOnChanged);
            SteamTools.Events.OnLobbyChatUpdate -= HandleChatUpdate;
        }

        private void HandleChatUpdate(LobbyData lobby, UserData user, EChatMemberStateChange state)
        {
            if (lobby == _mInspector.Data)
                label.text = _mInspector.Data.MemberCount.ToString();
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            if (_mInspector.Data.IsValid)
                label.text = _mInspector.Data.MemberCount.ToString();
            else
                label.text = "0";
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyMemberCount), true)]
    public class SteamLobbyMemberCountEditor : UnityEditor.Editor
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