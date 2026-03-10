#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Invokes an event when a Steam achievement's status changes.
    /// </summary>
    [ModularEvents(typeof(SteamAchievementData))]
    [RequireComponent(typeof(SteamAchievementData))]
    public class SteamAchievementChanged : MonoBehaviour
    {
        /// <summary>
        /// Event invoked when the achievement's status changes.
        /// </summary>
        [EventField]
        public UnityEvent<bool> onChanged;
        private SteamAchievementData _mData;

        private void Awake()
        {
            _mData = GetComponent<SteamAchievementData>();
            API.StatsAndAchievements.Client.OnAchievementStatusChanged.AddListener(HandleChange);
        }

        private void OnDestroy()
        {
            API.StatsAndAchievements.Client.OnAchievementStatusChanged.RemoveListener(HandleChange);
        }
        private void HandleChange(string arg0, bool arg1)
        {
            if (arg0 == _mData.apiName)
                onChanged?.Invoke(arg1);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for <see cref="SteamAchievementChanged"/>.
    /// </summary>
    [UnityEditor.CustomEditor(typeof(SteamAchievementChanged), true)]
    public class SteamAchievementChangedEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Draws the inspector GUI.
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