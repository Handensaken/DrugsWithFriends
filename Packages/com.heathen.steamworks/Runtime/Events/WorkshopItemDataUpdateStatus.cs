#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    public struct WorkshopItemDataUpdateStatus
    {
        public bool HasError;
        public string ErrorMessage;
        public WorkshopItemEditorData Data;
        public SubmitItemUpdateResult_t? SubmitItemUpdateResult;
    }
}
#endif