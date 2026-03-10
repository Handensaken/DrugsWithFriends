#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A custom serializable <see cref="UnityEvent{T0}"/> which handles <see cref="UserAchievementStored_t"/> data.
    /// </summary>
    [Serializable]
    public class UserAchievementStoredEvent : UnityEvent<UserAchievementStored_t>
    { }

    [Serializable]
    public struct UserAchievementStoredData
    {
        public GameData game;
        public bool groupAchievement;
        public string achievementName;
        public uint currentProgress;
        public uint maxProgress;
        
        public UserAchievementStoredData(GameData game, bool groupAchievement, string achievementName, uint currentProgress, uint maxProgress)
        {
            this.game = game;
            this.groupAchievement = groupAchievement;
            this.achievementName = achievementName;
            this.currentProgress = currentProgress;
            this.maxProgress = maxProgress;
        }

        public UserAchievementStoredData(UserAchievementStored_t data)
        {
            game = data.m_nGameID;
            groupAchievement = data.m_bGroupAchievement;
            achievementName = data.m_rgchAchievementName;
            currentProgress = data.m_nCurProgress;
            maxProgress = data.m_nMaxProgress;
        }
    }
}
#endif
