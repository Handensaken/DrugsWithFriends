using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    [Serializable]
    public struct AngleSpanPackage
    {
        [Range(0,360)]public uint angleStart;
        [Range(0,360)]public uint angleEnd;
    }
}