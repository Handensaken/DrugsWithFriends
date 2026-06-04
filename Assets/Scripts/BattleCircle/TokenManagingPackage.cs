using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    [Serializable]
    public struct TokenManagingPackage
    {
        [Space, SerializeField] public int minTime;
        [Space, SerializeField] public int maxTime;
    }
}