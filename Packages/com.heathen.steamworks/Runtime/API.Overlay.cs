#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides an interface for interacting with the Steam overlay functionality.
    /// This includes various utilities and events to manage and interact with the Steam overlay system.
    /// </summary>
    public static class Overlay
    {
        /// <summary>
        /// Provides functionality for interacting with the Steam overlay client system.
        /// This includes utilities, events, and methods to manage and interact with the overlay features,
        /// such as activating specific dialogues, handling notifications, and responding to game or lobby requests.
        /// </summary>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                _isShowing = false;
            }

            /// <summary>
            /// Indicates whether the Steam Overlay is currently enabled. This property retrieves its value from Steamworks and reflects the status of the overlay functionality.
            /// </summary>
            public static bool IsEnabled => SteamUtils.IsOverlayEnabled();

            /// <summary>
            /// Indicates whether the Steam Overlay is currently visible to the user.
            /// This property reflects the real-time visibility status of the overlay as managed by the Steamworks API.
            /// </summary>
            public static bool IsShowing
            {
                get => _isShowing;
                internal set => _isShowing = value;
            }

            /// <summary>
            /// Specifies the position where Steam Overlay notifications will appear on the screen.
            /// This property interacts with the Steamworks API to configure the location of overlay notifications.
            /// </summary>
            public static ENotificationPosition NotificationPosition
            {
                get => _notificationPosition;
                set
                {
                    _notificationPosition = value;
                    SteamUtils.SetOverlayNotificationPosition(_notificationPosition);
                }
            }

            /// <summary>
            /// Defines the inset offset of overlay notifications in pixels. This property sets and retrieves the position of notifications, allowing customization
            /// of their on-screen placement relative to the top-left of the display.
            /// </summary>
            public static Vector2Int NotificationInset
            {
                get => _notificationInset;
                set
                {
                    _notificationInset = value;
                    SteamUtils.SetOverlayNotificationInset(value.x, value.y);
                }
            }

            private static bool _isShowing = false;
            private static ENotificationPosition _notificationPosition = ENotificationPosition.k_EPositionBottomRight;
            private static Vector2Int _notificationInset = Vector2Int.zero;

            /// <summary>
            /// Activates the Steam Overlay to a specific dialogue.
            /// </summary>
            /// <param name="dialog">The dialogue to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
            public static void Activate(string dialog) => SteamFriends.ActivateGameOverlay(dialog);
            /// <summary>
            /// Activates the Steam Overlay to a specific dialogue.
            /// </summary>
            /// <param name="dialog">The dialogue to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
            public static void Activate(OverlayDialog dialog) => SteamFriends.ActivateGameOverlay(dialog.ToString());
            /// <summary>
            /// Activates the Steam Overlay to open the invite dialogue. Invitations sent from this dialogue will be for the provided lobby.
            /// </summary>
            /// <param name="lobbyId">The Steam ID of the lobby that selected users will be invited to.</param>
            public static void ActivateInviteDialog(LobbyData lobbyId) => SteamFriends.ActivateGameOverlayInviteDialog(lobbyId);

            /// <summary>
            /// Activates the Steam Overlay to display an invitation dialogue based on the provided connection string.
            /// This method is typically used to create connection-based invites, such as direct game connections.
            /// </summary>
            /// <param name="connectionString">The connection string used to establish the invite. This string should
            /// contain the necessary information for the recipient to connect to the specified session.</param>
            public static void ActivateInviteDialog(string connectionString) =>
                SteamFriends.ActivateGameOverlayInviteDialogConnectString(connectionString);

            /// <summary>
            /// Activates the Steam Overlay to display a Remote Play Together invite dialogue for the specified lobby.
            /// This allows the user to invite others to join a Remote Play Together session.
            /// </summary>
            /// <param name="lobbyId">The lobby data identifying the Steam lobby for which the invite dialogue will be shown.</param>
            public static void ActivateRemotePlayInviteDialog(LobbyData lobbyId) =>
                SteamFriends.ActivateGameOverlayRemotePlayTogetherInviteDialog(lobbyId);
            /// <summary>
            /// Activates the Steam Overlay to the Steam store page for the provided app.
            /// </summary>
            /// <param name="appID">The app ID to show the store page of.</param>
            /// <param name="flag">Flags to modify the behaviour when the page opens.</param>
            public static void Activate(AppData appID, EOverlayToStoreFlag flag) => SteamFriends.ActivateGameOverlayToStore(appID, flag);
            /// <summary>
            /// Activates Steam Overlay to a specific dialogue.
            /// </summary>
            /// <param name="dialog">The dialogue to open.</param>
            /// <param name="steamId">The Steam ID of the context to open this dialogue to.</param>
            public static void Activate(string dialog, CSteamID steamId) => SteamFriends.ActivateGameOverlayToUser(dialog, steamId);
            /// <summary>
            /// Activates Steam Overlay to a specific dialogue.
            /// </summary>
            /// <param name="dialog">The dialogue to open.</param>
            /// <param name="steamId">The Steam ID of the context to open this dialogue to.</param>
            public static void Activate(FriendDialog dialog, CSteamID steamId) => SteamFriends.ActivateGameOverlayToUser(dialog.ToString(), steamId);
            /// <summary>
            /// Activates Steam Overlay web browser directly to the specified URL.
            /// </summary>
            /// <param name="url">The webpage to open. (A fully qualified address with the protocol is required, e.g. "http://www.steampowered.com")</param>
            public static void ActivateWebPage(string url) => SteamFriends.ActivateGameOverlayToWebPage(url);
        }
    }
}
#endif