#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the data of a Steam Game Server.
    /// </summary>
    [AddComponentMenu("Steamworks/Game Server")]
    [HelpURL("https://heathen.group/kb/steam-features-authentication/")]
    public class SteamGameServerData : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets the game server data.
        /// </summary>
        public GameServerData Data
        {
            get => _mData;
            set
            {
                _mData = value;
                if (_mEvents != null)
                    _mEvents.onChange?.Invoke();
            }
        }

        private GameServerData _mData;
        private SteamGameServerEvents _mEvents;

        private void Awake()
        {
            _mEvents = GetComponent<SteamGameServerEvents>();
        }
    }
#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for the <see cref="SteamGameServerData"/> component.
    /// </summary>
    [CustomEditor(typeof(SteamGameServerData), true)]
    public class SteamGameServerDataEditor : Editor
    {
        /// <summary>
        /// Renders the inspector GUI for the Steam Game Server Data component.
        /// </summary>
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