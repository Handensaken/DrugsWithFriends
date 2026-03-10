#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Inventory item definition
    /// </summary>
    [HelpURL("https://kb.heathen.group/steamworks/features/inventory")]
    [Serializable]
    public class ItemDefinitionSettings
    {
        /// <summary>
        /// The ID of the item
        /// </summary>
        [SerializeField]
        public int id;
        /// <summary>
        /// The type of the item
        /// </summary>
        [FormerlySerializedAs("item_type")] [SerializeField]
        public InventoryItemType itemType;
        
        /// <summary>
        /// The name of the item
        /// </summary>
        [FormerlySerializedAs("item_name")] [SerializeField]
        public LanguageVariantNode itemName = new LanguageVariantNode { node = "name" };
        /// <summary>
        /// The description of the item
        /// </summary>
        [FormerlySerializedAs("item_description")] [SerializeField]
        public LanguageVariantNode itemDescription = new LanguageVariantNode { node = "description" };
        /// <summary>
        /// The display type for the item
        /// </summary>
        [FormerlySerializedAs("item_display_type")] [SerializeField]
        public LanguageVariantNode itemDisplayType = new LanguageVariantNode { node = "display_type" };

        /// <summary>
        /// The items background colour
        /// </summary>
        [FormerlySerializedAs("itemBackgroundColor")] [FormerlySerializedAs("item_background_color")] [SerializeField]
        public Colour itemBackgroundColour;
        /// <summary>
        /// The item's name's colour
        /// </summary>
        [FormerlySerializedAs("itemNameColor")] [FormerlySerializedAs("item_name_color")] [SerializeField]
        public Colour itemNameColour;
        /// <summary>
        /// A URL for the items main icon
        /// </summary>
        [FormerlySerializedAs("item_icon_url")] [SerializeField]
        public string itemIconURL;
        /// <summary>
        /// A large variant of the items icon
        /// </summary>
        [FormerlySerializedAs("item_icon_url_large")] [SerializeField]
        public string itemIconURLLarge;

        /// <summary>
        /// Is the item marketable
        /// </summary>
        [FormerlySerializedAs("item_marketable")] [SerializeField]
        public bool itemMarketable;
        /// <summary>
        /// Is the item tradable among players
        /// </summary>
        [FormerlySerializedAs("item_tradable")] [SerializeField]
        public bool itemTradable;

        /// <summary>
        /// Tags for use in the item store
        /// </summary>
        [FormerlySerializedAs("item_store_tags")] [SerializeField]
        public List<string> itemStoreTags = new();
        /// <summary>
        /// Additional item store images
        /// </summary>
        [FormerlySerializedAs("item_store_images")] [SerializeField]
        public List<string> itemStoreImages = new();

        /// <summary>
        /// The items internal <see cref="ItemData"/>
        /// </summary>
        public ItemData Data
        { 
            get => id;
            set => id = value;
        }
        /// <summary>
        /// The details related to this item if any
        /// </summary>
        public List<ItemDetail> Details => Data.GetDetails();
        /// <summary>
        /// The total quantity of this item the local user owns
        /// </summary>
        public long TotalQuantity => Data.GetTotalQuantity();
        /// <summary>
        /// This is only useful after calling API.Inventory.Client.LoadItemDefinitions.
        /// This will return the localised name of the item if definitions have been loaded.
        /// </summary>
        /// <remarks>
        /// In general, you shouldn't need to use this as you know what items your game has at development time.
        /// It is more performant to hard code the items name than to load it at run time so unless you support adding/changing items without deploying a patch this should never be needed.
        /// </remarks>
        public string DisplayName => Data.Name;
        /// <summary>
        /// Does this item have a valid price
        /// </summary>
        public bool HasPrice => Data.HasPrice;
        /// <summary>
        /// The currency code related to the user
        /// </summary>
        public Currency.Code CurrencyCode => ItemData.CurrencyCode;
        /// <summary>
        /// The currency symbol seen by the user
        /// </summary>
        public string CurrencySymbol => ItemData.CurrencySymbol;
        /// <summary>
        /// The current price of the item as seen by the user, this is base 100 e.g. 150 is $1.50
        /// </summary>
        public ulong CurrentPrice => Data.CurrentPrice;
        /// <summary>
        /// The base price of the item without any applicable discounts, this is base 100 e.g. 150 is $1.50
        /// </summary>
        public ulong BasePrice => Data.BasePrice;
        /// <summary>
        /// An event that is raised when the item experiences a change
        /// </summary>
        public ItemChangedEvent EventChanged
        {
            get
            {
                _eventChanged ??= new ItemChangedEvent();

                return _eventChanged;
            }
        }
        private ItemChangedEvent _eventChanged = new ItemChangedEvent();

        #region Internals
        /// <summary>
        /// An internal class which represents the language variants possible in string fields like name and description
        /// </summary>
        [Serializable]
        public class LanguageVariant
        {
            public LanguageCodes language;
            public string value;
        }
        /// <summary>
        /// A collection of <see cref="LanguageVariant"/> entries
        /// </summary>
        [Serializable]
        public class LanguageVariantNode
        {
            [HideInInspector]
            public string node;
            public string value;

            public List<LanguageVariant> variants = new();

            public string GetSimpleValue()
            {
                if (!string.IsNullOrEmpty(value))
                    return value;
                else if (variants.Count > 0)
                    return variants[0].value;
                else
                    return string.Empty;
            }

            public override string ToString()
            {
                if (variants.Count == 0)
                    return "\t\t\"" + node.Trim() + "\": \"" + value + "\"";
                else
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var variant in variants)
                    {
                        if (sb.Length > 0)
                            sb.Append(",\n");
                        sb.Append("\t\t\"" + node.Trim() + "_" + variant.language.ToString() + "\": \"" + variant.value + "\"");
                    }

                    return sb.ToString();
                }
            }

            public void Populate(SteamItemDef_t itemDefId)
            {
                value = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, node);

                if (variants == null)
                    variants = new List<LanguageVariant>();
                else
                    variants.Clear();

                var languages = Enum.GetNames(typeof(LanguageCodes));
                for (int i = 0; i < languages.Length; i++)
                {
                    var r = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, node + "_" + languages[i]);
                    if (!string.IsNullOrEmpty(r))
                    {
                        variants.Add(new LanguageVariant
                        {
                            language = (LanguageCodes)i,
                            value = r,
                        });
                    }
                }
            }
        }
        /// <summary>
        /// An internal class which represents the bundle information of an item
        /// </summary>
        [Serializable]
        public class Bundle
        {
            [Serializable]
            public struct Entry
            {
                [SerializeReference]
                public ItemDefinitionSettings item;
                public int count;

                public bool Valid
                {
                    get
                    {
                        if (count < 0)
                            return false;
                        if (item == null)
                            return false;
                        if (item.itemType == InventoryItemType.TagGenerator)
                            return false;

                        return true;
                    }
                }

                public override string ToString()
                {
                    if (!Valid)
                        return string.Empty;
                    else
                    {
                        if (count > 0)
                            return item.id.ToString() + "x" + count.ToString();
                        else
                            return item.id.ToString();
                    }
                }
            }

            public List<Entry> entries = new List<Entry>();

            public bool Valid
            {
                get
                {
                    if (entries.Count < 1)
                        return false;

                    return entries.All(p => p.Valid);
                }
            }

            public override string ToString()
            {
                if (!Valid)
                    return string.Empty;
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var t in entries)
                    {
                        if (sb.Length > 0)
                            sb.Append(";");

                        sb.Append(t.ToString());
                    }

                    return sb.ToString();
                }
            }
        }
        /// <summary>
        /// An internal class which represents the promotion rule structure of an item
        /// </summary>
        [Serializable]
        public class PromoRule
        {
            [Serializable]
            public struct PlayedEntry
            {
                public AppId_t app;
                public uint minutes;
            }

            public List<AppId_t> owns = new List<AppId_t>();
            public List<string> achievements = new List<string>();
            public List<PlayedEntry> played = new List<PlayedEntry>();
            public bool manual;

            public bool Valid
            {
                get
                {
                    if (owns.Count < 1
                        && achievements.Count < 1
                        && played.Count < 1
                        && !manual)
                        return false;
                    else
                        return true;
                }
            }

            public override string ToString()
            {
                if (!Valid)
                    return string.Empty;

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < owns.Count; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append("owns:" + owns[i].m_AppId.ToString());
                }

                foreach (var t in achievements)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append("ach:" + t);
                }

                for (int i = 0; i < played.Count; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append("played:" + played[i].app.ToString() + "/" + (played[i].minutes < 1 ? "1" : played[i].minutes.ToString()));
                }

                if (manual)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append("manual");
                }

                return sb.ToString();
            }

            public void Populate(SteamItemDef_t itemDefId)
            {
                owns = new List<AppId_t>();
                achievements = new List<string>();
                played = new List<PlayedEntry>();
                manual = false;

                var results = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "promo");

                if (!string.IsNullOrEmpty(results))
                {
                    var rules = results.Split(';');
                    foreach (var rule in rules)
                    {
                        if (rule.StartsWith("owns:"))
                            owns.Add(new AppId_t(uint.Parse(rule.Replace("owns:", string.Empty))));
                        else if (rule.StartsWith("ach:"))
                            achievements.Add(rule.Replace("ach:", string.Empty));
                        else if (rule.StartsWith("played:"))
                        {
                            var split = rule.Replace("played:", string.Empty).Split('/');
                            if (split.Length > 1)
                                played.Add(new PlayedEntry
                                {
                                    app = new AppId_t(uint.Parse(split[0])),
                                    minutes = uint.Parse(split[1])
                                });
                            else
                                played.Add(new PlayedEntry
                                {
                                    app = new AppId_t(uint.Parse(split[0]))
                                });
                        }
                    }
                }
            }
        }
        /// <summary>
        /// An internal struct which represents an exchange recipe
        /// </summary>
        [Serializable]
        public struct ExchangeRecipe
        {
            [Serializable]
            public struct Material
            {
                [Serializable]
                public struct ItemDefDescriptor
                {
                    public int item;
                    public uint count;

                    public override string ToString()
                    {
                        if (count > 1)
                        {
                            return item.ToString() + "x" + count.ToString();
                        }
                        else
                            return item.ToString();
                    }
                }
                [Serializable]
                public struct ItemTagDescriptor
                {
                    public string name;
                    public string value;
                    public uint count;

                    public override string ToString()
                    {
                        if (count > 1)
                        {
                            return name + ":" + value + "*" + count.ToString();
                        }
                        else
                            return name + ":" + value;
                    }
                }

                public ItemDefDescriptor item;
                public ItemTagDescriptor tag;

                public override string ToString()
                {
                    if (!Valid)
                        return string.Empty;

                    if (!string.IsNullOrEmpty(tag.name))
                        return item.ToString() + "," + tag.ToString();
                    else if (!string.IsNullOrEmpty(tag.name))
                        return tag.ToString();
                    else
                        return item.ToString();
                }

                public bool Valid
                {
                    get
                    {
                        //If an item is referenced
                        if (item.item > 0)
                        {
                            //Make sure we don't define a tag
                            if (!string.IsNullOrEmpty(tag.name) || !string.IsNullOrEmpty(tag.value) || tag.count != 0)
                            {
                                return false;
                            }
                            else if (item.count == 0)
                            {
                                return false;
                            }
                            else
                                return true;
                        }
                        else
                        {
                            //We are not an item reference so we must be a valid tag reference
                            if (string.IsNullOrEmpty(tag.name) || string.IsNullOrEmpty(tag.value) || tag.count == 0)
                            {
                                return false;
                            }
                            else
                                return true;
                        }
                    }
                }
            }

            public List<Material> materials;

            public bool Valid => materials != null && materials.All(p => p.Valid);

            public string GetSchema()
            {
                if (materials.Count > 1)
                {
                    StringBuilder sb = new StringBuilder(materials[0].ToString());
                    for (int i = 1; i < materials.Count; i++)
                    {
                        sb.Append("," + materials[i].ToString());
                    }

                    return sb.ToString();
                }
                else if (materials.Count == 1)
                    return materials[0].ToString();
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// An internal class which represents the exchange collection of an item
        /// </summary>
        [Serializable]
        public class ExchangeCollection
        {
            public List<ExchangeRecipe> recipe = new List<ExchangeRecipe>();

            public override string ToString()
            {
                if (recipe.Count == 0)
                    return string.Empty;

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < recipe.Count; i++)
                {
                    if (recipe[i].Valid)
                    {
                        if (sb.Length > 0)
                            sb.Append(";");

                        sb.Append(recipe[i].GetSchema());
                    }
                }

                return sb.ToString();
            }
        }
        /// <summary>
        /// An internal class which represents price
        /// </summary>
        [Serializable]
        public struct Price
        {
            [Serializable]
            public struct Value
            {
                public string currency;
                public uint value;

                public bool Valid
                {
                    get
                    {
                        if (string.IsNullOrEmpty(currency.Trim()))
                            return false;
                        if (value < 1)
                            return false;

                        return true;
                    }
                }

                public override string ToString()
                {
                    return currency.ToUpper().Trim() + value.ToString();
                }
            }

            [Serializable]
            public struct PriceList
            {
                [Serializable]
                public struct PriceCollection
                {
                    public Value[] values;

                    public bool Valid
                    {
                        get
                        {
                            if (values is { Length: < 1 })
                                return false;

                            return values != null && values.All(p => p.Valid);
                        }
                    }

                    public override string ToString()
                    {
                        if (!Valid)
                            return string.Empty;

                        StringBuilder sb = new();

                        foreach (var t in values)
                        {
                            if (sb.Length > 0)
                                sb.Append(",");

                            sb.Append(t.ToString());
                        }

                        return sb.ToString();
                    }
                }

                [Serializable]
                public struct ChangeCollection
                {
                    public string fromDate;
                    public string untilDate;
                    public PriceCollection prices;

                    public bool Valid => !string.IsNullOrEmpty(fromDate.Trim()) && !string.IsNullOrEmpty(untilDate.Trim());

                    public override string ToString()
                    {
                        if (!Valid)
                            return string.Empty;

                        return fromDate + "-" + untilDate + prices.ToString();
                    }
                }

                public PriceCollection original;
                public ChangeCollection[] changes;

                public bool Valid => original.Valid && (changes == null || changes.All(p => p.Valid));

                public override string ToString()
                {
                    if (!Valid)
                        return string.Empty;

                    StringBuilder sb = new StringBuilder();
                    sb.Append(original.ToString());

                    if (changes != null)
                    {
                        foreach (var t in changes)
                        {
                            sb.Append(";");
                            sb.Append(t);
                        }
                    }

                    return sb.ToString();
                }
            }

            public uint version;
            public bool usePricing;
            public bool useCategory;
            public ValvePriceCategories category;
            public PriceList priceList;

            public string Node => useCategory ? "price_category" : "price";

            public bool Valid
            {
                get
                {
                    if (useCategory)
                        return true;
                    else
                        return priceList.Valid;
                }
            }

            public override string ToString()
            {
                if (useCategory)
                    return version.ToString() + ";" + category.ToString();
                else
                    return version.ToString() + ";" + priceList.ToString();
            }

            public void Populate(SteamItemDef_t itemDefId)
            {
                var result = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "price_category");
                if (!string.IsNullOrEmpty(result))
                {
                    usePricing = true;
                    useCategory = true;
                    var entries = result.Split(';');
                    if (entries.Length > 1)
                    {
                        version = uint.Parse(entries[0]);
                        category = (ValvePriceCategories)Enum.Parse(typeof(ValvePriceCategories), entries[1]);
                    }
                    else
                    {
                        version = 1;
                        category = (ValvePriceCategories)Enum.Parse(typeof(ValvePriceCategories), result);
                    }
                    priceList = new PriceList();
                }
                else
                {
                    result = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "price");
                    if (!string.IsNullOrEmpty(result))
                    {
                        usePricing = true;
                        useCategory = false;
                        category = ValvePriceCategories.Vlv100;
                        priceList = new PriceList();

                        var entries = result.Split(';');
                        version = uint.Parse(entries[0]);
                        priceList.original = new PriceList.PriceCollection();
                        var originalValues = entries[1].Split(',');
                        priceList.original.values = new Value[originalValues.Length];
                        for (int i = 0; i < originalValues.Length; i++)
                        {
                            var currency = originalValues[i][..3];
                            var value = uint.Parse(originalValues[i].Replace(currency, string.Empty));
                            priceList.original.values[i] = new Value
                            {
                                currency = currency,
                                value = value,
                            };
                        }
                        if (entries.Length > 3)
                        {
                            priceList.changes = new PriceList.ChangeCollection[entries.Length];
                            if (entries.Length > 2)
                            {
                                for (int i = 2; i < entries.Length; i++)
                                {
                                    var changeData = new PriceList.ChangeCollection();
                                    var change = entries[i].Split(',');
                                    var times = change[0].Split('-');
                                    changeData.fromDate = times[0];
                                    changeData.untilDate = times[1];
                                    changeData.prices = new PriceList.PriceCollection();
                                    for (int ii = 1; ii < change.Length; ii++)
                                    {
                                        var currency = originalValues[i].Substring(0, 3);
                                        var value = uint.Parse(originalValues[i].Replace(currency, string.Empty));
                                        changeData.prices.values[ii] = new Value
                                        {
                                            currency = currency,
                                            value = value,
                                        };
                                    }
                                    priceList.changes[i-2] = changeData;
                                }
                            }
                        }
                    }
                    else
                    {
                        usePricing = false;
                        useCategory = false;
                        category = ValvePriceCategories.Vlv100;
                        priceList = new PriceList();
                    }
                }
            }
        }
        /// <summary>
        /// An internal class representing the colour related to an item
        /// </summary>
        [Serializable]
        public struct Colour
        {
            public bool use;
            public Color color;

            public override string ToString()
            {
                return ColorUtility.ToHtmlStringRGB(color);
            }
        }
        /// <summary>
        /// An internal class representing the items tags
        /// </summary>
        [Serializable]
        public class TagCollection
        {
            public List<ItemTag> tags = new List<ItemTag>();

            public override string ToString()
            {
                if (tags.Count == 0)
                    return string.Empty;

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < tags.Count; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append(tags[i].category.Trim() + ":" + tags[i].tag.Trim());
                }

                return sb.ToString();
            }

            public void Populate(SteamItemDef_t itemDefId)
            {
                tags = new List<ItemTag>();
                var result = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tags");

                if (!string.IsNullOrEmpty(result))
                {
                    var split1 = result.Split(';');

                    foreach (var t in split1)
                    {
                        var tag = t.Split(':');
                        tags.Add(new ItemTag { category = tag[0], tag = tag[1] });
                    }
                }
            }
        }
        /// <summary>
        /// An internal struct representing the tag generator value
        /// </summary>
        [Serializable]
        public struct TagGeneratorValue
        {
            public string value;
            public uint weight;

            public bool Valid => !string.IsNullOrEmpty(value.Trim());

            public override string ToString()
            {
                return value.Trim() + (weight > 0 ? ":" + weight.ToString() : "");
            }
        }
        /// <summary>
        /// An internal class collection of <see cref="TagGeneratorValue"/>
        /// </summary>
        [Serializable]
        public class TagGeneratorValues
        {
            public List<TagGeneratorValue> values = new List<TagGeneratorValue>();

            public bool Valid
            {
                get
                {
                    return values.Count > 0 && values.All(p => p.Valid);
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                foreach (var t in values)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append(t.ToString());
                }

                return sb.ToString();
            }

            public void Populate(SteamItemDef_t itemDefId)
            {
                values = new List<TagGeneratorValue>();
                var result = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generator_values");

                if (!string.IsNullOrEmpty(result))
                {
                    var split1 = result.Split(';');

                    foreach (var t in split1)
                    {
                        var tagId = t.Split(':');
                        values.Add(new TagGeneratorValue { value = tagId[0], weight = uint.Parse(tagId[1]) });
                    }
                }
            }
        }
        /// <summary>
        /// An internal class object representing the extended schema
        /// </summary>
        [Serializable]
        public class ExtendedSchema
        {
            [Serializable]
            public struct Entry
            {
                public string node;
                public string value;

                public override string ToString()
                {
                    return "\"" + node.Trim() + "\": " + "\"" + value.Trim() + "\"";
                }
            }

            public List<Entry> entries = new List<Entry>();

            public override string ToString()
            {
                if (entries.Count < 1)
                    return string.Empty;

                StringBuilder sb = new StringBuilder();
                foreach (var t in entries)
                {
                    if (sb.Length > 0)
                        sb.Append(",\n\t\t");
                    sb.Append(t.ToString());
                }

                return sb.ToString();
            }
        }
        /// <summary>
        /// The inventory item type
        /// </summary>
        public InventoryItemType Type => itemType;        
        /// <summary>
        /// The name of the item
        /// </summary>
        public string Name
        {
            get => itemName.GetSimpleValue();
            set => itemName.value = value;
        }      
        /// <summary>
        /// The description of the item
        /// </summary>
        public string Description => itemDescription.GetSimpleValue();        
        /// <summary>
        /// The display type of the item
        /// </summary>
        public string DisplayType => itemDisplayType.GetSimpleValue();
        /// <summary>
        /// The item's ID
        /// </summary>
        public SteamItemDef_t Id
        {
            get => Data;

            set => Data = value;
        }       
        /// <summary>
        /// The background colour
        /// </summary>
        public Colour BackgroundColour => itemBackgroundColour;        
        /// <summary>
        /// The name colour
        /// </summary>
        public Colour NameColour => itemNameColour;        
        /// <summary>
        /// The Icon URL
        /// </summary>
        public string IconUrl => itemIconURL;        
        /// <summary>
        /// The large icon URL
        /// </summary>
        public string IconUrlLarge => itemIconURLLarge;        
        /// <summary>
        /// Is the item marketable
        /// </summary>
        public bool Marketable => itemMarketable;        
        /// <summary>
        /// Is the item tradable between players
        /// </summary>
        public bool Tradable => itemTradable;        
                       
        /// <summary>
        /// Store tags for the item
        /// </summary>
        public string[] StoreTags => itemStoreTags.ToArray();        
        /// <summary>
        /// Additional store images
        /// </summary>
        public string[] StoreImages => itemStoreImages.ToArray();        
        
        #endregion

    }
}
#endif