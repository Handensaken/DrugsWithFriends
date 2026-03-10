#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Displays the name of a Steam achievement.
    /// </summary>
    [ModularComponent(typeof(SteamAchievementData), "Names", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAchievementData))]
    public class SteamAchievementName : MonoBehaviour
    {
        /// <summary>
        /// The text component to display the name in.
        /// </summary>
        public TMPro.TextMeshProUGUI label;
        private SteamAchievementData _mData;

        private void Awake()
        {
            _mData = GetComponent<SteamAchievementData>();
            SteamTools.Interface.WhenReady(Refresh);
        }

        /// <summary>
        /// Refreshes the display label with the achievement's name.
        /// </summary>
        public void Refresh()
        {
            if (!string.IsNullOrEmpty(_mData.apiName))
                label.text = API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(_mData.apiName, AchievementAttributes.Name);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for <see cref="SteamAchievementName"/>.
    /// </summary>
    [UnityEditor.CustomEditor(typeof(SteamAchievementName), true)]
    public class SteamAchievementNameEditor : UnityEditor.Editor
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