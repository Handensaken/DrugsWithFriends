#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the attributes of a Steam achievement used to retrieve metadata.
    /// </summary>
    public enum AchievementAttributes
    {
        /// <summary>
        /// Get the name of the achievement
        /// </summary>
        Name,
        /// <summary>
        /// Get the description of the achievement
        /// </summary>
        Desc,
        /// <summary>
        /// Return a value that indicates if the achievement is hidden from users
        /// </summary>
        Hidden,
    }
}
#endif