using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    [Serializable]
    public struct HealthPackage
    {
        public int HealthAmount { get; set; }
        public int BatchAmount { get; set; }
    }
}