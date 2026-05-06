using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    [Serializable]
    public struct HealthPackage
    {
        public uint HealthAmount { get; set; }
        public uint BatchAmount { get; set; }
    }
}