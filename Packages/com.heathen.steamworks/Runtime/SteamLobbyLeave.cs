#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Leave", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyLeave : MonoBehaviour
    {
        private SteamLobbyData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLobbyData>();
        }

        public void Leave()
        {
            if (_mInspector.Data.IsValid)
            {
                _mInspector.Data.Leave();
                _mInspector.Data = CSteamID.Nil;
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyLeave), true)]
    public class SteamLobbyLeaveEditor : UnityEditor.Editor
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