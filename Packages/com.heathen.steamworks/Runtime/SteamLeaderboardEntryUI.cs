#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A simple implementation of the <see cref="ILeaderboardEntryDisplay"/> interface.
    /// </summary>
    [RequireComponent(typeof(SteamUserData))]
    [HelpURL("")]
    public class SteamLeaderboardEntryUI : MonoBehaviour, ILeaderboardEntryDisplay
    {
        /// <summary>
        /// Read or write the <see cref="LeaderboardEntry"/> for this object
        /// </summary>
        public LeaderboardEntry Entry
        {
            get => _entry;
            set => SetEntry(value);
        }

        [SerializeField]
        private TMPro.TextMeshProUGUI score;
        [SerializeField]
        private TMPro.TextMeshProUGUI rank;

        private SteamUserData _userData;
        private LeaderboardEntry _entry;

        private void Awake()
        {
            _userData = GetComponent<SteamUserData>();
        }

        private void SetEntry(LeaderboardEntry entry)
        {
            _userData.Data = entry.User;

            if (score != null)
                score.text = entry.Score.ToString();
            if (rank != null)
                rank.text = entry.Rank.ToString();

            _entry = entry;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLeaderboardEntryUI), true)]
    public class SteamLeaderboardEntryUIEditor : UnityEditor.Editor
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