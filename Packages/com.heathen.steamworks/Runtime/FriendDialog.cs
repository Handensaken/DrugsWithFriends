#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the various dialogue types that can be opened in the Steam Overlay.
    /// </summary>
    public enum FriendDialog
    {
        /// <summary>
        /// Opens the overlay web browser to the specified user or groups profile.
        /// </summary>
        Steamid,
        /// <summary>
        /// Opens a chat window to the specified user or joins the group chat.
        /// </summary>
        Chat,
        /// <summary>
        /// Opens a window to a Steam Trading session that was started with the ISteamEconomy/StartTrade Web API.
        /// </summary>
        Jointrade,
        /// <summary>
        /// Opens the overlay web browser to the specified user's stats.
        /// </summary>
        Stats,
        /// <summary>
        /// Opens the overlay web browser to the specified user's achievements.
        /// </summary>
        Achievements,
        /// <summary>
        /// Opens the overlay in minimal mode, prompting the user to add the target user as a friend.
        /// </summary>
        Friendadd,
        /// <summary>
        /// Opens the overlay in minimal mode, prompting the user to remove the target friend.
        /// </summary>
        Friendremove,
        /// <summary>
        /// Opens the overlay in minimal mode, prompting the user to accept an incoming friend invite.
        /// </summary>
        Friendrequestaccept,
        /// <summary>
        /// Opens the overlay in minimal mode, prompting the user to ignore an incoming friend invite.
        /// </summary>
        Friendrequestignore,
    }
}
#endif