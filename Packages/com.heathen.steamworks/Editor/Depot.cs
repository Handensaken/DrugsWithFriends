#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using UnityEditor;

namespace Heathen.SteamworksIntegration.Editors
{
    [System.Serializable]
    public class Depot
    {
        public string name;
        public uint id;
        public BuildTarget target;
        public string extension;
    }
}
#endif