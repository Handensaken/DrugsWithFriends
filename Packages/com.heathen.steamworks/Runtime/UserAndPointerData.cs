#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using System;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
#endif

namespace Heathen.SteamworksIntegration.UI
{
    [Serializable]
    public class UserAndPointerData
    {
        public UserData user;
        public PointerEventData PointerEventData;

        public UserAndPointerData(UserData userData, PointerEventData data)
        {
            user = userData;
            PointerEventData = data;
        }
    }
}
#endif