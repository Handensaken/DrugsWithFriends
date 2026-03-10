#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Steamworks;
using System;
using System.Collections.Generic;


namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Component used to manage the exchange of Steam Inventory items based on a defined recipe.
    /// </summary>
    [ModularComponent(typeof(SteamInventoryItemData), "Exchange", "")]
    [RequireComponent(typeof(SteamInventoryItemData))]
    [AddComponentMenu("")]
    public class SteamInventoryExchange : MonoBehaviour
    {
        /// <summary>
        /// Represents an entry in an exchange recipe.
        /// </summary>
        [Serializable]
        public struct RecipeEntry
        {
            /// <summary>
            /// The ID of the item required for the exchange.
            /// </summary>
            public int id;
            /// <summary>
            /// The number of items required for the exchange.
            /// </summary>
            public uint count;
        }

        /// <summary>
        /// The list of items required to perform the exchange.
        /// </summary>
        [SettingsField]
        public List<RecipeEntry> recipe = new();

        /// <summary>
        /// Gets a value indicating whether the exchange can currently be performed based on the user's inventory.
        /// </summary>
        public bool IsCanExchange
        {
            get
            {
                if (recipe.Count == 0)
                {
                    return false;
                }

                bool allPass = true;
                foreach (var entry in recipe)
                {
                    ItemData item = entry.id;
                    if (!item.GetExchangeEntry(entry.count, out var _))
                    {
                        allPass = false;
                        break;
                    }
                }

                return allPass;
            }
        }

        private SteamInventoryItemData _mInspector;
        private SteamInventoryItemDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamInventoryItemData>();
            _mEvents = GetComponent<SteamInventoryItemDataEvents>();

            SteamTools.Events.OnInventoryResultReady += HandleInventoryUpdated;

            SteamTools.Interface.WhenReady(RefreshCanExchange);
        }

        private void OnDestroy()
        {
            SteamTools.Events.OnInventoryResultReady -= HandleInventoryUpdated;
        }

        private void HandleInventoryUpdated(InventoryResult _)
        {
            RefreshCanExchange();
        }

        /// <summary>
        /// Refreshes the state of whether an exchange can be performed and invokes the associated event.
        /// </summary>
        public void RefreshCanExchange()
        {
            if (_mEvents != null)
                _mEvents.onCanExchangeChange?.Invoke(IsCanExchange);
        }

        /// <summary>
        /// Attempts to perform the exchange using the items in the recipe.
        /// </summary>
        public void Exchange()
        {
            List<ExchangeEntry> exchangeReagents = new();
            bool allPass = true;
            foreach (var entry in recipe)
            {
                ItemData item = entry.id;
                if (item.GetExchangeEntry(entry.count, out var reagents))
                {
                    exchangeReagents.AddRange(reagents);
                }
                else
                {
                    allPass = false;
                    break;
                }
            }

            if (allPass)
                _mInspector.Data.Exchange(exchangeReagents, result =>
                {
                    if (result.result == EResult.k_EResultOK)
                    {
                        if (_mEvents != null)
                            _mEvents.onExchangeComplete?.Invoke(result.items);
                    }
                    else if (_mEvents != null)
                        _mEvents.onExchangeFailed?.Invoke(result.result);
                });
            else if (_mEvents != null)
                _mEvents.onExchangeRejected?.Invoke();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom property drawer for <see cref="SteamInventoryExchange.RecipeEntry"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(SteamInventoryExchange.RecipeEntry))]
    public class RecipeEntryDrawer : PropertyDrawer
    {
        private string[] _options;
        private int[] _ids;
        private int _selectedIndex;
        private SteamToolsSettings _settings;
        private bool _initialized;

        private void Init(SerializedProperty property)
        {
            if (_initialized) return;
            _initialized = true;

            _settings = SteamToolsSettings.GetOrCreate();
            var list = _settings && _settings.items != null
                ? _settings.items
                : new List<SteamToolsSettings.NameAndID>();

            if (list.Count > 0)
            {
                _options = new string[list.Count];
                _ids = new int[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    _options[i] = list[i].name;
                    _ids[i] = list[i].id;
                }

                var idProp = property.FindPropertyRelative("id");
                int current = idProp.intValue;
                _selectedIndex = Mathf.Max(0, Array.IndexOf(_ids, current));
                if (_selectedIndex < 0)
                    _selectedIndex = 0;
            }
            else
            {
                _options = new string[] { "No Items Configured" };
                _ids = new int[] { 0 };
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            var idProp = property.FindPropertyRelative("id");
            var countProp = property.FindPropertyRelative("count");

            EditorGUI.BeginProperty(position, label, property);

            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rect, label);

            EditorGUI.indentLevel++;
            rect.y += EditorGUIUtility.singleLineHeight + 2;

            // Item dropdown
            EditorGUI.BeginChangeCheck();
            _selectedIndex = EditorGUI.Popup(rect, "Item", _selectedIndex, _options);
            if (EditorGUI.EndChangeCheck() && _selectedIndex >= 0 && _selectedIndex < _ids.Length)
                idProp.intValue = _ids[_selectedIndex];

            // Count field
            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, countProp);

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 3 lines: label + dropdown + count
            return EditorGUIUtility.singleLineHeight * 3 + 6;
        }
    }
#endif
}
#endif