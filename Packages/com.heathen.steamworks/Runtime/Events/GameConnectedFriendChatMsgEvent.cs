#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class GameConnectedFriendChatMsgEvent : UnityEvent<UserData, string, EChatEntryType> { }
}
#endif