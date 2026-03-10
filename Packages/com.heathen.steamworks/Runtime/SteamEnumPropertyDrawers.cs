#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
using Steamworks;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A custom property drawer for <see cref="ERemoteStoragePublishedFileVisibility"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ERemoteStoragePublishedFileVisibility))]
    public class SteamVisibilityDrawer : PropertyDrawer
    {
        /// <summary>
        /// Draws the property GUI.
        /// </summary>
        /// <param name="position">The rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var names = System.Enum.GetNames(typeof(ERemoteStoragePublishedFileVisibility))
                .Select(n => n.Replace("k_ERemoteStoragePublishedFileVisibility", ""))
                .ToArray();

            var values = System.Enum.GetValues(typeof(ERemoteStoragePublishedFileVisibility))
                .Cast<int>()
                .ToArray();

            int index = System.Array.IndexOf(values, property.intValue);
            int newIndex = EditorGUI.Popup(position, label.text, index, names);

            if (newIndex >= 0 && newIndex < values.Length)
                property.intValue = values[newIndex];
        }
    }

    /// <summary>
    /// A custom property drawer for <see cref="EItemPreviewType"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(EItemPreviewType))]
    public class ItemPreviewTypeDrawer : PropertyDrawer
    {
        /// <summary>
        /// Draws the property GUI.
        /// </summary>
        /// <param name="position">The rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var names = System.Enum.GetNames(typeof(EItemPreviewType))
                .Select(n => n.Replace("k_EItemPreviewType_", ""))
                .ToArray();

            var values = System.Enum.GetValues(typeof(EItemPreviewType))
                .Cast<int>()
                .ToArray();

            int index = System.Array.IndexOf(values, property.intValue);
            int newIndex = EditorGUI.Popup(position, label.text, index, names);

            if (newIndex >= 0 && newIndex < values.Length)
                property.intValue = values[newIndex];
        }
    }

    /// <summary>
    /// A custom property drawer for <see cref="EResult"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(EResult))]
    public class EResultDrawer : PropertyDrawer
    {
        /// <summary>
        /// Draws the property GUI.
        /// </summary>
        /// <param name="position">The rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get enum names and remove Steam prefix
            var names = System.Enum.GetNames(typeof(EResult))
                .Select(n => n.Replace("k_EResult", ""))
                .ToArray();

            var values = System.Enum.GetValues(typeof(EResult))
                .Cast<int>()
                .ToArray();

            int index = System.Array.IndexOf(values, property.intValue);
            int newIndex = EditorGUI.Popup(position, label.text, index, names);

            if (newIndex >= 0 && newIndex < values.Length)
                property.intValue = values[newIndex];
        }
    }
}
#endif
#endif