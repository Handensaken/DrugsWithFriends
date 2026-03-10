#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public class InventorySettings
    {
        public Currency.Code LocalCurrencyCode => API.Inventory.Client.LocalCurrencyCode;
        public string LocalCurrencySymbol => API.Inventory.Client.LocalCurrencySymbol;
        public List<ItemDefinitionSettings> items = new();

        public InventoryChangedEvent EventChanged
        {
            get
            {
                _eventChanged ??= new InventoryChangedEvent();

                return _eventChanged;
            }
        }
        private InventoryChangedEvent _eventChanged = new InventoryChangedEvent();

#if UNITY_EDITOR
        public void UpdateItemDefinitions(bool debugLog)
        {
            if (debugLog)
                Debug.Log("Update Item Definitions: Start");

            if (API.Inventory.Client.GetItemDefinitionIDs(out SteamItemDef_t[] results))
            {
                if (debugLog)
                    Debug.Log($"Get Item Definition IDs found {results.Length} entries.");

                for (int i = 0; i < results.Length; i++)
                {
                    if (debugLog)
                        Debug.Log($"Processing Index {i}, ID = {results[i].m_SteamItemDef}");

                    try
                    {
                        var itemDefId = results[i];
                        var target = items.FirstOrDefault(p => p.id == itemDefId.m_SteamItemDef);
                        var created = false;
                        if (target == null)
                        {
                            if (debugLog)
                                Debug.Log($"New Item Definition Object required");
                            created = true;
                            target = new();
                        }
                        else if (debugLog)
                            Debug.Log($"Item Definition Object {target.itemName.GetSimpleValue()} found");

                        target.id = itemDefId.m_SteamItemDef;

                        target.itemName.Populate(itemDefId);

                        target.itemDescription.Populate(itemDefId);
                        target.itemDisplayType.Populate(itemDefId);

                        var type = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "type");
                        switch (type)
                        {
                            case "item":
                                target.itemType = InventoryItemType.Item;
                                break;
                            case "bundle":
                                target.itemType = InventoryItemType.Bundle;
                                break;
                            case "generator":
                                target.itemType = InventoryItemType.Generator;
                                break;
                            case "playtimegenerator":
                                target.itemType = InventoryItemType.Playtimegenerator;
                                break;
                            case "tag_generator":
                                target.itemType = InventoryItemType.TagGenerator;
                                break;
                            default:
                                Debug.LogWarning("Unknown Item Type: " + target.itemName.GetSimpleValue());
                                break;
                        }

                        var bgColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "background_color");
                        if (!string.IsNullOrEmpty(bgColor))
                        {
                            if (ColorUtility.TryParseHtmlString(bgColor, out Color color))
                                target.itemBackgroundColour.color = color;
                            else
                                target.itemBackgroundColour.use = false;
                        }
                        else
                            target.itemBackgroundColour.use = false;

                        var nColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "name_color");
                        if (!string.IsNullOrEmpty(nColor))
                        {
                            if (ColorUtility.TryParseHtmlString(nColor, out Color color))
                                target.itemNameColour.color = color;
                            else
                                target.itemNameColour.use = false;
                        }
                        else
                            target.itemNameColour.use = false;

                        target.itemIconURL = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url");
                        target.itemIconURLLarge = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url_large");

                        var returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "marketable");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool marketable))
                            target.itemMarketable = marketable;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tradable");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool tradable))
                            target.itemTradable = tradable;

                        if (created)
                        {
                            if (debugLog)
                                Debug.Log($"Adding new Item Definition Object: {target.itemName.GetSimpleValue()}, to Steam Settings");
                            items.Add(target);
                        }
                        else if(debugLog)
                                Debug.Log($"Updating Item Definition Object: {target.itemName.GetSimpleValue()}");

                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to parse item definition load from Valve: " + ex.Message);
                    }
                }

                API.Inventory.Client.GetAllItems(HandleItemResults);
            }
        }

        public void HandleSettingsInventoryDefinitionUpdate()
        {
            Debug.Log("Processing inventory item definition update!");
            UpdateItemDefinitions(true);
            SteamTools.Events.OnInventoryDefinitionUpdate -= HandleSettingsInventoryDefinitionUpdate;
        }
#endif

        private void HandleItemResults(InventoryResult results)
        {
            //Create the existing record
            Dictionary<ItemDefinitionSettings, List<ItemDetail>> currentState = new Dictionary<ItemDefinitionSettings, List<ItemDetail>>();
            Dictionary<ItemDefinitionSettings, List<ItemDetail>> newState = new Dictionary<ItemDefinitionSettings, List<ItemDetail>>();

            foreach (var item in items)
            {
                currentState.Add(item, new List<ItemDetail>(item.Details.ToArray()));
            }

            foreach (var detail in results.items)
            {
                var def = items.FirstOrDefault(p => p.id == detail.Definition.id);
                if (def != null)
                {
                    var itemDetails = def.Details;
                    itemDetails.RemoveAll(p => p.ItemId == detail.ItemId);
                    itemDetails.Add(detail);
                }
            }

            foreach (var item in items)
            {
                newState.Add(item, new List<ItemDetail>(item.Details.ToArray()));
            }

            //Compare
            List<ItemChangeRecord> changes = new List<ItemChangeRecord>();

            foreach (var kvp in currentState)
            {
                var item = kvp.Key;
                var before = kvp.Value;
                var after = newState[item];

                //Was in before is not in after
                var removed = before.Where(b => after.All(a => a.ItemId != b.ItemId));

                //Is in after was not in before
                var added = after.Where(a => before.All(b => b.ItemId != a.ItemId));

                //Is in both but count doesn't match
                var bChange = before.Where(b => after.Any(a => a.ItemId == b.ItemId) && after.FirstOrDefault(a => a.ItemId == b.ItemId).Quantity != b.Quantity);
                var aChange = after.Where(a => before.Any(b => b.ItemId == a.ItemId) && before.FirstOrDefault(b => b.ItemId == a.ItemId).Quantity != a.Quantity);

                var itemDetails = removed as ItemDetail[] ?? removed.ToArray();
                var enumerable = added as ItemDetail[] ?? added.ToArray();
                var details = bChange as ItemDetail[] ?? bChange.ToArray();
                if (itemDetails.Any()
                    || enumerable.Any()
                    || details.Any())
                {
                    var change = aChange as ItemDetail[] ?? aChange.ToArray();
                    //We have some change so record it
                    List<ItemInstanceChangeRecord> changeRecords = new List<ItemInstanceChangeRecord>();

                    foreach (var r in itemDetails)
                    {
                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = false,
                            changed = false,
                            removed = true,
                            quantityBefore = r.Quantity,
                            quantityAfter = 0,
                            instance = r.ItemId
                        });
                    }

                    foreach (var r in enumerable)
                    {
                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = true,
                            changed = false,
                            removed = false,
                            quantityBefore = 0,
                            quantityAfter = r.Quantity,
                            instance = r.ItemId
                        });
                    }

                    foreach (var r in details)
                    {
                        var match = change.FirstOrDefault(a => a.ItemId == r.ItemId);

                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = false,
                            changed = true,
                            removed = false,
                            quantityBefore = r.Quantity,
                            quantityAfter = match.Quantity,
                            instance = match.ItemId
                        });
                    }

                    var iChanged = new ItemChangeRecord
                    {
                        item = item,
                        changes = changeRecords.ToArray()
                    };

                    changes.Add(iChanged);

                    try
                    {
                        iChanged.item.EventChanged.Invoke(iChanged);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            if (changes.Count > 0)
            {
                EventChanged.Invoke(new InventoryChangeRecord
                {
                    changes = changes.ToArray()
                });
            }
        }
    }
}
#endif