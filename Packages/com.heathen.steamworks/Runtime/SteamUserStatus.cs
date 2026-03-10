#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Monitors the status of a Steam user and updates UI elements accordingly.
    /// </summary>
    [ModularComponent(typeof(SteamUserData), "Status", nameof(settings))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserStatus : MonoBehaviour
    {
        /// <summary>
        /// Defines the display options for different user statuses.
        /// </summary>
        [Serializable]
        public class Options
        {
            /// <summary>
            /// Defines the UI references and configuration for a specific status.
            /// </summary>
            [Serializable]
            public class References
            {
                /// <summary>
                /// The icon to display for this status.
                /// </summary>
                public Sprite icon;
                /// <summary>
                /// Should the colour of the icon be set?
                /// </summary>
                public bool setIconColor;
                /// <summary>
                /// The colour to apply to the icon.
                /// </summary>
                public Color iconColor;
                /// <summary>
                /// The message to display for this status.
                /// </summary>
                [Tooltip("You can use %gameName% and it will be replaced with the name of the game the player is currently playing. This is only relevant for In This Game and In Another Game options.")]
                public SteamText message;
                /// <summary>
                /// Should the colour of the message label be set?
                /// </summary>
                public bool setMessageColor;
                /// <summary>
                /// The colour to apply to the message label.
                /// </summary>
                public Color messageColor;

                /// <summary>
                /// Sets the status information on the provided UI elements.
                /// </summary>
                /// <param name="image">The image component to update.</param>
                /// <param name="label">The label component to update.</param>
                /// <param name="gameInfo">Information about the game being played, if any.</param>
                public void Set(Image image, TextMeshProUGUI label, FriendGameInfo_t? gameInfo)
                {
                    if (image != null)
                    {
                        image.gameObject.SetActive(true);
                        image.sprite = icon;
                        if (setIconColor)
                            image.color = iconColor;
                    }

                    label.text = message;

                    if (setMessageColor)
                        label.color = messageColor;
                }
            }

            /// <summary>
            /// Configuration for when the user is playing this game.
            /// </summary>
            [FormerlySerializedAs("InThisGame")] public References inThisGame = new References
            {
                setIconColor = false,
                iconColor = new Color(0.8862f, 0.9960f, 0.7568f, 1),
                message = new("Playing %gameName%"),
                setMessageColor = false,
                messageColor = new Color(0.8862f, 0.9960f, 0.7568f, 1)
            };
            /// <summary>
            /// Configuration for when the user is playing another game.
            /// </summary>
            [FormerlySerializedAs("InAnotherGame")] public References inAnotherGame = new References
            {
                setIconColor = false,
                iconColor = new Color(0.5686f, 0.7607f, 0.3411f, 1),
                message = new("Playing %gameName%"),
                setMessageColor = false,
                messageColor = new Color(0.5686f, 0.7607f, 0.3411f, 1)
            };
            /// <summary>
            /// Configuration for when the user is online.
            /// </summary>
            [FormerlySerializedAs("Online")] public References online = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Online"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
            /// <summary>
            /// Configuration for when the user is offline.
            /// </summary>
            [FormerlySerializedAs("Offline")] public References offline = new References
            {
                setIconColor = false,
                iconColor = new Color(0.887f, 0.887f, 0.887f, 1),
                message = new("Offline"),
                setMessageColor = false,
                messageColor = new Color(0.887f, 0.887f, 0.887f, 1)
            };
            /// <summary>
            /// Configuration for when the user is busy.
            /// </summary>
            [FormerlySerializedAs("Busy")] public References busy = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Busy"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
            /// <summary>
            /// Configuration for when the user is away.
            /// </summary>
            [FormerlySerializedAs("Away")] public References away = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Away"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
            /// <summary>
            /// Configuration for when the user is snoozing.
            /// </summary>
            [FormerlySerializedAs("Snooze")] public References snooze = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Snooze"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
            /// <summary>
            /// Configuration for when the user is looking to trade.
            /// </summary>
            [FormerlySerializedAs("LookingToTrade")] public References lookingToTrade = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Looking to Trade"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
            /// <summary>
            /// Configuration for when the user is looking to play.
            /// </summary>
            [FormerlySerializedAs("LookingToPlay")] public References lookingToPlay = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Looking to Play"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
        }
        /// <summary>
        /// Defines the settings for the SteamUserStatus component.
        /// </summary>
        [Serializable]
        public class Settings
        {
            /// <summary>
            /// The status configuration options.
            /// </summary>
            public Options configuration;
            /// <summary>
            /// The image elements to update.
            /// </summary>
            [Header("Elements")]
            public List<Image> images = new();
            /// <summary>
            /// The label elements to update.
            /// </summary>
            public List<TextMeshProUGUI> labels = new();
        }

        /// <summary>
        /// The settings for this component.
        /// </summary>
        public Settings settings = new();

        private SteamUserData _mData;

        private void Awake()
        {
            _mData = GetComponent<SteamUserData>();
            _mData.onChanged.AddListener(InternalPersonaStateChange);
            SteamTools.Events.OnFriendRichPresenceUpdate += InternalRichPresenceUpdate;
        }

        private void OnDestroy()
        {
            _mData.onChanged.RemoveListener(InternalPersonaStateChange);
            SteamTools.Events.OnFriendRichPresenceUpdate -= InternalRichPresenceUpdate;
        }

        private void InternalRichPresenceUpdate(UserData friend, AppData app)
        {
            Refresh();
        }

        private void InternalPersonaStateChange(UserData friend, EPersonaChange flag)
        {
            Refresh();
        }

        /// <summary>
        /// Refreshes the UI elements based on the current user status.
        /// </summary>
        public void Refresh()
        {
            var max = math.max(settings.images.Count, settings.labels.Count);
            for (int i = 0; i < max; i++)
            {
                Image icon = settings.images.Count > i ? settings.images[i] : null;
                TextMeshProUGUI message = settings.labels.Count > i ? settings.labels[i] : null;

                if (_mData.Data.GetGamePlayed(out var gameInfo))
                {
                    if (gameInfo.Game.IsMe)
                        settings.configuration.inThisGame.Set(icon, message, gameInfo);
                    else
                        settings.configuration.inAnotherGame.Set(icon, message, gameInfo);
                }
                else
                {
                    switch (_mData.Data.State)
                    {
                        case EPersonaState.k_EPersonaStateAway:
                            settings.configuration.away.Set(icon, message, null);
                            break;
                        case EPersonaState.k_EPersonaStateBusy:
                            settings.configuration.busy.Set(icon, message, null);
                            break;
                        case EPersonaState.k_EPersonaStateOnline:
                            settings.configuration.online.Set(icon, message, null);
                            break;
                        case EPersonaState.k_EPersonaStateSnooze:
                            settings.configuration.snooze.Set(icon, message, null);
                            break;
                        case EPersonaState.k_EPersonaStateLookingToPlay:
                            settings.configuration.lookingToPlay.Set(icon, message, null);
                            break;
                        case EPersonaState.k_EPersonaStateLookingToTrade:
                            settings.configuration.lookingToTrade.Set(icon, message, null);
                            break;
                        default:
                            settings.configuration.offline.Set(icon, message, null);
                            break;
                    }
                }
            }
        }
    }
}
#endif