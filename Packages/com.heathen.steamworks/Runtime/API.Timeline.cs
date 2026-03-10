#if !DISABLESTEAMWORKS && (STEAM_161 || STEAM_162 || STEAM_163)
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides a static set of methods and properties to manage and interact with the Steam Timeline API.
    /// The Timeline class serves as an integration point for creating, updating, and managing timeline events and game phases
    /// within the Steamworks framework.
    /// </summary>
    public static class Timeline
    {
        /// <summary>
        /// Provides a set of static methods and properties to create, manage, and interact with timeline events and game phases
        /// through the Steamworks Timeline API. The Client class enables operations such as adding and updating timeline events,
        /// managing game phases, and integrating with the Steam overlay for timeline and game phase visualisation.
        /// </summary>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RunTimeInit()
            {
                TimelineEvents.Clear();
                MTimelineEventDataArguments.Clear();
                _mSteamTimelineEventRecordingExistsT = null;
                _mSteamTimelineGamePhaseRecordingExistsT = null;
            }

            /// <summary>
            /// A static readonly list that holds all timeline events currently registered within the system.
            /// Each element represents an event stored as <see cref="TimelineEventData"/>
            /// and provides details such as the event's title, description, icon, priority, start time, and duration.
            /// </summary>
            public static readonly List<TimelineEventData> TimelineEvents = new();

            private static readonly Dictionary<ulong, TimelineEventDataArguments> MTimelineEventDataArguments = new();

            private static CallResult<SteamTimelineEventRecordingExists_t> _mSteamTimelineEventRecordingExistsT;
            private static CallResult<SteamTimelineGamePhaseRecordingExists_t> _mSteamTimelineGamePhaseRecordingExistsT;

            /// <summary>
            /// Retrieves the arguments associated with the specified timeline event.
            /// </summary>
            /// <param name="timelineEvent">The timeline event for which arguments need to be retrieved.</param>
            /// <returns>The arguments associated with the specified timeline event, or null if no arguments are available.</returns>
            public static TimelineEventDataArguments GetArguments(TimelineEventData timelineEvent)
            {
                return MTimelineEventDataArguments.GetValueOrDefault(timelineEvent);
            }

            /// <summary>
            /// Sets a tooltip description for the timeline, providing context or additional information about the current game state.
            /// This helps users reference specific moments in a timeline when reviewing or saving clips.
            /// </summary>
            /// <param name="description">The textual description to be displayed as a tooltip for the timeline event.</param>
            /// <param name="timeDelta">The time offset, in seconds, to associate with this state. Negative values indicate a past event relative to the current time.</param>
            public static void SetTimelineTooltip(string description, float timeDelta) =>
                SteamTimeline.SetTimelineTooltip(description, timeDelta);

            /// <summary>
            /// Clears the tooltip information previously set in the timeline.
            /// </summary>
            /// <param name="timeDelta">The time offset in seconds to associate with the tooltip clearance. Negative values represent past events relative to the current timeline state.</param>
            public static void ClearTimelineTooltip(float timeDelta) => SteamTimeline.ClearTimelineTooltip(timeDelta);

            /// <summary>
            /// Adds an instantaneous timeline event with the specified details.
            /// </summary>
            /// <param name="title">The title of the timeline event.</param>
            /// <param name="description">A brief description of the timeline event.</param>
            /// <param name="icon">The icon representing the timeline event.</param>
            /// <param name="priority">The priority level of the timeline event.</param>
            /// <param name="startOffsetSeconds">The offset in seconds before the event starts relative to the current time.</param>
            /// <param name="possibleClip">The clip priority indicating possible overlap handling for this timeline event.</param>
            /// <returns>A <see cref="TimelineEventData"/> object representing the added timeline event.</returns>
            public static TimelineEventData AddInstantaneousTimelineEvent(string title, string description, string icon,
                uint priority, float startOffsetSeconds, ETimelineEventClipPriority possibleClip)
            {
                var handle = SteamTimeline.AddInstantaneousTimelineEvent(title, description, icon, priority,
                    startOffsetSeconds, possibleClip);
                TimelineEvents.Add(handle);
                MTimelineEventDataArguments.TryAdd(handle.m_TimelineEventHandle, new()
                {
                    title = title,
                    description = description,
                    icon = icon,
                    priority = priority,
                    startSeconds = Time.time + startOffsetSeconds,
                    durationSeconds = 0,
                    possibleClip = possibleClip
                });
                return handle;
            }

            /// <summary>
            /// Adds a new timeline event with the specified parameters to the Steam timeline.
            /// </summary>
            /// <param name="title">The title of the timeline event.</param>
            /// <param name="description">A detailed description of the timeline event.</param>
            /// <param name="icon">The icon associated with the timeline event.</param>
            /// <param name="priority">The priority of the timeline event.</param>
            /// <param name="startOffsetSeconds">The time offset in seconds from the current time when the event starts.</param>
            /// <param name="durationSeconds">The duration of the event in seconds.</param>
            /// <param name="possibleClip">The clip priority associated with the timeline event.</param>
            /// <returns>An object representing the timeline event data created with the specified parameters.</returns>
            public static TimelineEventData AddRangeTimelineEvent(string title, string description, string icon,
                uint priority, float startOffsetSeconds, float durationSeconds, ETimelineEventClipPriority possibleClip)
            {
                var handle = SteamTimeline.AddRangeTimelineEvent(title, description, icon, priority, startOffsetSeconds,
                    durationSeconds, possibleClip);
                TimelineEvents.Add(handle);
                MTimelineEventDataArguments.TryAdd(handle.m_TimelineEventHandle, new()
                {
                    title = title,
                    description = description,
                    icon = icon,
                    priority = priority,
                    startSeconds = Time.time + startOffsetSeconds,
                    durationSeconds = durationSeconds,
                    possibleClip = possibleClip
                });
                return handle;
            }

            /// <summary>
            /// Starts a range-based timeline event with the specified parameters.
            /// </summary>
            /// <param name="title">The title of the timeline event.</param>
            /// <param name="description">The description of the timeline event.</param>
            /// <param name="icon">The icon associated with the timeline event.</param>
            /// <param name="priority">The priority level of the timeline event.</param>
            /// <param name="startOffsetSeconds">The offset in seconds from the current time at which the event starts.</param>
            /// <param name="possibleClip">The clip priority level indicating how the event interacts with potential timeline overlaps.</param>
            /// <returns>The timeline event data representing the newly created event.</returns>
            public static TimelineEventData StartRangeTimelineEvent(string title, string description, string icon,
                uint priority, float startOffsetSeconds, ETimelineEventClipPriority possibleClip)
            {
                var handle = SteamTimeline.StartRangeTimelineEvent(title, description, icon, priority,
                    startOffsetSeconds, possibleClip);
                TimelineEvents.Add(handle);
                MTimelineEventDataArguments.TryAdd(handle.m_TimelineEventHandle, new()
                {
                    title = title,
                    description = description,
                    icon = icon,
                    priority = priority,
                    startSeconds = Time.time + startOffsetSeconds,
                    durationSeconds = -1,
                    possibleClip = possibleClip
                });
                return handle;
            }

            /// <summary>
            /// Updates the details of an existing timeline event, such as its title, description, icon, priority, and possible video clip suggestion.
            /// </summary>
            /// <param name="timelineEvent">The timeline event to update.</param>
            /// <param name="title">The new title to display for the timeline event.</param>
            /// <param name="description">The updated description to associate with the timeline event.</param>
            /// <param name="icon">The name of the icon to display for the timeline event. It can be a custom or standard Steam-provided icon.</param>
            /// <param name="priority">The priority value that determines the prominence of the event's icon on the timeline. Higher values indicate greater prominence.</param>
            /// <param name="possibleClip">Specifies whether this event should be suggested to the user as a potential video clip.</param>
            public static void UpdateRangeTimelineEvent(TimelineEventData timelineEvent, string title,
                string description, string icon, uint priority, ETimelineEventClipPriority possibleClip)
            {
                SteamTimeline.UpdateRangeTimelineEvent(timelineEvent, title, description, icon, priority, possibleClip);

                if (!MTimelineEventDataArguments.TryGetValue(timelineEvent, out var handle)) return;
                handle.title = title;
                handle.description = description;
                handle.icon = icon;
                handle.priority = priority;
                handle.possibleClip = possibleClip;
                MTimelineEventDataArguments[timelineEvent] = handle;
            }

            /// <summary>
            /// Marks the conclusion of a range timeline event that was initiated using the corresponding start method.
            /// </summary>
            /// <param name="timelineEvent">The timeline event to be ended.</param>
            /// <param name="endOffsetSeconds">The time offset in seconds to be applied to the end of the event. A negative value indicates that the event ended in the past.</param>
            public static void EndRangeTimelineEvent(TimelineEventData timelineEvent, float endOffsetSeconds)
            {
                SteamTimeline.EndRangeTimelineEvent(timelineEvent, endOffsetSeconds);
                if (MTimelineEventDataArguments.TryGetValue(timelineEvent, out var handle))
                {
                    var endTime = Time.time + endOffsetSeconds;
                    handle.durationSeconds = endTime - handle.startSeconds;
                    MTimelineEventDataArguments[timelineEvent] = handle;
                }
            }

            /// <summary>
            /// Removes a specified timeline event from the timeline, including its arguments and associated data.
            /// </summary>
            /// <param name="timelineEvent">The timeline event to be removed.</param>
            public static void RemoveTimelineEvent(TimelineEventData timelineEvent)
            {
                TimelineEvents.Remove(timelineEvent);
                MTimelineEventDataArguments.Remove(timelineEvent);
                SteamTimeline.RemoveTimelineEvent(timelineEvent);
            }

            /// <summary>
            /// Determines whether a video recording exists for the specified timeline event.
            /// </summary>
            /// <param name="timelineEvent">The event data for which to check the existence of a video recording.</param>
            /// <param name="callback">The callback to invoke with the result of the check. The boolean parameter indicates whether the recording exists.</param>
            public static void DoesEventRecordingExist(TimelineEventData timelineEvent, Action<bool> callback)
            {
                if (callback == null)
                    return;

                _mSteamTimelineEventRecordingExistsT ??= CallResult<SteamTimelineEventRecordingExists_t>.Create();

                var handle = SteamTimeline.DoesEventRecordingExist(timelineEvent);
                _mSteamTimelineEventRecordingExistsT.Set(handle, (r, _) => { callback.Invoke(r.m_bRecordingExists); });
            }

            /// <summary>
            /// Starts a game phase to define a section of gameplay for background recordings and clips.
            /// Game phases are segments of gameplay that help users organise and navigate their recordings, typically spanning
            /// between 10 minutes to a few hours, depending on the game's structure. These segments are shown in a user interface
            /// grouped by the date the game was played, allowing users to easily revisit specific parts of their gameplay.
            /// This feature is useful for marking key points in a user's gaming session that might be of interest for future viewing.
            /// </summary>
            public static void StartGamePhase() => SteamTimeline.StartGamePhase();

            /// <summary>
            /// Ends the current game phase and finalises any ongoing timeline activities related to it.
            /// This method should be used to signal the completion of a game phase in the Steam Timeline system.
            /// </summary>
            public static void EndGamePhase() => SteamTimeline.EndGamePhase();

            /// <summary>
            /// Sets the identifier for the current game phase.
            /// </summary>
            /// <param name="id">The unique identifier for the game phase to set.</param>
            public static void SetGamePhaseId(string id) => SteamTimeline.SetGamePhaseID(id);

            /// <summary>
            /// Checks if video recordings exist for the specified game phase.
            /// Steam will provide the result through a SteamTimelineGamePhaseRecordingExists_t callback.
            /// This method is useful for determining whether to display controls
            /// that interact with game phase recordings, such as calling OpenOverlayToGamePhase.
            /// </summary>
            /// <param name="id">The identifier for the game phase to check.</param>
            /// <param name="callback">The callback invoked with the results from Steam.</param>
            public static void DoesGamePhaseRecordingExist(string id,
                Action<SteamTimelineGamePhaseRecordingExists_t> callback)
            {
                if (callback == null)
                    return;

                _mSteamTimelineGamePhaseRecordingExistsT ??=
                    CallResult<SteamTimelineGamePhaseRecordingExists_t>.Create();

                var handle = SteamTimeline.DoesGamePhaseRecordingExist(id);
                _mSteamTimelineGamePhaseRecordingExistsT.Set(handle, (r, _) => { callback.Invoke(r); });
            }

            /// <summary>
            /// Adds a new game phase tag to the timeline with the specified parameters.
            /// </summary>
            /// <param name="tagName">The name of the tag to add.</param>
            /// <param name="tagIcon">The icon associated with the tag.</param>
            /// <param name="tagGroup">The group to which the tag belongs.</param>
            /// <param name="priority">The priority level of the tag.</param>
            public static void AddGamePhaseTag(string tagName, string tagIcon, string tagGroup, uint priority) =>
                SteamTimeline.AddGamePhaseTag(tagName, tagIcon, tagGroup, priority);

            /// <summary>
            /// Adds or updates a game phase attribute, which represents generic text-based metadata for a game phase.
            /// Attributes can be updated multiple times, with only the latest value being displayed to the user.
            /// This feature can be used for metadata such as a KDA (Kills/Deaths/Assists) score
            /// or other dynamic information that changes throughout the phase.
            /// </summary>
            /// <param name="attributeGroup">The identifier for the attribute group being updated, such as "KDA" or "CharacterName".</param>
            /// <param name="attributeValue">The current value of the attribute to display, such as "0/0/0" or a character name.</param>
            /// <param name="priority">The priority level of the attribute, determining its importance during display.</param>
            public static void SetGamePhaseAttribute(string attributeGroup, string attributeValue, uint priority) =>
                SteamTimeline.SetGamePhaseAttribute(attributeGroup, attributeValue, priority);

            /// <summary>
            /// Sets the game mode for the timeline, which determines the colour of the timeline bar segments.
            /// </summary>
            /// <param name="mode">The game mode to set, represented by the <see cref="ETimelineGameMode"/> enumeration.</param>
            public static void SetTimelineGameMode(ETimelineGameMode mode) => SteamTimeline.SetTimelineGameMode(mode);

            /// <summary>
            /// Opens the Steam overlay to the section of the timeline associated with the specified game phase.
            /// </summary>
            /// <param name="phaseId">The unique identifier of the game phase to open in the Steam overlay.</param>
            public static void OpenOverlayToGamePhase(string phaseId) => SteamTimeline.OpenOverlayToGamePhase(phaseId);

            /// <summary>
            /// Opens the Steam overlay to the section of the timeline associated with the specified timeline event.
            /// The timeline event must belong to the current game session, as timeline event data cannot be reused across different game sessions.
            /// </summary>
            /// <param name="timelineEvent">The timeline event for which the overlay should be opened.</param>
            public static void OpenOverlayToTimelineEvent(TimelineEventData timelineEvent) =>
                SteamTimeline.OpenOverlayToTimelineEvent(timelineEvent);
        }
    }
}
#endif