#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Provides Lobby Metadata access to the Unity Inspector
    /// </summary>
    [ModularComponent(typeof(SteamLobbyData), "Metadata", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyMetadata : MonoBehaviour
    {
        /// <summary>
        /// Defines a key and string event used to track changes to lobby metadata
        /// </summary>
        [Serializable]
        public struct KeyEventMap
        {
            public string key;
            public UnityEvent<string> onUpdate;
            [HideInInspector]
            [NonSerialized]
            public string PreviousValue;
        }
        /// <summary>
        /// A collection of key and values to be set when the Set() function is called.
        /// </summary>
        [SettingsField(0, false, "Metadata")]
        [Tooltip("A collection of key and values to be set when the Set() function is called.")]
        public List<StringKeyValuePair> dataToSet = new();
        /// <summary>
        /// A collection of key and event, the events will be invoked when the key's data changes on the lobby.
        /// </summary>
        [SettingsField(0, false, "Metadata")]
        [Tooltip("A collection of key and event, the events will be invoked when the key's data changes on the lobby.")]
        public List<KeyEventMap> onChanged = new();

        private SteamLobbyData _inspector;

        private void Awake()
        {
            _inspector = GetComponent<SteamLobbyData>();
            _inspector.onChanged.AddListener(HandleOnChanged);
            
            SteamTools.Events.OnLobbyDataUpdate += HandleMetadataChange;
            RefreshKeyValues();
        }

        private void HandleMetadataChange(LobbyData lobby, LobbyMemberData? member)
        {
            if(lobby == _inspector.Data)
            {
                for (int i = 0; i < onChanged.Count; i++)
                {
                    var map = onChanged[i];
                    var currentValue = _inspector.Data[map.key];
                    if (currentValue != map.PreviousValue)
                    {
                        map.PreviousValue = currentValue;
                        onChanged[i] = map;
                        map.onUpdate?.Invoke(map.PreviousValue);
                    }
                }
            }
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            RefreshKeyValues();
        }

        /// <summary>
        /// Refresh the stored values for all On Changed keys.
        /// This will automatically be called when the lobby data is updated.
        /// </summary>
        public void RefreshKeyValues()
        {
            for (int i = 0; i < onChanged.Count; i++)
            {
                var map = onChanged[i];
                if (_inspector.Data.IsValid)
                    map.PreviousValue = _inspector.Data[map.key];
                else
                    map.PreviousValue = string.Empty;

                onChanged[i] = map;
                map.onUpdate?.Invoke(map.PreviousValue);
            }
        }

        /// <summary>
        /// Set the metadata on the lobby for all Data to Set entries
        /// This is not automatically called but can be connected to Steam Lobby Data Events On Create or similar to effectively call it on creation of a lobby.
        /// </summary>
        public void Set()
        {
            if (_inspector.Data.IsValid
                && dataToSet.Count > 0
                && _inspector.Data.IsOwner)
            {
                foreach (var kvp in dataToSet)
                {
                    API.Matchmaking.Client.SetLobbyData(_inspector.Data, kvp.key, kvp.value);
                }
            }
        }

        /// <summary>
        /// Set a key and value on the lobby metadata
        /// This will not modify the Data to Set collection.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            if (_inspector.Data.IsValid)
            {
                if (_inspector.Data.IsOwner)
                    API.Matchmaking.Client.SetLobbyData(_inspector.Data, key, value);
                else
                    Debug.LogWarning($"[{nameof(SteamLobbyMetadata)}] Only the owner can set data");
            }
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyMetadata)}] No lobby to set");
            }
        }

        /// <summary>
        /// Set a key and value on the lobby metadata
        /// This will not modify the Data to Set collection
        /// </summary>
        /// <param name="data"></param>
        public void Set(StringKeyValuePair data)
        {
            Set(data.key, data.value);
        }

        /// <summary>
        /// Get the value of a given metadata key
        /// </summary>
        /// <param name="key">The key of the data to read</param>
        /// <returns>The string value of the key</returns>
        public string Get(string key)
        {
            if (_inspector.Data.IsValid)
                return _inspector.Data[key];
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyMetadata)}] No lobby to read");
                return string.Empty;
            }
        }

        /// <summary>
        /// Does this lobby have a value for this key
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if it has a value and is not string empty, else false</returns>
        public bool HasKey(string key)
        {
            if (_inspector.Data.IsValid)
                return !string.IsNullOrEmpty(_inspector.Data[key]);
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyMetadata)}] No lobby to test");
                return false;
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyMetadata), true)]
    public class SteamLobbyMetadataEditor : UnityEditor.Editor
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