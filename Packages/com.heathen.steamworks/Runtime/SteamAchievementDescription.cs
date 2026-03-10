#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Displays the description of a Steam achievement.
    /// </summary>
    [ModularComponent(typeof(SteamAchievementData), "Descriptions", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAchievementData))]
    public class SteamAchievementDescription : MonoBehaviour
    {
        /// <summary>
        /// The text component to display the description in.
        /// </summary>
        public TMPro.TextMeshProUGUI label;
        private SteamAchievementData _mData;

        private void Awake()
        {
            _mData = GetComponent<SteamAchievementData>();
            SteamTools.Interface.WhenReady(Refresh);
        }

        private void OnDestroy()
        {
            SteamTools.Events.OnSteamInitialised -= Refresh;
        }

        /// <summary>
        /// Refreshes the display label with the achievement's description.
        /// </summary>
        public void Refresh()
        {
            if (!string.IsNullOrEmpty(_mData.apiName))
                label.text = API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(_mData.apiName, AchievementAttributes.Desc);

            SteamTools.Events.OnSteamInitialised -= Refresh;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for <see cref="SteamAchievementDescription"/>.
    /// </summary>
    [UnityEditor.CustomEditor(typeof(SteamAchievementDescription), true)]
    public class SteamAchievementDescriptionEditor : UnityEditor.Editor
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