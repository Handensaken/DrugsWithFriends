#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using Steamworks;
using System;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a component that holds data for a specific Steam Inventory item.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Steamworks/Inventory Item")]
    [HelpURL("https://heathen.group/kb/inventory/")]
    public class SteamInventoryItemData : MonoBehaviour
    {
        /// <summary>
        /// The unique identifier for the Steam Inventory item.
        /// </summary>
        public int id;
        /// <summary>
        /// Gets or sets the item data associated with this component.
        /// </summary>
        public ItemData Data
        {
            get => id;
            set
            {
                id = value.id;
                if (_mEvents != null)
                    _mEvents.onChange?.Invoke();
            }
        }

        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates;
        private SteamInventoryItemDataEvents _mEvents;

        private void Awake()
        {
            _mEvents = GetComponent<SteamInventoryItemDataEvents>();
        }

        /// <summary>
        /// Consumes a single instance of this item from the user's inventory.
        /// </summary>
        public void ConsumeOne()
        {
            var requestResult = Data.Consume(result =>
            {
                if (result.result != EResult.k_EResultOK)
                {
                    if (_mEvents != null)
                        _mEvents.onConsumeRequestFailed?.Invoke(result.result);
                }
                else if (_mEvents != null)
                    _mEvents.onConsumeRequestComplete?.Invoke(result.items);
            });
            if (!requestResult && _mEvents != null)
                _mEvents.onConsumeRequestRejected?.Invoke();
        }

        /// <summary>
        /// Consumes a specified quantity of this item from the user's inventory.
        /// </summary>
        /// <param name="quantity">The number of items to consume.</param>
        public void ConsumeMany(int quantity)
        {
            var requestResult = Data.Consume(Convert.ToUInt32(quantity), result =>
            {
                if (result.result != EResult.k_EResultOK)
                {
                    if (_mEvents != null)
                        _mEvents.onConsumeRequestFailed?.Invoke(result.result);
                }
                else if (_mEvents != null)
                    _mEvents.onConsumeRequestComplete?.Invoke(result.items);
            });

            if (!requestResult && _mEvents != null)
                _mEvents.onConsumeRequestRejected?.Invoke();
        }

        /// <summary>
        /// Attempts to add a promotional item to the user's inventory.
        /// </summary>
        public void AddPromo()
        {
            var requestResult = Data.AddPromoItem(HandleAddPromoResults);
            if(!requestResult && _mEvents != null)
                _mEvents.onAddPromoRejected?.Invoke();
        }

        /// <summary>
        /// Requests a refresh of all inventory items for the current user.
        /// </summary>
        public void GetAll()
        {
            API.Inventory.Client.GetAllItems();
        }

        /// <summary>
        /// Initiates the purchase process for this item.
        /// </summary>
        public void StartPurchase()
        {
            Data.StartPurchase((result, ioError) =>
            {
                if (!ioError && result.m_result == EResult.k_EResultOK)
                    _mEvents.onPurchaseStarted?.Invoke(result);
                else
                    _mEvents.onPurchaseStartFailed?.Invoke(result.m_result);
            });
        }

        private void HandleAddPromoResults(InventoryResult results)
        {
            if (results.result != EResult.k_EResultOK)
            {
                if (_mEvents != null)
                    _mEvents.onAddPromoFailed?.Invoke(results.result);
            }
            else if (_mEvents != null)
                _mEvents.onAddPromoComplete?.Invoke(results.items);
        }
    }
#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for <see cref="SteamInventoryItemData"/>.
    /// </summary>
    [CustomEditor(typeof(SteamInventoryItemData), true)]
    public class SteamInventoryItemDataEditor : ModularEditor
    {
        private string[] _options;
        private int[] _ids;
        private SteamToolsSettings.NameAndID[] _nameAndIds;
        private int _selectedIndex;
        private SerializedProperty _idProp;
        private SteamToolsSettings _settings;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new[]
        {            
            typeof(SteamInventoryItemQuantity),
            typeof(SteamInventoryItemName),
            typeof(SteamInventoryItemCurrentPrice),
            typeof(SteamInventoryItemBasePrice),
            typeof(SteamInventoryExchange),
            typeof(SteamInventoryItemDataEvents),
        };

        private void OnEnable()
        {
            _idProp = serializedObject.FindProperty("id");
            RefreshOptions();
        }

        private void RefreshOptions()
        {
            _settings = SteamToolsSettings.GetOrCreate();
            var list = _settings != null && _settings.items != null
                ? _settings.items
                : new List<SteamToolsSettings.NameAndID>();

            if (list.Count > 0)
            {
                _nameAndIds = list.ToArray();
                _options = new string[list.Count];
                _ids = new int[list.Count];

                for(int i = 0; i < list.Count; i++)
                {
                    _options[i] = _nameAndIds[i].name;
                    _ids[i] = _nameAndIds[i].id;
                }

                var current = _idProp.intValue;
                _selectedIndex = Mathf.Max(0, Array.IndexOf(_ids, current));
                if (_selectedIndex < 0)
                    _selectedIndex = 0;
            }
            else
            {
                _options = null;
            }
        }

        /// <summary>
        /// Draws the inspector GUI for the component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (_settings)
                _settings = SteamToolsSettings.GetOrCreate();

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Player/Steamworks");
            if (EditorGUILayout.LinkButton("Portal"))
                if (_settings.ActiveApp != null)
                    Application.OpenURL("https://partner.steamgames.com/apps/landing/" +
                                        _settings.Get(_settings.ActiveApp.Value).applicationId.ToString());
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam/features/inventory");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // === Achievement dropdown ===
            if (_options == null || _options.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No items found!.\n\n" +
                    "Open Project Settings > Player > Steamworks to configure your inventory items.",
                    MessageType.Warning
                );

                serializedObject.ApplyModifiedProperties();
                return;
            }

            _selectedIndex = EditorGUILayout.Popup(_selectedIndex, _options);
            if (_selectedIndex >= 0 && _selectedIndex < _options.Length)
                _idProp.intValue = _ids[_selectedIndex];

            EditorGUILayout.Space();

            // --- Features Dropdown ---
            HideAllAllowedComponents();
            DrawAddFieldDropdown();

            // --- Draw existing components via attributes ---
            EditorGUI.indentLevel++;
            DrawModularComponents();
            EditorGUI.indentLevel--;

            // --- Draw Functions as Flags (single-instance components) ---
            DrawFunctionFlags();

            // --- Draw Settings / Elements / Templates / Events ---
            DrawFields<SettingsFieldAttribute>("Settings");
            DrawFields<ElementFieldAttribute>("Elements");
            DrawFields<TemplateFieldAttribute>("Templates");
            DrawEventFields();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif