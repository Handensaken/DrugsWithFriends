#if !DISABLESTEAMWORKS  && (STEAM_161 || STEAM_162 || STEAM_163)
using Steamworks;
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a handle to a Steam Timeline event.
    /// </summary>
    /// <remarks>
    /// Timeline events are used by the Steam Timeline API to mark significant moments in gameplay,
    /// which can then be displayed in the Steam overlay or used for generating clips.
    /// </remarks>
    [Serializable]
    public struct TimelineEventData : IEquatable<TimelineEventHandle_t>, IEquatable<ulong>, IComparable<TimelineEventHandle_t>, IComparable<ulong>
    {
        [SerializeField]
        private TimelineEventHandle_t handle;

        /// <summary>
        /// The underlying Steamworks handle for this timeline event.
        /// </summary>
        public readonly TimelineEventHandle_t Handle => handle;

        /// <summary>
        /// The unique 64-bit identifier for this timeline event.
        /// </summary>
        public readonly ulong Id => handle.m_TimelineEventHandle;

        /// <summary>
        /// Gets the arguments associated with this timeline event from the Steam API.
        /// </summary>
        public readonly TimelineEventDataArguments Arguments => API.Timeline.Client.GetArguments(this);

        #region Boilerplate
        public readonly int CompareTo(TimelineEventData other)
        {
            return Id.CompareTo(other.Id);
        }

        public readonly int CompareTo(TimelineEventHandle_t other)
        {
            return Id.CompareTo(other.m_TimelineEventHandle);
        }

        public readonly int CompareTo(ulong other)
        {
            return Id.CompareTo(other);
        }

        public readonly override string ToString()
        {
            return Id.ToString();
        }

        public readonly bool Equals(TimelineEventData other)
        {
            return Id.Equals(other.Id);
        }

        public readonly bool Equals(TimelineEventHandle_t other)
        {
            return Id.Equals(other.m_TimelineEventHandle);
        }

        public readonly bool Equals(ulong other)
        {
            return Id.Equals(other);
        }

        public readonly override bool Equals(object obj)
        {
            return Id.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(TimelineEventData l, TimelineEventData r) => l.Id == r.Id;
        public static bool operator ==(TimelineEventHandle_t l, TimelineEventData r) => l.m_TimelineEventHandle == r.Id;
        public static bool operator ==(TimelineEventData l, TimelineEventHandle_t r) => l.Id == r.m_TimelineEventHandle;
        public static bool operator !=(TimelineEventData l, TimelineEventData r) => l.Id != r.Id;
        public static bool operator !=(TimelineEventHandle_t l, TimelineEventData r) => l.m_TimelineEventHandle != r.Id;
        public static bool operator !=(TimelineEventData l, TimelineEventHandle_t r) => l.Id != r.m_TimelineEventHandle;

        public static implicit operator TimelineEventData(TimelineEventHandle_t value) => new TimelineEventData { handle = value };
        public static implicit operator ulong(TimelineEventData c) => c.Id;
        public static implicit operator TimelineEventData(ulong id) => new TimelineEventData { handle = new TimelineEventHandle_t(id) };
        public static implicit operator TimelineEventHandle_t(TimelineEventData c) => c.handle;
        #endregion
    }

    /// <summary>
    /// Arguments used when creating or retrieving a Steam Timeline event.
    /// </summary>
    [Serializable]
    public struct TimelineEventDataArguments
    {
        /// <summary>
        /// The title of the event, shown in the Steam overlay.
        /// </summary>
        public string title;
        /// <summary>
        /// A localized description of the event.
        /// </summary>
        public string description;
        /// <summary>
        /// The name of the icon to display for this event.
        /// </summary>
        /// <remarks>
        /// Icons must be uploaded to the Steamworks partner site.
        /// </remarks>
        public string icon;
        /// <summary>
        /// The priority of the event. Higher priority events are more likely to be shown.
        /// </summary>
        public uint priority;
        /// <summary>
        /// The start time of the event in seconds relative to the start of the game session.
        /// </summary>
        public float startSeconds;
        /// <summary>
        /// The duration of the event in seconds.
        /// </summary>
        public float durationSeconds;
        /// <summary>
        /// Specifies if this event should be considered for automatic clip generation.
        /// </summary>
        public ETimelineEventClipPriority possibleClip;
    }
}
#endif