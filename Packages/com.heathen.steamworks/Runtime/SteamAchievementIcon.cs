#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Displays the icon of a Steam achievement.
    /// </summary>
    [ModularComponent(typeof(SteamAchievementData), "Icons", nameof(image))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAchievementData))]
    public class SteamAchievementIcon : MonoBehaviour
    {
        /// <summary>
        /// The raw image component to display the icon in.
        /// </summary>
        public UnityEngine.UI.RawImage image;
        private SteamAchievementData _data;

        private void Awake()
        {
            _data = GetComponent<SteamAchievementData>();
        }

        private void Start()
        {
            API.StatsAndAchievements.Client.OnAchievementStatusChanged.AddListener(HandleChange);
            SteamTools.Interface.WhenReady(Refresh);
        }

        private void OnDestroy()
        {
            API.StatsAndAchievements.Client.OnAchievementStatusChanged.RemoveListener(HandleChange);
        }

        private void HandleChange(string arg0, bool arg1)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (!string.IsNullOrEmpty(_data.apiName))
            {
                API.StatsAndAchievements.Client.GetAchievementIcon(_data.apiName, texture =>
                {
                    image.texture = texture;
                });
            }

            SteamTools.Events.OnSteamInitialised -= Refresh;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for <see cref="SteamAchievementIcon"/>.
    /// </summary>
    [UnityEditor.CustomEditor(typeof(SteamAchievementIcon), true)]
    public class SteamAchievementIconEditor : UnityEditor.Editor
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