#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    public struct WorkshopItemDataCreateStatus
    {
        public bool HasError;
        public string ErrorMessage;
        public WorkshopItemEditorData Data;
        public CreateItemResult_t? CreateItemResult;
        public SubmitItemUpdateResult_t? SubmitItemUpdateResult;
    }
}
#endif