using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.AI.Extra
{
    [Serializable]
    public struct UtilityAITarget
    {
        [SerializeField, Tooltip("Used for calculate the effect of the distance to the target")] public ValuePackage distance;
        [SerializeField, Tooltip("Used for calculate the effect of low vs high maxHP")] public ValuePackageStart maxHealth;
        [SerializeField,Tooltip("Used for calculate the effect of currently low vs high HP compared to its maxHP")] public ValuePackageStart health;
        [SerializeField,Min(0)] public float weightPerEnemyTargeting;
    }
}