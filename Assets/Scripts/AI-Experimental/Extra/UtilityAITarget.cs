using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct UtilityAITarget
    {
        [SerializeField] public DistanceValuePackage distance;
        [SerializeField] public HealthValuePackage maxHealth;
        [SerializeField] public HealthValuePackage currentHealth;
    }
}