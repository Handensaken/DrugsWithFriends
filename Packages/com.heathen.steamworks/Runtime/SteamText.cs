#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A localised FText like feature inspired by Unreal Engine's localisation model
    /// </summary>
    [Serializable]
    public struct SteamText
    {
#if LOCALIZED
        [FormerlySerializedAs("localized")] 
        [FormerlySerializedAs("Localized")] 
        public UnityEngine.Localization.LocalizedString localised;
#endif
        [FormerlySerializedAs("default")] 
        [FormerlySerializedAs("Default")] 
        public string defaultText;

        public SteamText(string value)
        {
#if LOCALIZED
            localised = new();
#endif
            defaultText = value;
        }

        public readonly string Get()
        {
#if LOCALIZED
            if (localised == null ||
                localised.TableReference.ReferenceType == UnityEngine.Localization.Tables.TableReference.Type.Empty ||
                localised.TableEntryReference.ReferenceType ==
                UnityEngine.Localization.Tables.TableEntryReference.Type.Empty) return defaultText;
            var value = localised.GetLocalizedString();
            return string.IsNullOrEmpty(value) ? defaultText : value;
#else
            return defaultText;
#endif
        }

        public override string ToString() => Get();

        public static implicit operator string(SteamText l) => l.Get();
    }
}
#endif